namespace Domain.TradeJournal;

public interface ITradeRepository
{
    Task<Trade?> GetByIdAsync(TradeId id, CancellationToken ct = default);
    Task<Trade?> GetByIdWithEntriesAsync(TradeId id, CancellationToken ct = default);
    Task<IReadOnlyList<Trade>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<int> CountRecentByUserIdAsync(Guid userId, int days, CancellationToken ct = default);
    Task AddAsync(Trade trade, CancellationToken ct = default);
    void Update(Trade trade);
    void AddEntry(JournalEntry entry);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
