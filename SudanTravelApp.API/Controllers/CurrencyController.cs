using Microsoft.AspNetCore.Mvc;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/currency")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _fx;

    public CurrencyController(ICurrencyService fx)
    {
        _fx = fx;
    }

    [HttpGet("rates")]
    public ActionResult GetRates()
    {
        var ratesFromSdg = _fx.Supported.ToDictionary(
            c => c,
            c => _fx.FromSdg(1m, c) == 0 ? 1m : 1m / Math.Max(0.0000001m, _fx.SdgPerUnit.TryGetValue(c, out var r) ? r : 600m));

        // Clearer payload: how many of each currency for 1 SDG, and SDG per 1 unit
        var sdgPerUnit = _fx.Supported.ToDictionary(
            c => c,
            c => _fx.SdgPerUnit.TryGetValue(c, out var r) ? r : 600m);

        return Ok(new
        {
            baseCurrency = _fx.BaseCurrency,
            defaultDisplayCurrency = _fx.DefaultDisplayCurrency,
            supported = _fx.Supported,
            sdgPerUnit,
            updatedAtUtc = DateTime.UtcNow,
            note = "Approximate company rates for display. 1 USD ≈ " +
                   (sdgPerUnit.TryGetValue("USD", out var usd) ? usd.ToString("0") : "600") + " SDG"
        });
    }

    [HttpGet("convert")]
    public ActionResult Convert([FromQuery] decimal amount, [FromQuery] string from = "SDG", [FromQuery] string to = "USD")
    {
        var result = _fx.Convert(amount, from, to);
        return Ok(new
        {
            amount,
            from = from.ToUpperInvariant(),
            to = to.ToUpperInvariant(),
            converted = result
        });
    }
}
