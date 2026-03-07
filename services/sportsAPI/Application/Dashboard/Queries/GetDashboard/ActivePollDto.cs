namespace Application.Dashboard.Queries.GetDashboard;

public sealed class ActivePollDto
{
    public Guid PollId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public List<ActivePollOptionDto> Options { get; init; } = [];
    public bool VotedByMe { get; init; }
    public Guid? SelectedOptionId { get; init; }
}

public sealed class ActivePollOptionDto
{
    public Guid PollOptionId { get; init; }
    public string OptionText { get; init; } = string.Empty;
    public int VoteCount { get; init; }
}
