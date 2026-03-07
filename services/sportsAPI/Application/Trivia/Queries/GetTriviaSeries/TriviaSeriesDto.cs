namespace Application.Trivia.Queries.GetTriviaSeries;

public sealed class TriviaSeriesDto
{
    public Guid SeriesId { get; init; }
    public string SeriesName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<TriviaQuestionDto> Questions { get; init; } = [];
}
