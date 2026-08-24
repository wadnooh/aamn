using Microsoft.Extensions.Options;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public interface ICurrencyService
{
    string BaseCurrency { get; }
    string DefaultDisplayCurrency { get; }
    IReadOnlyList<string> Supported { get; }
    IReadOnlyDictionary<string, decimal> SdgPerUnit { get; }
    decimal Convert(decimal amount, string fromCurrency, string toCurrency);
    decimal ToSdg(decimal amount, string fromCurrency);
    decimal FromSdg(decimal amountSdg, string toCurrency);
}

public class CurrencyService : ICurrencyService
{
    private readonly CurrencyOptions _options;

    public CurrencyService(IOptions<CurrencyOptions> options)
    {
        _options = options.Value;
    }

    public string BaseCurrency => string.IsNullOrWhiteSpace(_options.BaseCurrency) ? "SDG" : _options.BaseCurrency.ToUpperInvariant();
    public string DefaultDisplayCurrency =>
        string.IsNullOrWhiteSpace(_options.DefaultDisplayCurrency) ? "USD" : _options.DefaultDisplayCurrency.ToUpperInvariant();

    public IReadOnlyDictionary<string, decimal> SdgPerUnit => _options.SdgPerUnit;

    public IReadOnlyList<string> Supported =>
        _options.SdgPerUnit.Keys
            .Select(k => k.ToUpperInvariant())
            .Distinct()
            .OrderBy(k => k == "USD" ? 0 : k == "EUR" ? 1 : k == "GBP" ? 2 : k == "SAR" ? 3 : k == "AED" ? 4 : k == "SDG" ? 5 : 10)
            .ThenBy(k => k)
            .ToList();

    private decimal RateToSdg(string currency)
    {
        var code = (currency ?? BaseCurrency).Trim().ToUpperInvariant();
        if (_options.SdgPerUnit.TryGetValue(code, out var rate) && rate > 0)
            return rate;
        return _options.SdgPerUnit.TryGetValue("USD", out var usd) ? usd : 600m;
    }

    public decimal ToSdg(decimal amount, string fromCurrency) =>
        Math.Round(amount * RateToSdg(fromCurrency), 2);

    public decimal FromSdg(decimal amountSdg, string toCurrency)
    {
        var rate = RateToSdg(toCurrency);
        if (rate <= 0) return amountSdg;
        var decimals = toCurrency.Equals("SDG", StringComparison.OrdinalIgnoreCase) ||
                       toCurrency.Equals("EGP", StringComparison.OrdinalIgnoreCase) ||
                       toCurrency.Equals("TRY", StringComparison.OrdinalIgnoreCase)
            ? 0
            : 2;
        return Math.Round(amountSdg / rate, decimals);
    }

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency)
    {
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return amount;
        var sdg = ToSdg(amount, fromCurrency);
        return FromSdg(sdg, toCurrency);
    }
}
