namespace Application.Dashboard.Queries.GetDashboard;

public sealed class DashboardResponse
{
    public List<TrendingPlayerOptionDto> TrendingPlayerOptions { get; init; } = [];

    // Populated in Epic 3
    public List<ActiveTriviaQuestionDto> ActiveTriviaQuestions { get; init; } = [];

    // Populated in Epic 5
    public ActivePollDto? ActivePoll { get; init; } = null;
}
