namespace Domain.BiweeklyReview;

public interface IReviewSessionRepository
{
    Task<ReviewSession?> GetByIdAsync(ReviewSessionId id, CancellationToken ct = default);
    Task<ReviewSession?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(ReviewSession session, CancellationToken ct = default);
    void Update(ReviewSession session);
    void AddMessages(IEnumerable<ReviewMessage> messages);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
