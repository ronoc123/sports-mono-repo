namespace Application.Dashboard.Queries.GetDashboard;

public sealed class ActiveTriviaQuestionDto
{
    public Guid QuestionId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public List<string> AnswerOptions { get; init; } = [];
    public int VoteRewardAmount { get; init; }
    public bool AnsweredByMe { get; init; }
    public string? SelectedAnswer { get; init; }
}
