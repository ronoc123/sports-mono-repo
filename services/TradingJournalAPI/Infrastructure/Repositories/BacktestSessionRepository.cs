using Domain.BacktestIntegrity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class BacktestSessionRepository : IBacktestSessionRepository
{
    private readonly TradingJournalDbContext _db;

    public BacktestSessionRepository(TradingJournalDbContext db) => _db = db;

    public async Task<BacktestSession?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.BacktestSessions
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<BacktestSession?> GetByIdAsync(BacktestSessionId id, CancellationToken ct = default)
        => await _db.BacktestSessions
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(BacktestSession session, CancellationToken ct = default)
        => await _db.BacktestSessions.AddAsync(session, ct);

    public void Update(BacktestSession session) => _db.BacktestSessions.Update(session);

    public void ReplaceAnswers(IEnumerable<IntegrityAnswer> oldAnswers, IEnumerable<IntegrityAnswer> newAnswers)
    {
        _db.IntegrityAnswers.RemoveRange(oldAnswers);
        _db.IntegrityAnswers.AddRange(newAnswers);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
