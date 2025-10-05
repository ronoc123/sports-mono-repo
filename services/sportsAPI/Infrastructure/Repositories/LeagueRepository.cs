using Domain.Leagues;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class LeagueRepository
  : EfRepository<League, LeagueId>, ILeagueRepository
{
  public LeagueRepository(SportsDbAppContext db) : base(db) { }

  // Named alias -> forward to base
  public Task<League?> GetLeagueByIdAsync(LeagueId leagueId, CancellationToken ct = default)
    => base.GetByIdAsync(leagueId, ct);

  public Task AddLeagueAsync(League league, CancellationToken ct = default)
    => AddAsync(league, ct);

  public Task UpdateLeagueAsync(League league, CancellationToken ct = default)
  {
    Update(league);
    return Task.CompletedTask;
  }

  public async Task DeleteLeagueAsync(LeagueId leagueId, CancellationToken ct = default)
  {
    var entity = await GetByIdAsync(leagueId, ct);
    if (entity is null) return;
    Remove(entity);
  }
}
