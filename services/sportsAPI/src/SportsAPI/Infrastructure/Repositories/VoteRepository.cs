using Domain.Repositories;
using Domain.User.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VoteRepository : IVoteRepository
{
    private readonly SportsDbAppContext _db;

    public VoteRepository(SportsDbAppContext db)
    {
        _db = db;
    }

    public async Task<Vote?> GetVoteByIdAsync(Guid voteId)
    {
        return await _db.Votes
            .FirstOrDefaultAsync(v => v.Id.Value == voteId);
    }

    public async Task<List<Vote>> GetVotesByUserIdAsync(Guid userId)
    {
        return await _db.Votes
            .Where(v => _db.Users.Any(u => u.Id.Value == userId && u.Votes.Contains(v)))
            .ToListAsync();
    }

    public async Task AddVoteAsync(Vote vote)
    {
        await _db.Votes.AddAsync(vote);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateVoteAsync(Vote vote)
    {
        _db.Votes.Update(vote);
        await _db.SaveChangesAsync();
    }
}
