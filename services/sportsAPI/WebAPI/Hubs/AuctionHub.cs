using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace sportsAPI.Hubs;

/// <summary>
/// Real-time auction hub. Stub — fully wired in Story 3.4.
/// Clients join/leave auction groups to receive live bid updates and outbid notifications.
/// </summary>
[Authorize]
public sealed class AuctionHub : Hub
{
    /// <summary>Join the group for a specific listing to receive live bid events.</summary>
    public async Task JoinListing(string listingId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{listingId}");

    /// <summary>Leave the group for a specific listing.</summary>
    public async Task LeaveListing(string listingId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction-{listingId}");

    public override async Task OnConnectedAsync()
    {
        // Join user-specific group for outbid notifications (Story 3.4)
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnConnectedAsync();
    }
}
