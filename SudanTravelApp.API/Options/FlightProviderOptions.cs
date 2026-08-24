namespace SudanTravelApp.API.Options;

public class FlightProviderOptions
{
    public const string SectionName = "FlightProvider";

    /// <summary>duffel | demo</summary>
    public string Provider { get; set; } = "demo";

    public string DuffelApiKey { get; set; } = string.Empty;
    public string DuffelBaseUrl { get; set; } = "https://api.duffel.com";
    public string DuffelVersion { get; set; } = "v2";

    /// <summary>Popular routes refreshed automatically without admin.</summary>
    public List<PopularRoute> PopularRoutes { get; set; } =
    [
        new() { From = "KRT", To = "DXB", FromCity = "الخرطوم", ToCity = "دبي" },
        new() { From = "KRT", To = "CAI", FromCity = "الخرطوم", ToCity = "القاهرة" },
        new() { From = "KRT", To = "JED", FromCity = "الخرطوم", ToCity = "جدة" },
        new() { From = "KRT", To = "IST", FromCity = "الخرطوم", ToCity = "إسطنبول" },
        new() { From = "KRT", To = "DOH", FromCity = "الخرطوم", ToCity = "الدوحة" },
        new() { From = "KRT", To = "ADD", FromCity = "الخرطوم", ToCity = "أديس أبابا" },
        new() { From = "KRT", To = "NBO", FromCity = "الخرطوم", ToCity = "نيروبي" },
        new() { From = "KRT", To = "LHR", FromCity = "الخرطوم", ToCity = "لندن" },
        new() { From = "KRT", To = "CDG", FromCity = "الخرطوم", ToCity = "باريس" },
        new() { From = "KRT", To = "FRA", FromCity = "الخرطوم", ToCity = "فرانكفورت" },
        new() { From = "KRT", To = "JFK", FromCity = "الخرطوم", ToCity = "نيويورك" },
        new() { From = "KRT", To = "PZU", FromCity = "الخرطوم", ToCity = "بورتسودان" },
        new() { From = "DXB", To = "KRT", FromCity = "دبي", ToCity = "الخرطوم" },
        new() { From = "CAI", To = "KRT", FromCity = "القاهرة", ToCity = "الخرطوم" },
        new() { From = "LHR", To = "DXB", FromCity = "لندن", ToCity = "دبي" },
        new() { From = "JFK", To = "DXB", FromCity = "نيويورك", ToCity = "دبي" }
    ];

    public int SyncIntervalMinutes { get; set; } = 15;
}

public class PopularRoute
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
}
