using Domain.TradeJournal;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class TradeRepository : ITradeRepository
{
    private readonly TradingJournalDbContext _db;

    public TradeRepository(TradingJournalDbContext db) => _db = db;

    public async Task<Trade?> GetByIdAsync(TradeId id, CancellationToken ct = default)
        => await _db.Trades.FindAsync(new object[] { id }, ct);

    public async Task<Trade?> GetByIdWithEntriesAsync(TradeId id, CancellationToken ct = default)
        => await _db.Trades
            .Include(t => t.JournalEntries)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Trade>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Trades
            .Include(t => t.JournalEntries)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<int> CountRecentByUserIdAsync(Guid userId, int days, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return await _db.Trades
            .AsNoTracking()
            .CountAsync(t => t.UserId == userId && t.CreatedAt >= since, ct);
    }

    public async Task AddAsync(Trade trade, CancellationToken ct = default)
        => await _db.Trades.AddAsync(trade, ct);

    public void Update(Trade trade) => _db.Trades.Update(trade);
    public void AddEntry(JournalEntry entry) => _db.JournalEntries.Add(entry);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
