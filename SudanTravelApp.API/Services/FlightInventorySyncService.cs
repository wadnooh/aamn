using Microsoft.Extensions.Options;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

/// <summary>
/// Background sync of popular routes — no admin intervention required.
/// </summary>
public class FlightInventorySyncService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IFlightOfferCache _cache;
    private readonly FlightProviderOptions _options;
    private readonly ILogger<FlightInventorySyncService> _logger;

    public FlightInventorySyncService(
        IServiceProvider services,
        IFlightOfferCache cache,
        IOptions<FlightProviderOptions> options,
        ILogger<FlightInventorySyncService> logger)
    {
        _services = services;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial sync shortly after startup
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        await SyncOnceAsync(stoppingToken);

        var interval = TimeSpan.FromMinutes(Math.Max(5, _options.SyncIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncOnceAsync(stoppingToken);
        }
    }

    private async Task SyncOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var provider = scope.ServiceProvider.GetRequiredService<IFlightProvider>();
            var demo = scope.ServiceProvider.GetRequiredService<DemoFlightProvider>();

            var all = new List<Models.Dtos.FlightOfferDto>();

            if (provider is DuffelFlightProvider && !string.IsNullOrWhiteSpace(_options.DuffelApiKey))
            {
                foreach (var route in _options.PopularRoutes)
                {
                    ct.ThrowIfCancellationRequested();
                    var offers = await provider.SearchAsync(new Models.Dtos.FlightSearchRequest
                    {
                        From = route.From,
                        To = route.To,
                        Date = DateTime.Today.AddDays(2),
                        Passengers = 1,
                        LiveOnly = true
                    }, ct);

                    foreach (var o in offers.Take(3))
                    {
                        o.Source = o.Source == "duffel" ? "duffel-sync" : o.Source;
                        all.Add(o);
                    }

                    await Task.Delay(200, ct);
                }
            }

            if (all.Count == 0)
            {
                all.AddRange(demo.GeneratePopularInventory());
            }

            _cache.ReplacePopular(all);
            _logger.LogInformation(
                "Flight inventory synced automatically: {Count} offers at {Time:u} via {Provider}",
                all.Count,
                DateTime.UtcNow,
                provider.Name);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Automatic flight sync failed");
        }
    }
}
