namespace Application.Dashboard.Queries.GetDashboard;

public sealed class TrendingPlayerOptionDto
{
    public Guid PlayerOptionId { get; init; }
    public string PlayerName { get; init; } = string.Empty;
    public string PositionLabel { get; init; } = string.Empty;
    public long VoteCount { get; init; }
}
