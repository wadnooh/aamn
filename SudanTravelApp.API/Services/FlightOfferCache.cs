using System.Collections.Concurrent;
using SudanTravelApp.API.Models.Dtos;

namespace SudanTravelApp.API.Services;

public class FlightOfferCache : IFlightOfferCache
{
    private readonly ConcurrentDictionary<string, FlightOfferDto> _offers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _syncLock = new();

    public DateTime? LastSyncUtc { get; private set; }

    public IReadOnlyList<FlightOfferDto> GetAll()
    {
        return _offers.Values
            .OrderBy(o => o.DepartureTime)
            .ThenBy(o => o.Price)
            .ToList();
    }

    public FlightOfferDto? GetByOfferId(string offerId)
    {
        return _offers.TryGetValue(offerId, out var offer) ? offer : null;
    }

    public void Upsert(IEnumerable<FlightOfferDto> offers)
    {
        foreach (var offer in offers)
        {
            if (string.IsNullOrWhiteSpace(offer.OfferId)) continue;
            offer.LastUpdatedUtc = DateTime.UtcNow;
            _offers[offer.OfferId] = offer;
        }

        LastSyncUtc = DateTime.UtcNow;
    }

    public void ReplacePopular(IEnumerable<FlightOfferDto> offers)
    {
        lock (_syncLock)
        {
            var popularIds = _offers.Values
                .Where(o => o.Source is "demo" or "duffel-sync")
                .Select(o => o.OfferId)
                .ToList();

            foreach (var id in popularIds)
            {
                _offers.TryRemove(id, out _);
            }

            Upsert(offers);
        }
    }
}
