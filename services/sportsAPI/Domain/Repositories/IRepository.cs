using System.Linq.Expressions;

namespace Domain.Repositories;

public interface IRepository
{

    Task<TAgg?> GetByIdAsync<TAgg, TId>(
        TId id,
        CancellationToken ct = default)
        where TAgg : class, IAggregate<TId>;

    Task<TAgg?> GetByIdAsync<TAgg>(
        CancellationToken ct = default,
        params object[] keyValues)
        where TAgg : class;

    // ---------- Querying ----------

    IQueryable<TAgg> Query<TAgg>(bool asNoTracking = true)
        where TAgg : class;

    IQueryable<TAgg> Find<TAgg>(bool asNoTracking = false)
      where TAgg : class;


    Task<IReadOnlyList<TAgg>> ListAsync<TAgg>(
      Expression<Func<TAgg, bool>>? filter = null,
      Func<IQueryable<TAgg>, IOrderedQueryable<TAgg>>? orderBy = null,
      int? skip = null,
      int? take = null,
      bool asNoTracking = true,
      CancellationToken ct = default,
      params Expression<Func<TAgg, object>>[] includes)
      where TAgg : class;

    Task<bool> ExistsAsync<TAgg>(
        Expression<Func<TAgg, bool>> predicate,
        CancellationToken ct = default)
        where TAgg : class;

    Task<int> CountAsync<TAgg>(
        Expression<Func<TAgg, bool>>? predicate = null,
        CancellationToken ct = default)
        where TAgg : class;

    // ---------- Commands (write) – no TId needed ----------

    Task AddAsync<TAgg>(
        TAgg entity,
        CancellationToken ct = default)
        where TAgg : class, IAggregate;

    Task AddRangeAsync<TAgg>(
        IEnumerable<TAgg> entities,
        CancellationToken ct = default)
        where TAgg : class, IAggregate;

    void Update<TAgg>(TAgg entity)
        where TAgg : class, IAggregate;

    void Remove<TAgg>(TAgg entity)
        where TAgg : class, IAggregate;

    void RemoveRange<TAgg>(IEnumerable<TAgg> entities)
        where TAgg : class, IAggregate;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
