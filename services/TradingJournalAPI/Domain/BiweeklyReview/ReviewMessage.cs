using Domain.Abstractions;

namespace Domain.BiweeklyReview;

public enum ReviewMessageRole { User, Assistant }

public class ReviewMessage : Entity<Guid>
{
    internal ReviewMessage() { }

    public ReviewSessionId SessionId { get; private set; } = default!;
    public ReviewMessageRole Role { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTime Timestamp { get; private set; }

    internal static ReviewMessage Create(ReviewSessionId sessionId, ReviewMessageRole role, string content)
    {
        return new ReviewMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
    }
}
