namespace SudanTravelApp.API.Options;

public class CurrencyOptions
{
    public const string SectionName = "Currency";

    /// <summary>Canonical inventory currency (bookings stored in this).</summary>
    public string BaseCurrency { get; set; } = "SDG";

    /// <summary>Default UI currency for international visitors.</summary>
    public string DefaultDisplayCurrency { get; set; } = "USD";

    /// <summary>
    /// How many units of BaseCurrency (SDG) equal 1 unit of each currency.
    /// Example: USD=600 means 1 USD ≈ 600 SDG.
    /// </summary>
    public Dictionary<string, decimal> SdgPerUnit { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SDG"] = 1m,
        ["USD"] = 600m,
        ["EUR"] = 650m,
        ["GBP"] = 760m,
        ["AED"] = 163m,
        ["SAR"] = 160m,
        ["EGP"] = 12m,
        ["QAR"] = 165m,
        ["KWD"] = 1950m,
        ["TRY"] = 18m
    };
}
