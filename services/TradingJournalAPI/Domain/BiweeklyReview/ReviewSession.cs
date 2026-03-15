using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.BiweeklyReview;

public sealed class ReviewSession : Aggregate<ReviewSessionId>
{
    private readonly List<ReviewMessage> _messages = new();

    internal ReviewSession() { }

    public Guid UserId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public IReadOnlyList<ReviewMessage> Messages => _messages.AsReadOnly();

    public static ReviewSession Create(Guid userId, DateTime periodStart, DateTime periodEnd)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");

        return new ReviewSession
        {
            Id = ReviewSessionId.New(),
            UserId = userId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };
    }

    public ReviewMessage AddUserMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Message content cannot be empty.");
        var msg = ReviewMessage.Create(Id, ReviewMessageRole.User, content);
        _messages.Add(msg);
        return msg;
    }

    public ReviewMessage AddAssistantMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Assistant response cannot be empty.");
        var msg = ReviewMessage.Create(Id, ReviewMessageRole.Assistant, content);
        _messages.Add(msg);
        return msg;
    }
}
