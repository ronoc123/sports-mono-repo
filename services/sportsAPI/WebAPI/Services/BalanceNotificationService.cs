using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using sportsAPI.Hubs;

namespace sportsAPI.Services;

public sealed class BalanceNotificationService : IBalanceNotificationService
{
    private readonly IHubContext<BalanceHub> _hub;

    public BalanceNotificationService(IHubContext<BalanceHub> hub) => _hub = hub;

    public Task NotifyBalanceChangedAsync(string userId, long newBalance, CancellationToken ct = default)
        => _hub.Clients.Group($"balance-{userId}")
            .SendAsync("BalanceChanged", new { balance = newBalance }, ct);
}
