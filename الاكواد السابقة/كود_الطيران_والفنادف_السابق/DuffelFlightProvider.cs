using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

/// <summary>
/// Duffel NDC-style airline offers. Falls back to demo when API key is missing or call fails.
/// </summary>
public class DuffelFlightProvider : IFlightProvider
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly FlightProviderOptions _options;
    private readonly DemoFlightProvider _demo;
    private readonly IFlightOfferCache _cache;
    private readonly ICurrencyService _fx;
    private readonly ILogger<DuffelFlightProvider> _logger;

    public DuffelFlightProvider(
        IHttpClientFactory httpFactory,
        IOptions<FlightProviderOptions> options,
        DemoFlightProvider demo,
        IFlightOfferCache cache,
        ICurrencyService fx,
        ILogger<DuffelFlightProvider> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _demo = demo;
        _cache = cache;
        _fx = fx;
        _logger = logger;
    }

    private HttpClient CreateClient()
    {
        var http = _httpFactory.CreateClient("duffel");
        http.BaseAddress = new Uri(_options.DuffelBaseUrl.TrimEnd('/') + "/");
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!http.DefaultRequestHeaders.Contains("Duffel-Version"))
            http.DefaultRequestHeaders.TryAddWithoutValidation("Duffel-Version", _options.DuffelVersion);
        if (!string.IsNullOrWhiteSpace(_options.DuffelApiKey))
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.DuffelApiKey);
        return http;
    }

    public string Name => HasApiKey ? "duffel" : "demo";
    public bool IsLive => true;
    private bool HasApiKey => !string.IsNullOrWhiteSpace(_options.DuffelApiKey);

    public async Task<IReadOnlyList<FlightOfferDto>> SearchAsync(FlightSearchRequest request, CancellationToken ct = default)
    {
        if (!HasApiKey)
        {
            return await _demo.SearchAsync(request, ct);
        }

        var from = AirportCatalog.CodeOrEmpty(request.From);
        var to = AirportCatalog.CodeOrEmpty(request.To);
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
        {
            return await _demo.SearchAsync(request, ct);
        }

        var date = (request.Date ?? DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd");
        var passengers = Math.Max(1, request.Passengers);
        var cabin = MapCabin(request.CabinClass);

        var payload = new
        {
            data = new
            {
                slices = new[]
                {
                    new { origin = from, destination = to, departure_date = date }
                },
                passengers = Enumerable.Range(0, passengers).Select(_ => new { type = "adult" }).ToArray(),
                cabin_class = cabin
            }
        };

        try
        {
            using var http = CreateClient();
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await http.PostAsync("air/offer_requests", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Duffel search failed ({Status}): {Body}", (int)response.StatusCode, Truncate(body));
                return await _demo.SearchAsync(request, ct);
            }

            var offers = ParseOffers(body, from, to);
            if (offers.Count == 0)
            {
                _logger.LogInformation("Duffel returned no offers; using demo inventory for {From}-{To}", from, to);
                return await _demo.SearchAsync(request, ct);
            }

            _cache.Upsert(offers);
            return offers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Duffel search error");
            return await _demo.SearchAsync(request, ct);
        }
    }

    public async Task<LiveBookingResult> BookAsync(LiveBookingRequest request, FlightOfferDto offer, CancellationToken ct = default)
    {
        if (!HasApiKey || offer.Source == "demo" || offer.OfferId.StartsWith("off_demo", StringComparison.OrdinalIgnoreCase))
        {
            return await _demo.BookAsync(request, offer, ct);
        }

        try
        {
            var names = SplitName(request.PassengerName);
            var payload = new
            {
                data = new
                {
                    selected_offers = new[] { offer.OfferId },
                    passengers = new[]
                    {
                        new
                        {
                            phone_number = NormalizePhone(request.PassengerPhone),
                            email = request.PassengerEmail,
                            title = request.Gender.Equals("f", StringComparison.OrdinalIgnoreCase) ? "ms" : "mr",
                            gender = request.Gender.Equals("f", StringComparison.OrdinalIgnoreCase) ? "f" : "m",
                            born_on = (request.DateOfBirth ?? new DateTime(1990, 1, 1)).ToString("yyyy-MM-dd"),
                            family_name = names.Last,
                            given_name = names.First,
                            infant_passenger_id = (string?)null
                        }
                    },
                    type = "instant",
                    payments = new[]
                    {
                        new
                        {
                            type = "balance",
                            amount = offer.Price.ToString("0.00"),
                            currency = string.IsNullOrWhiteSpace(offer.Currency) ? "USD" : offer.Currency
                        }
                    }
                }
            };

            using var http = CreateClient();
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await http.PostAsync("air/orders", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Duffel booking failed ({Status}): {Body}", (int)response.StatusCode, Truncate(body));
                return new LiveBookingResult
                {
                    Success = false,
                    Source = Name,
                    Message = "تعذر إتمام الحجز مع شركة الطيران. حاول مرة أخرى أو اختر عرضاً آخر."
                };
            }

            using var doc = JsonDocument.Parse(body);
            var data = doc.RootElement.GetProperty("data");
            var orderId = data.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
            var bookingRef = data.TryGetProperty("booking_reference", out var refProp)
                ? refProp.GetString() ?? orderId
                : orderId;

            return new LiveBookingResult
            {
                Success = true,
                BookingReference = string.IsNullOrWhiteSpace(bookingRef) ? orderId : bookingRef,
                ExternalOrderId = orderId,
                Status = "Confirmed",
                Source = Name,
                TotalPrice = offer.Price * Math.Max(1, request.NumberOfSeats),
                Currency = offer.Currency,
                Message = "تم إصدار التذكرة مباشرة عبر شبكة شركات الطيران (Duffel/NDC)",
                Offer = offer
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Duffel booking error");
            return new LiveBookingResult
            {
                Success = false,
                Source = Name,
                Message = "حدث خطأ أثناء الربط مع شركة الطيران"
            };
        }
    }

    public async Task<bool> CancelAsync(string externalOrderId, CancellationToken ct = default)
    {
        if (!HasApiKey || externalOrderId.StartsWith("DUF-DEMO", StringComparison.OrdinalIgnoreCase))
        {
            return await _demo.CancelAsync(externalOrderId, ct);
        }

        try
        {
            using var http = CreateClient();
            using var content = new StringContent(
                JsonSerializer.Serialize(new { data = new { order_id = externalOrderId } }),
                Encoding.UTF8,
                "application/json");
            using var response = await http.PostAsync("air/order_cancellations", content, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Duffel cancel error for {OrderId}", externalOrderId);
            return false;
        }
    }

    private List<FlightOfferDto> ParseOffers(string body, string from, string to)
    {
        var results = new List<FlightOfferDto>();
        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("data", out var data)) return results;

        JsonElement offersEl;
        if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("offers", out offersEl))
        {
            // offer_requests response
        }
        else if (data.ValueKind == JsonValueKind.Array)
        {
            offersEl = data;
        }
        else
        {
            return results;
        }

        AirportCatalog.TryResolve(from, out _, out var fromCity);
        AirportCatalog.TryResolve(to, out _, out var toCity);

        foreach (var offer in offersEl.EnumerateArray().Take(30))
        {
            try
            {
                var offerId = offer.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N");
                var amount = decimal.Parse(offer.GetProperty("total_amount").GetString() ?? "0");
                var currency = offer.GetProperty("total_currency").GetString() ?? "USD";
                var seats = offer.TryGetProperty("available_services", out _) ? 9 : 20;

                string airline = "Airline";
                string airlineCode = "";
                string flightNumber = "";
                string aircraft = "";
                DateTime dep = DateTime.UtcNow;
                DateTime arr = DateTime.UtcNow;
                var stops = 0;

                if (offer.TryGetProperty("slices", out var slices) && slices.GetArrayLength() > 0)
                {
                    var slice = slices[0];
                    if (slice.TryGetProperty("segments", out var segments) && segments.GetArrayLength() > 0)
                    {
                        stops = Math.Max(0, segments.GetArrayLength() - 1);
                        var first = segments[0];
                        var last = segments[segments.GetArrayLength() - 1];

                        dep = DateTime.Parse(first.GetProperty("departing_at").GetString()!);
                        arr = DateTime.Parse(last.GetProperty("arriving_at").GetString()!);

                        if (first.TryGetProperty("marketing_carrier", out var carrier))
                        {
                            airline = carrier.TryGetProperty("name", out var n) ? n.GetString() ?? airline : airline;
                            airlineCode = carrier.TryGetProperty("iata_code", out var c) ? c.GetString() ?? "" : "";
                        }

                        flightNumber = first.TryGetProperty("marketing_carrier_flight_number", out var fn)
                            ? $"{airlineCode}{fn.GetString()}"
                            : airlineCode;

                        if (first.TryGetProperty("aircraft", out var ac) && ac.ValueKind != JsonValueKind.Null &&
                            ac.TryGetProperty("name", out var acn))
                        {
                            aircraft = acn.GetString() ?? "";
                        }
                    }
                }

                // Keep original network price; convert to SDG for canonical booking storage
                var displayPrice = currency.Equals("SDG", StringComparison.OrdinalIgnoreCase)
                    ? amount
                    : _fx.ToSdg(amount, currency);

                results.Add(new FlightOfferDto
                {
                    OfferId = offerId,
                    Source = "duffel",
                    FlightNumber = string.IsNullOrWhiteSpace(flightNumber) ? airlineCode : flightNumber,
                    Airline = airline,
                    AirlineCode = airlineCode,
                    DepartureCity = fromCity,
                    ArrivalCity = toCity,
                    DepartureAirport = from,
                    ArrivalAirport = to,
                    DepartureTime = dep,
                    ArrivalTime = arr,
                    Price = displayPrice,
                    Currency = "SDG",
                    OriginalPrice = amount,
                    OriginalCurrency = currency.ToUpperInvariant(),
                    AvailableSeats = seats,
                    AircraftType = aircraft,
                    Stops = stops,
                    CabinClass = "economy",
                    LastUpdatedUtc = DateTime.UtcNow,
                    BookableDirect = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Skipping malformed Duffel offer");
            }
        }

        return results;
    }

    private static string MapCabin(string cabin) => cabin.ToLowerInvariant() switch
    {
        "business" => "business",
        "first" => "first",
        "premium" or "premium_economy" => "premium_economy",
        _ => "economy"
    };

    private static (string First, string Last) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return ("Passenger", "Guest");
        if (parts.Length == 1) return (parts[0], parts[0]);
        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static string NormalizePhone(string phone)
    {
        var p = phone.Trim();
        if (!p.StartsWith('+')) p = "+249" + p.TrimStart('0');
        return p;
    }

    private static string Truncate(string s) => s.Length <= 400 ? s : s[..400];
}
