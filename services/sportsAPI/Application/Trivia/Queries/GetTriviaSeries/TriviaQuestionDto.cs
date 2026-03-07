using Domain.Enums;

namespace Application.Trivia.Queries.GetTriviaSeries;

public sealed class TriviaQuestionDto
{
    public Guid QuestionId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public List<string> AnswerOptions { get; init; } = [];
    public string CorrectAnswer { get; init; } = string.Empty;
    public int VoteRewardAmount { get; init; }
    public TriviaQuestionStatus Status { get; init; }
    public int ParticipationCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
