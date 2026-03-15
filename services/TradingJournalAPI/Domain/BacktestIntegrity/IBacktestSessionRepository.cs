namespace Domain.BacktestIntegrity;

public interface IBacktestSessionRepository
{
    Task<BacktestSession?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<BacktestSession?> GetByIdAsync(BacktestSessionId id, CancellationToken ct = default);
    Task AddAsync(BacktestSession session, CancellationToken ct = default);
    void Update(BacktestSession session);
    void ReplaceAnswers(IEnumerable<IntegrityAnswer> oldAnswers, IEnumerable<IntegrityAnswer> newAnswers);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
