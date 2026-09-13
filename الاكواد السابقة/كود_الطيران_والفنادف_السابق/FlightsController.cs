using Microsoft.AspNetCore.Mvc;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IFlightProvider _provider;
    private readonly IFlightOfferCache _cache;

    public FlightsController(IFlightProvider provider, IFlightOfferCache cache)
    {
        _provider = provider;
        _cache = cache;
    }

    /// <summary>Cached live inventory (auto-synced, no admin).</summary>
    [HttpGet]
    public ActionResult<object> GetFlights()
    {
        var offers = _cache.GetAll();
        return Ok(new
        {
            source = _provider.Name,
            live = _provider.IsLive,
            lastSyncUtc = _cache.LastSyncUtc,
            count = offers.Count,
            flights = offers
        });
    }

    [HttpGet("{offerId}")]
    public ActionResult<FlightOfferDto> GetFlight(string offerId)
    {
        var offer = _cache.GetByOfferId(offerId);
        if (offer == null) return NotFound();
        return offer;
    }

    /// <summary>Live search against airline network (Duffel) or demo live feed.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<object>> SearchFlights(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] DateTime? date,
        [FromQuery] int passengers = 1,
        [FromQuery] string cabinClass = "economy",
        CancellationToken ct = default)
    {
        var request = new FlightSearchRequest
        {
            From = from,
            To = to,
            Date = date,
            Passengers = passengers,
            CabinClass = cabinClass,
            LiveOnly = true
        };

        var offers = await _provider.SearchAsync(request, ct);
        return Ok(new
        {
            source = _provider.Name,
            live = true,
            autoUpdated = true,
            count = offers.Count,
            flights = offers
        });
    }

    [HttpGet("status")]
    public ActionResult<object> Status()
    {
        return Ok(new
        {
            provider = _provider.Name,
            live = _provider.IsLive,
            lastSyncUtc = _cache.LastSyncUtc,
            cachedOffers = _cache.GetAll().Count,
            message = _provider.Name == "duffel"
                ? "متصل بشبكة شركات الطيران عبر Duffel — التحديث تلقائي بدون أدمن"
                : "وضع تجريبي حي يحاكي الربط المباشر بشركات الطيران — أضف مفتاح Duffel للإنتاج"
        });
    }
}
