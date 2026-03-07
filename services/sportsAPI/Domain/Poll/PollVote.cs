using BuildingBlocks.Exceptions;

namespace Domain.Poll;

/// <summary>
/// Records a fan's vote on a poll option.
/// Idempotency enforced by UNIQUE (UserId, PollId) at the DB level.
/// </summary>
public sealed class PollVote
{
    internal PollVote() { } // EF

    public PollVoteId Id { get; private set; } = null!;
    public PollId PollId { get; private set; } = null!;
    public PollOptionId PollOptionId { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DateTime VotedAt { get; private set; }

    public static PollVote Record(PollId pollId, PollOptionId optionId, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(pollId);
        ArgumentNullException.ThrowIfNull(optionId);
        if (userId == Guid.Empty) throw new DomainException("UserId cannot be empty.");

        return new PollVote
        {
            Id = PollVoteId.Of(Guid.NewGuid()),
            PollId = pollId,
            PollOptionId = optionId,
            UserId = userId,
            VotedAt = DateTime.UtcNow
        };
    }
}
