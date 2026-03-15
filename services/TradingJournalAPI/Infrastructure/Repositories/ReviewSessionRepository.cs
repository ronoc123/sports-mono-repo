using Domain.BiweeklyReview;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ReviewSessionRepository : IReviewSessionRepository
{
    private readonly TradingJournalDbContext _db;

    public ReviewSessionRepository(TradingJournalDbContext db) => _db = db;

    public async Task<ReviewSession?> GetByIdAsync(ReviewSessionId id, CancellationToken ct = default)
        => await _db.ReviewSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<ReviewSession?> GetLatestByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.ReviewSessions
            .Include(s => s.Messages)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(ReviewSession session, CancellationToken ct = default)
        => await _db.ReviewSessions.AddAsync(session, ct);

    public void Update(ReviewSession session) => _db.ReviewSessions.Update(session);
    public void AddMessages(IEnumerable<ReviewMessage> messages) => _db.ReviewMessages.AddRange(messages);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
