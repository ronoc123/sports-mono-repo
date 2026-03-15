using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.AIConversation;

public enum AISessionContext { JournalEntry, IntegrityAnswer }

public sealed class AISession : Aggregate<AISessionId>
{
    private readonly List<AIMessage> _messages = new();

    internal AISession() { }

    public Guid UserId { get; private set; }
    public AISessionContext Context { get; private set; }

    /// <summary>JournalEntry.Id or IntegrityAnswer.Id that this session is linked to.</summary>
    public Guid LinkedEntityId { get; private set; }

    public IReadOnlyList<AIMessage> Messages => _messages.AsReadOnly();

    public static AISession Create(Guid userId, AISessionContext context, Guid linkedEntityId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (linkedEntityId == Guid.Empty) throw new DomainException("LinkedEntityId is required.");

        return new AISession
        {
            Id = AISessionId.New(),
            UserId = userId,
            Context = context,
            LinkedEntityId = linkedEntityId
        };
    }

    public AIMessage AddUserMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Message content cannot be empty.");
        var msg = AIMessage.Create(Id, MessageRole.User, content);
        _messages.Add(msg);
        return msg;
    }

    public AIMessage AddAssistantMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Assistant response cannot be empty.");
        var msg = AIMessage.Create(Id, MessageRole.Assistant, content);
        _messages.Add(msg);
        return msg;
    }
}
