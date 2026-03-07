using Domain.Enums;

namespace Application.Poll.Queries.GetPolls;

public sealed class PollDto
{
    public Guid PollId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public PollStatus Status { get; init; }
    public int TotalVotes { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<PollOptionDto> Options { get; init; } = [];
}

public sealed class PollOptionDto
{
    public Guid PollOptionId { get; init; }
    public string OptionText { get; init; } = string.Empty;
    public int VoteCount { get; init; }
    public int SortOrder { get; init; }
}
