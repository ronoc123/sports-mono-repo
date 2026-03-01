using Application.Marketplace.Commands.SettleAuction;
using Domain.Marketplace;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Marketplace.Services;

/// <summary>
/// Background service that polls every 30 seconds for expired AuctionListings
/// and dispatches SettleAuctionCommand for each one via MediatR.
/// Errors are logged and do not crash the service.
/// </summary>
public sealed class AuctionExpiryService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionExpiryService> _logger;

    public AuctionExpiryService(
        IServiceScopeFactory scopeFactory,
        ILogger<AuctionExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuctionExpiryService started. Poll interval: {Interval}s.",
            PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAndSettleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "AuctionExpiryService: unhandled exception during poll cycle. Service continues.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("AuctionExpiryService stopping.");
    }

    private async Task PollAndSettleAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var repo = scope.ServiceProvider.GetRequiredService<IRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var now = DateTime.UtcNow;

        // Find all active listings that have expired
        var expiredListingIds = await repo.Query<AuctionListing>()
            .Where(l => l.Status == "active" && l.ExpiresAt <= now)
            .Select(l => l.Id)
            .ToListAsync(ct);

        if (expiredListingIds.Count == 0)
            return;

        _logger.LogInformation(
            "AuctionExpiryService: found {Count} expired listing(s) to settle.", expiredListingIds.Count);

        foreach (var listingId in expiredListingIds)
        {
            try
            {
                await sender.Send(new SettleAuctionCommand(listingId), ct);
            }
            catch (Exception ex)
            {
                // Log and continue — do not let one failed settlement block the rest
                _logger.LogError(ex,
                    "AuctionExpiryService: failed to settle listing {ListingId}.", listingId);
            }
        }
    }
}
