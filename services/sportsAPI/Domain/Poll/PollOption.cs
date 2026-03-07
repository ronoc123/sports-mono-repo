using BuildingBlocks.Exceptions;

namespace Domain.Poll;

/// <summary>
/// An answer option within a Poll.
/// </summary>
public sealed class PollOption
{
    internal PollOption() { } // EF

    public PollOptionId Id { get; private set; } = null!;
    public PollId PollId { get; private set; } = null!;
    public string Text { get; private set; } = string.Empty;
    public int VoteCount { get; private set; }
    public int SortOrder { get; private set; }

    public static PollOption Create(PollId pollId, string text, int sortOrder)
    {
        ArgumentNullException.ThrowIfNull(pollId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (text.Trim().Length > 300) throw new DomainException("Option text cannot exceed 300 characters.");

        return new PollOption
        {
            Id = PollOptionId.Of(Guid.NewGuid()),
            PollId = pollId,
            Text = text.Trim(),
            VoteCount = 0,
            SortOrder = sortOrder
        };
    }

    public void IncrementVoteCount() => VoteCount++;
}
