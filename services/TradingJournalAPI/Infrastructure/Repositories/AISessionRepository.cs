using Domain.AIConversation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class AISessionRepository : IAISessionRepository
{
    private readonly TradingJournalDbContext _db;

    public AISessionRepository(TradingJournalDbContext db) => _db = db;

    public async Task<AISession?> GetByIdAsync(AISessionId id, CancellationToken ct = default)
        => await _db.AISessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<AISession?> GetByLinkedEntityAsync(Guid linkedEntityId, CancellationToken ct = default)
        => await _db.AISessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.LinkedEntityId == linkedEntityId, ct);

    public async Task AddAsync(AISession session, CancellationToken ct = default)
        => await _db.AISessions.AddAsync(session, ct);

    public void Update(AISession session) => _db.AISessions.Update(session);
    public void AddMessages(IEnumerable<AIMessage> messages) => _db.AIMessages.AddRange(messages);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
