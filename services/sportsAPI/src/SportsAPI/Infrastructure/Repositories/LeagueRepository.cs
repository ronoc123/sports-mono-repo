using Domain.Leagues;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class LeagueRepository
    : EfRepository<League, LeagueId>, ILeagueRepository
{
  private readonly SportsDbAppContext _db;

  public LeagueRepository(SportsDbAppContext db) : base(db)
  {
    _db = db;
  }

  // Bridge Guid -> LeagueId and reuse the base FindAsync
  public Task<League?> GetByIdAsync(LeagueId leagueId)
      => FindAsync(leagueId);

  public Task AddLeagueAsync(League league)
      => _db.Set<League>().AddAsync(league).AsTask();

  public Task UpdateLeagueAsync(League league)
  {
    _db.Set<League>().Update(league);
    return Task.CompletedTask;
  }

  public async Task DeleteLeagueAsync(LeagueId leagueId)
  {
    var entity = await FindAsync(leagueId);
    if (entity is null) return;
    _db.Set<League>().Remove(entity);
  }

}
