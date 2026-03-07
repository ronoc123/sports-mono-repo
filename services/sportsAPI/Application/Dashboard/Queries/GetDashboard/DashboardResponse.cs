namespace Application.Dashboard.Queries.GetDashboard;

public sealed class DashboardResponse
{
    public List<TrendingPlayerOptionDto> TrendingPlayerOptions { get; init; } = [];

    // Populated in Epic 3 — grouped by series
    public List<ActiveTriviaSeriesDto> ActiveTriviaSeries { get; init; } = [];

    // Populated in Epic 5
    public ActivePollDto? ActivePoll { get; init; } = null;
}
