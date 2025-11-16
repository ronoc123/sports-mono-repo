using Domain.Leagues;
using Domain.Organizations;
using System.Linq.Expressions;

namespace Domain.Repositories
{

  public interface IRepository<TAgg, TId>
      where TAgg : class, IAggregate<TId>
  {
    Task<TAgg?> GetByIdAsync(TId id, CancellationToken ct = default);

    Task<TAgg?> GetByIdAsync(CancellationToken ct = default, params object[] keyValues);

    IQueryable<TAgg> Query(bool asNoTracking = true);

    Task<IReadOnlyList<TAgg>> ListAsync(
      Expression<Func<TAgg, bool>>? filter = null,
      Func<IQueryable<TAgg>, IOrderedQueryable<TAgg>>? orderBy = null,
      int? skip = null,
      int? take = null,
      bool asNoTracking = true,
      CancellationToken ct = default,
      params Expression<Func<TAgg, object>>[] includes);

    Task<bool> ExistsAsync(Expression<Func<TAgg, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<TAgg, bool>>? predicate = null, CancellationToken ct = default);

    Task AddAsync(TAgg entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TAgg> entities, CancellationToken ct = default);
    void Update(TAgg entity);
    void Remove(TAgg entity);
    void RemoveRange(IEnumerable<TAgg> entities);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
  }

  public interface IReadRepository<TEntity, TId>
  where TEntity : class
  {
    IQueryable<TEntity> Query(bool asNoTracking = true);

    Task<IReadOnlyList<TEntity>> ListAsync(
      Expression<Func<TEntity, bool>>? filter = null,
      Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
      int? skip = null,
      int? take = null,
      bool asNoTracking = true,
      CancellationToken ct = default,
      params Expression<Func<TEntity, object>>[] includes);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);
  }


  public interface IOrganizationRepository : IRepository<Organization, OrganizationId>
  {
    Task<Organization?> GetOrganizationByIdAsync(OrganizationId organizationId, CancellationToken ct = default);
    Task<List<Organization>> GetAllOrganizationsAsync(CancellationToken ct = default);
    Task AddOrganizationAsync(Organization organization, CancellationToken ct = default);
    Task UpdateOrganizationAsync(Organization organization, CancellationToken ct = default);
    Task DeleteOrganizationAsync(OrganizationId organizationId, CancellationToken ct = default);
  }
  public interface ILeagueRepository : IRepository<League, LeagueId>
  {
    Task<League?> GetLeagueByIdAsync(LeagueId leagueId, CancellationToken ct = default);
    Task AddLeagueAsync(League league, CancellationToken ct = default);
    Task UpdateLeagueAsync(League league, CancellationToken ct = default);
    Task DeleteLeagueAsync(LeagueId leagueId, CancellationToken ct = default);
  }
}
