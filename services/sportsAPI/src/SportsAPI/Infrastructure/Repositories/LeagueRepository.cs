using Domain.Leagues;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LeagueRepository : ILeagueRepository
{
    private readonly SportsDbAppContext _db;

    public LeagueRepository(SportsDbAppContext db)
    {
        _db = db;
    }

    public async Task<League?> GetByIdAsync(Guid leagueId)
    {
        // Create the LeagueId value object for comparison
        var leagueIdVO = LeagueId.Of(leagueId);
        return await _db.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueIdVO);
    }

    public async Task<List<League>> GetLeaguesByOrganizationIdAsync(Guid organizationId)
    {
        var organizationIdVO = OrganizationId.Of(organizationId);
        return await _db.Leagues
            .Where(l => l.Organization.Any(o => o.Id == organizationIdVO))
            .ToListAsync();
    }

    public async Task AddLeagueAsync(League league)
    {
        await _db.Leagues.AddAsync(league);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateLeagueAsync(League league)
    {
        _db.Leagues.Update(league);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteLeagueAsync(Guid leagueId)
    {
        var leagueIdVO = LeagueId.Of(leagueId);
        var league = await _db.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueIdVO);

        if (league != null)
        {
            _db.Leagues.Remove(league);
            await _db.SaveChangesAsync();
        }
    }
}
