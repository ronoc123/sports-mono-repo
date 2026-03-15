namespace Domain.AIConversation;

public interface IAISessionRepository
{
    Task<AISession?> GetByIdAsync(AISessionId id, CancellationToken ct = default);
    Task<AISession?> GetByLinkedEntityAsync(Guid linkedEntityId, CancellationToken ct = default);
    Task AddAsync(AISession session, CancellationToken ct = default);
    void Update(AISession session);
    void AddMessages(IEnumerable<AIMessage> messages);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
