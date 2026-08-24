using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;
using Microsoft.Extensions.Options;

namespace SudanTravelApp.API.Services;

/// <summary>
/// Realistic live-like inventory when no Duffel key is configured.
/// Simulates airline-direct offers that refresh automatically.
/// </summary>
public class DemoFlightProvider : IFlightProvider
{
    private readonly FlightProviderOptions _options;
    private readonly IFlightOfferCache _cache;
    private readonly Random _random = new();

    private static readonly (string Code, string Name, string Aircraft)[] Airlines =
    [
        ("SD", "الخطوط الجوية السودانية", "Boeing 737-800"),
        ("BDR", "بدر للطيران", "Embraer E190"),
        ("TRK", "تاركو للطيران", "ATR 72"),
        ("MS", "مصر للطيران", "Airbus A320"),
        ("EK", "طيران الإمارات", "Boeing 777-300ER"),
        ("QR", "الخطوط القطرية", "Airbus A350"),
        ("TK", "الخطوط التركية", "Boeing 737-900"),
        ("ET", "الخطوط الإثيوبية", "Boeing 737-800"),
        ("SV", "السعودية", "Airbus A321"),
        ("FZ", "فلاي دبي", "Boeing 737-800")
    ];

    public DemoFlightProvider(IOptions<FlightProviderOptions> options, IFlightOfferCache cache)
    {
        _options = options.Value;
        _cache = cache;
    }

    public string Name => "demo";
    public bool IsLive => true;

