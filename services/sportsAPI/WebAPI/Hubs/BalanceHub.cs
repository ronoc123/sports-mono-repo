using Microsoft.AspNetCore.SignalR;

namespace sportsAPI.Hubs;

/// <summary>
/// Hub for real-time balance updates. Clients are auto-joined to their own
/// "balance-{userId}" group on connect and receive BalanceChanged events
/// whenever any balance-mutating command completes.
/// </summary>
public sealed class BalanceHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"balance-{userId}");

        await base.OnConnectedAsync();
    }
}
