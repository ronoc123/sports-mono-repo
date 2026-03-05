namespace Application.Common.Interfaces;

public interface IBalanceNotificationService
{
    Task NotifyBalanceChangedAsync(string userId, long newBalance, CancellationToken ct = default);
}
