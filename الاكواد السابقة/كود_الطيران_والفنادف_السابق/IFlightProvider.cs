using SudanTravelApp.API.Models.Dtos;

namespace SudanTravelApp.API.Services;

public interface IFlightProvider
{
    string Name { get; }
    bool IsLive { get; }
    Task<IReadOnlyList<FlightOfferDto>> SearchAsync(FlightSearchRequest request, CancellationToken ct = default);
    Task<LiveBookingResult> BookAsync(LiveBookingRequest request, FlightOfferDto offer, CancellationToken ct = default);
    Task<bool> CancelAsync(string externalOrderId, CancellationToken ct = default);
}

public interface IFlightOfferCache
{
    IReadOnlyList<FlightOfferDto> GetAll();
    FlightOfferDto? GetByOfferId(string offerId);
    void Upsert(IEnumerable<FlightOfferDto> offers);
    void ReplacePopular(IEnumerable<FlightOfferDto> offers);
    DateTime? LastSyncUtc { get; }
}
