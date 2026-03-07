namespace Application.Dashboard.Queries.GetDashboard;

public sealed class ActiveTriviaSeriesDto
{
    public Guid SeriesId { get; init; }
    public string SeriesName { get; init; } = string.Empty;
    public List<ActiveTriviaQuestionDto> Questions { get; init; } = [];
}
