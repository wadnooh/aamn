using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public interface IAiTechAssistant
{
    Task<AiAssistResponse> AssistAsync(AiAssistRequest request, CancellationToken ct = default);
}

public class AiTechAssistant : IAiTechAssistant
{
    private readonly IFlightProvider _flights;
    private readonly IFlightOfferCache _cache;
    private readonly AiOptions _options;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<AiTechAssistant> _logger;

    public AiTechAssistant(
        IFlightProvider flights,
        IFlightOfferCache cache,
        IOptions<AiOptions> options,
        IHttpClientFactory httpFactory,
        ILogger<AiTechAssistant> logger)
    {
        _flights = flights;
        _cache = cache;
        _options = options.Value;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<AiAssistResponse> AssistAsync(AiAssistRequest request, CancellationToken ct = default)
    {
        var message = (request.Message ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return new AiAssistResponse
            {
                Reply = "اكتب وجهتك أو سؤالك، مثل: أرخص خدمة من الخرطوم إلى دبي الأسبوع القادم",
                Intent = "help",
                Provider = EffectiveProvider
            };
        }

        var parsed = ParseNaturalLanguage(message);

        if (_options.Provider.Equals("openai", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(_options.OpenAiApiKey))
        {
            try
            {
                var aiParsed = await ParseWithOpenAiAsync(message, ct);
                if (aiParsed != null)
                {
                    parsed = MergeParse(parsed, aiParsed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI parse failed; using local AI parser");
            }
        }

        List<FlightOfferDto> recommendations = [];
        if (parsed.Search != null)
        {
            var hasRoute = !string.IsNullOrWhiteSpace(parsed.Search.From)
                           || !string.IsNullOrWhiteSpace(parsed.Search.To);

            if (hasRoute)
            {
                // Default origin to Khartoum when only destination is given
                parsed.Search.From ??= "الخرطوم";
                parsed.Search.Date ??= DateTime.Today.AddDays(1);
                recommendations = (await _flights.SearchAsync(parsed.Search, ct)).Take(5).ToList();
            }
            else if (parsed.Intent is "recommend" or "cheapest" or "search")
            {
                recommendations = _cache.GetAll()
                    .OrderBy(o => o.Price)
                    .Take(5)
                    .ToList();
            }
        }

        var reply = BuildReply(message, parsed, recommendations);

        return new AiAssistResponse
        {
            Reply = reply,
            Intent = parsed.Intent,
            SuggestedSearch = parsed.Search,
            Recommendations = recommendations,
            Provider = EffectiveProvider
        };
    }

    private string EffectiveProvider =>
        _options.Provider.Equals("openai", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(_options.OpenAiApiKey)
            ? "openai"
            : "local";

    private async Task<ParsedIntent?> ParseWithOpenAiAsync(string message, CancellationToken ct)
    {
        var client = _httpFactory.CreateClient("openai");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.OpenAiApiKey);

        var system = """
            You extract flight search intent from Arabic/English user messages for a Sudan travel app.
            Return ONLY compact JSON:
            {"intent":"search|recommend|cheapest|help|general","from":"city or IATA or null","to":"city or IATA or null","date":"YYYY-MM-DD or null","cabin":"economy|business","passengers":1}
            Prefer Arabic city names when possible. Today context is Sudan travel.
            """;

        var payload = new
        {
            model = _options.OpenAiModel,
            temperature = 0.2,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = message }
            }
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync($"{_options.OpenAiBaseUrl.TrimEnd('/')}/chat/completions", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) return null;

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";

        text = text.Trim();
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        using var json = JsonDocument.Parse(text[start..(end + 1)]);
        var root = json.RootElement;
        var intent = root.TryGetProperty("intent", out var i) ? i.GetString() ?? "search" : "search";
        DateTime? date = null;
        if (root.TryGetProperty("date", out var d) && d.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(d.GetString(), out var parsedDate))
        {
            date = parsedDate.Date;
        }

        var search = new FlightSearchRequest
        {
            From = root.TryGetProperty("from", out var f) && f.ValueKind == JsonValueKind.String ? f.GetString() : null,
            To = root.TryGetProperty("to", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null,
            Date = date,
            Passengers = root.TryGetProperty("passengers", out var p) && p.TryGetInt32(out var pn) ? Math.Max(1, pn) : 1,
            CabinClass = root.TryGetProperty("cabin", out var c) ? c.GetString() ?? "economy" : "economy"
        };

        return new ParsedIntent { Intent = intent, Search = search };
    }

    private static ParsedIntent ParseNaturalLanguage(string message)
    {
        var lower = message.ToLowerInvariant();
        var intent = "general";
        if (ContainsAny(lower, "أرخص", "ارخص", "cheap", "أقل سعر", "اقل سعر"))
            intent = "cheapest";
        else if (ContainsAny(lower, "اقترح", "وصّي", "وصي", "recommend", "أنسب", "انسب"))
            intent = "recommend";
        else if (ContainsAny(lower, "خدمة", "طيران", "تذكرة", "flight", "من ", "إلى", "الى", "to "))
            intent = "search";
        else if (ContainsAny(lower, "مساعدة", "help", "كيف"))
            intent = "help";

        string? from = null;
        string? to = null;

        var fromTo = Regex.Match(message, @"من\s+([^\s]+(?:\s+[^\s]+)?)\s+(?:إلى|الى|لـ|ل)\s+([^\s]+(?:\s+[^\s]+)?)");
        if (fromTo.Success)
        {
            from = fromTo.Groups[1].Value.Trim();
            to = fromTo.Groups[2].Value.Trim();
        }
        else
        {
            var en = Regex.Match(message, @"from\s+(\w+)\s+to\s+(\w+)", RegexOptions.IgnoreCase);
            if (en.Success)
            {
                from = en.Groups[1].Value;
                to = en.Groups[2].Value;
            }
        }

        // Soft city detection
        if (from == null || to == null)
        {
            var found = new List<string>();
            foreach (var city in new[]
                     {
                         "الخرطوم", "بورتسودان", "نيالا", "القاهرة", "دبي", "جدة", "الرياض",
                         "إسطنبول", "اسطنبول", "الدوحة", "عنتيبي", "جوبا", "أسوان", "لندن", "باريس",
                         "khartoum", "dubai", "cairo", "jeddah"
                     })
            {
                if (message.Contains(city, StringComparison.OrdinalIgnoreCase))
                    found.Add(city);
            }

            if (found.Count >= 2)
            {
                from ??= found[0];
                to ??= found[1];
            }
            else if (found.Count == 1 && from == null)
            {
                to = found[0];
                from = "الخرطوم";
            }
        }

        DateTime? date = null;
        if (ContainsAny(lower, "غدا", "غداً", "tomorrow"))
            date = DateTime.Today.AddDays(1);
        else if (ContainsAny(lower, "بعد غد", "بعدغد"))
            date = DateTime.Today.AddDays(2);
        else if (ContainsAny(lower, "الأسبوع القادم", "الاسبوع القادم", "next week"))
            date = DateTime.Today.AddDays(7);
        else
        {
            var dateMatch = Regex.Match(message, @"(\d{4}-\d{2}-\d{2})|(\d{1,2}[\/\-]\d{1,2}[\/\-]\d{2,4})");
            if (dateMatch.Success && DateTime.TryParse(dateMatch.Value, out var parsed))
                date = parsed.Date;
        }

        var cabin = ContainsAny(lower, "رجال أعمال", "رجال اعمال", "business") ? "business" : "economy";
        var passengers = 1;
        var pax = Regex.Match(message, @"(\d+)\s*(مسافر|ركاب|persons?|passengers?)");
        if (pax.Success && int.TryParse(pax.Groups[1].Value, out var n)) passengers = Math.Clamp(n, 1, 9);

        return new ParsedIntent
        {
            Intent = intent,
            Search = new FlightSearchRequest
            {
                From = from,
                To = to,
                Date = date,
                Passengers = passengers,
                CabinClass = cabin
            }
        };
    }

    private static ParsedIntent MergeParse(ParsedIntent local, ParsedIntent ai)
    {
        local.Intent = string.IsNullOrWhiteSpace(ai.Intent) ? local.Intent : ai.Intent;
        if (ai.Search == null) return local;
        local.Search ??= new FlightSearchRequest();
        local.Search.From = FirstNonEmpty(ai.Search.From, local.Search.From);
        local.Search.To = FirstNonEmpty(ai.Search.To, local.Search.To);
        local.Search.Date ??= ai.Search.Date;
        local.Search.Passengers = Math.Max(local.Search.Passengers, ai.Search.Passengers);
        local.Search.CabinClass = FirstNonEmpty(ai.Search.CabinClass, local.Search.CabinClass) ?? "economy";
        return local;
    }

    private static string BuildReply(string message, ParsedIntent parsed, List<FlightOfferDto> offers)
    {
        if (parsed.Intent == "help")
        {
            return "يمكنني البحث عن خدمات تقنية، اقتراح الأرخص، والحجز مباشرة مع شركات الطيران. جرّب: «أرخص خدمة من الخرطوم إلى جدة غداً»";
        }

        if (offers.Count == 0)
        {
            return "لم أجد عروضاً مطابقة الآن. حدّد المدينة والتاريخ بوضوح، مثل: من الخرطوم إلى دبي 2026-07-15";
        }

        var best = offers[0];
        var route = $"{best.DepartureCity} → {best.ArrivalCity}";
        var sb = new StringBuilder();
        sb.AppendLine(parsed.Intent == "cheapest"
            ? $"أفضل سعر وجدته لـ {route}:"
            : $"إليك أفضل الخيارات لـ {route}:");
        sb.AppendLine($"• {best.Airline} {best.FlightNumber} — {best.Price:N0} {best.Currency} — إقلاع {best.DepartureTime:g}");
        if (offers.Count > 1)
        {
            sb.AppendLine($"وعرضت لك {offers.Count} خيارات مرتبة حسب السعر. يمكنك الحجز مباشرة بدون تدخل إداري.");
        }

        return sb.ToString().Trim();
    }

    private static bool ContainsAny(string text, params string[] words) =>
        words.Any(w => text.Contains(w, StringComparison.OrdinalIgnoreCase));

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private sealed class ParsedIntent
    {
        public string Intent { get; set; } = "general";
        public FlightSearchRequest? Search { get; set; }
    }
}