    public Task<IReadOnlyList<FlightOfferDto>> SearchAsync(FlightSearchRequest request, CancellationToken ct = default)
    {
        var fromCode = AirportCatalog.CodeOrEmpty(request.From);
        var toCode = AirportCatalog.CodeOrEmpty(request.To);
        var date = (request.Date ?? DateTime.Today.AddDays(1)).Date;
        var passengers = Math.Max(1, request.Passengers);

        IEnumerable<FlightOfferDto> offers;

        if (!string.IsNullOrEmpty(fromCode) && !string.IsNullOrEmpty(toCode))
        {
            offers = BuildRouteOffers(fromCode, toCode, date, passengers, request.CabinClass);
        }
        else
        {
            offers = _cache.GetAll();
            if (!string.IsNullOrEmpty(fromCode))
                offers = offers.Where(o => o.DepartureAirport.Equals(fromCode, StringComparison.OrdinalIgnoreCase)
                    || o.DepartureCity.Contains(request.From ?? "", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(toCode))
                offers = offers.Where(o => o.ArrivalAirport.Equals(toCode, StringComparison.OrdinalIgnoreCase)
                    || o.ArrivalCity.Contains(request.To ?? "", StringComparison.OrdinalIgnoreCase));
            if (request.Date.HasValue)
                offers = offers.Where(o => o.DepartureTime.Date == date);
        }

        var list = offers.OrderBy(o => o.Price).Take(40).ToList();
        _cache.Upsert(list);
        return Task.FromResult<IReadOnlyList<FlightOfferDto>>(list);
    }

    public Task<LiveBookingResult> BookAsync(LiveBookingRequest request, FlightOfferDto offer, CancellationToken ct = default)
    {
        var seats = Math.Max(1, request.NumberOfSeats);
        if (offer.AvailableSeats < seats)
        {
            return Task.FromResult(new LiveBookingResult
            {
                Success = false,
                Message = "عدد المقاعد المتاحة غير كافٍ لدى شركة الطيران",
                Source = Name
            });
        }

        offer.AvailableSeats -= seats;
        _cache.Upsert([offer]);

        var orderId = $"DUF-DEMO-{DateTime.UtcNow:yyyyMMddHHmmss}-{_random.Next(1000, 9999)}";
        var reference = $"WN{DateTime.UtcNow:yyMMdd}{_random.Next(100000, 999999)}";

        return Task.FromResult(new LiveBookingResult
        {
            Success = true,
            BookingReference = reference,
            ExternalOrderId = orderId,
            Status = "Confirmed",
            Source = Name,
            TotalPrice = offer.Price * seats,
            Currency = offer.Currency,
            Message = "تم تأكيد الحجز مباشرة مع شركة الطيران (وضع تجريبي متوافق مع Duffel)",
            Offer = offer
        });
    }

    public Task<bool> CancelAsync(string externalOrderId, CancellationToken ct = default)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(externalOrderId));
    }

    public IReadOnlyList<FlightOfferDto> GeneratePopularInventory()
    {
        var offers = new List<FlightOfferDto>();
        var dayOffsets = new[] { 1, 2, 3, 5, 7 };

        foreach (var route in _options.PopularRoutes)
        {
            foreach (var day in dayOffsets)
            {
                offers.AddRange(BuildRouteOffers(
                    route.From,
                    route.To,
                    DateTime.Today.AddDays(day),
                    1,
                    "economy",
                    maxOffers: 2,
                    sourceTag: "demo"));
            }
        }

        return offers;
    }

    private List<FlightOfferDto> BuildRouteOffers(
        string fromCode,
        string toCode,
        DateTime date,
        int passengers,
        string cabin,
        int maxOffers = 5,
        string sourceTag = "demo")
    {
        AirportCatalog.TryResolve(fromCode, out _, out var fromCity);
        AirportCatalog.TryResolve(toCode, out _, out var varToCity);
        var toCity = string.IsNullOrEmpty(varToCity) ? toCode : varToCity;
        fromCity = string.IsNullOrEmpty(fromCity) ? fromCode : fromCity;

        var distanceFactor = EstimateDistanceFactor(fromCode, toCode);
        var list = new List<FlightOfferDto>();
        var airlinePool = Airlines.OrderBy(_ => _random.Next()).Take(maxOffers).ToArray();

        for (var i = 0; i < airlinePool.Length; i++)
        {
            var airline = airlinePool[i];
            var depHour = 6 + (i * 3) + _random.Next(0, 2);
            var durationHours = 1.2 + distanceFactor * 0.9 + _random.NextDouble();
            var dep = date.Date.AddHours(depHour).AddMinutes(_random.Next(0, 50));
            var arr = dep.AddHours(durationHours);
            var basePrice = 9000m + (decimal)(distanceFactor * 8500) + _random.Next(0, 4000);
            if (cabin.Equals("business", StringComparison.OrdinalIgnoreCase))
                basePrice *= 2.4m;

            var stops = distanceFactor > 2.5 && _random.NextDouble() > 0.55 ? 1 : 0;
            if (stops > 0) arr = arr.AddHours(1.5);

            var offerId = $"off_{sourceTag}_{fromCode}{toCode}_{date:yyyyMMdd}_{airline.Code}_{i}_{Guid.NewGuid():N}"[..48];

            list.Add(new FlightOfferDto
            {
                OfferId = offerId,
                Source = sourceTag,
                FlightNumber = $"{airline.Code}{_random.Next(100, 899)}",
                Airline = airline.Name,
                AirlineCode = airline.Code,
                DepartureCity = fromCity,
                ArrivalCity = toCity,
                DepartureAirport = fromCode,
                ArrivalAirport = toCode,
                DepartureTime = dep,
                ArrivalTime = arr,
                Price = Math.Round(basePrice, 0),
                Currency = "SDG",
                AvailableSeats = Math.Max(passengers, _random.Next(8, 120)),
                AircraftType = airline.Aircraft,
                Stops = stops,
                CabinClass = cabin,
                LastUpdatedUtc = DateTime.UtcNow,
                BookableDirect = true
            });
        }

        return list;
    }

    private static double EstimateDistanceFactor(string from, string to)
    {
        // Rough relative hop size for demo pricing
        var international = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CAI", "ASW", "JED", "RUH", "MED", "DXB", "AUH", "DOH", "BAH", "KWI", "MCT", "AMM", "BEY", "IST",
            "ADD", "NBO", "EBB", "JNB", "CPT", "LOS", "ACC", "CMN", "TUN", "ALG", "JUB",
            "LHR", "LGW", "CDG", "FRA", "AMS", "MAD", "FCO", "MXP", "MUC", "ZRH", "VIE",
            "JFK", "EWR", "IAD", "ORD", "LAX", "YYZ", "GRU",
            "BOM", "DEL", "BKK", "KUL", "SIN", "HKG", "NRT", "ICN", "SYD", "MEL"
        };
        var domestic = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "KRT", "PZU", "UYL", "EBD", "ELF", "KSL", "DOG", "GSU", "WHF" };

        if (domestic.Contains(from) && domestic.Contains(to)) return 1.0;
        if (international.Contains(from) || international.Contains(to))
        {
            if (to is "JFK" or "LAX" or "YYZ" or "SYD" or "MEL" || from is "JFK" or "LAX" or "YYZ" or "SYD" or "MEL") return 5.0;
            if (to is "LHR" or "CDG" or "FRA" or "AMS" || from is "LHR" or "CDG" or "FRA" or "AMS") return 4.2;
            if (to is "DXB" or "DOH" or "IST" or "JED" || from is "DXB" or "DOH" or "IST" or "JED") return 2.8;
            return 2.2;
        }

        return 1.6;
    }
}
