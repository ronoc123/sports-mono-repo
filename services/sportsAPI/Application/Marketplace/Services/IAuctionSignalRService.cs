namespace Application.Marketplace.Services;

/// <summary>
/// Abstraction over SignalR so Application-layer handlers can emit real-time events
/// without taking a direct dependency on ASP.NET Core SignalR types.
/// </summary>
public interface IAuctionSignalRService
{
    /// <summary>
    /// Broadcast a bid-placed event to all clients watching the listing group.
    /// </summary>
    Task BroadcastBidPlacedAsync(
        Guid listingId,
        long currentBid,
        Guid bidderId,
        DateTime timestamp,
        CancellationToken ct = default);

    /// <summary>
    /// Send an outbid notification to the specific outbid user's personal group.
    /// </summary>
    Task SendOutbidNotificationAsync(
        Guid outbidUserId,
        Guid listingId,
        long releasedAmount,
        CancellationToken ct = default);

    /// <summary>
    /// Broadcast an auction-settled event to all clients watching the listing group.
    /// </summary>
    Task BroadcastAuctionSettledAsync(
        Guid listingId,
        Guid? winnerId,
        long finalBid,
        CancellationToken ct = default);
}
