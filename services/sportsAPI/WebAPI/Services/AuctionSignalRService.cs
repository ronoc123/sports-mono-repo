using Application.Marketplace.Services;
using Microsoft.AspNetCore.SignalR;
using sportsAPI.Hubs;

namespace sportsAPI.Services;

/// <summary>
/// Wraps IHubContext&lt;AuctionHub&gt; to deliver real-time events to connected clients.
/// Injected into PlaceBidCommandHandler and SettleAuctionCommandHandler.
/// </summary>
public sealed class AuctionSignalRService : IAuctionSignalRService
{
    private readonly IHubContext<AuctionHub> _hub;

    public AuctionSignalRService(IHubContext<AuctionHub> hub) => _hub = hub;

    public Task BroadcastBidPlacedAsync(
        Guid listingId,
        long currentBid,
        Guid bidderId,
        DateTime timestamp,
        CancellationToken ct = default)
        => _hub.Clients
            .Group($"auction-{listingId}")
            .SendAsync("BidPlaced", new
            {
                listingId,
                currentBid,
                bidderId,
                timestamp,
            }, ct);

    public Task SendOutbidNotificationAsync(
        Guid outbidUserId,
        Guid listingId,
        long releasedAmount,
        CancellationToken ct = default)
        => _hub.Clients
            .Group($"user-{outbidUserId}")
            .SendAsync("OutbidNotification", new
            {
                listingId,
                releasedAmount,
            }, ct);

    public Task BroadcastAuctionSettledAsync(
        Guid listingId,
        Guid? winnerId,
        long finalBid,
        CancellationToken ct = default)
        => _hub.Clients
            .Group($"auction-{listingId}")
            .SendAsync("AuctionSettled", new
            {
                listingId,
                winnerId,
                finalBid,
            }, ct);
}
