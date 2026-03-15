using Domain.Abstractions;

namespace Domain.AIConversation;

public enum MessageRole { User, Assistant }

public class AIMessage : Entity<Guid>
{
    internal AIMessage() { }

    public AISessionId SessionId { get; private set; } = default!;
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTime Timestamp { get; private set; }

    internal static AIMessage Create(AISessionId sessionId, MessageRole role, string content)
    {
        return new AIMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
    }
}
