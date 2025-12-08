using System.Linq.Expressions;
using Domain.Abstractions;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class Repository : IRepository
{
  private readonly SportsDbAppContext _db;

  public Repository(SportsDbAppContext db) => _db = db;

  // ---------- Single aggregate by ID ----------

  public async Task<TAgg?> GetByIdAsync<TAgg, TId>(
      TId id,
      CancellationToken ct = default)
      where TAgg : class, IAggregate<TId>
      => await _db.Set<TAgg>()
                  .FindAsync(new object?[] { id! }, ct)
                  .AsTask();

  public async Task<TAgg?> GetByIdAsync<TAgg>(
      CancellationToken ct = default,
      params object[] keyValues)
      where TAgg : class
      => await _db.Set<TAgg>()
                  .FindAsync(keyValues, ct)
                  .AsTask();

  // ---------- Querying ----------

  public IQueryable<TAgg> Query<TAgg>(bool asNoTracking = true)
      where TAgg : class
      => asNoTracking
          ? _db.Set<TAgg>().AsNoTracking()
          : _db.Set<TAgg>();


  public async Task<IReadOnlyList<TAgg>> ListAsync<TAgg>(
      Expression<Func<TAgg, bool>>? filter = null,
      Func<IQueryable<TAgg>, IOrderedQueryable<TAgg>>? orderBy = null,
      int? skip = null,
      int? take = null,
      bool asNoTracking = true,
      CancellationToken ct = default,
      params Expression<Func<TAgg, object>>[] includes)
      where TAgg : class
  {
    IQueryable<TAgg> q = _db.Set<TAgg>();

    if (asNoTracking) q = q.AsNoTracking();

    if (includes is { Length: > 0 })
    {
      foreach (var include in includes)
        q = q.Include(include);
    }

    if (filter is not null)
      q = q.Where(filter);

    if (orderBy is not null)
      q = orderBy(q);

    if (skip.HasValue)
      q = q.Skip(skip.Value);

    if (take.HasValue)
      q = q.Take(take.Value);

    return await q.ToListAsync(ct);
  }

  public Task<bool> ExistsAsync<TAgg>(
      Expression<Func<TAgg, bool>> predicate,
      CancellationToken ct = default)
      where TAgg : class
      => _db.Set<TAgg>()
            .AsNoTracking()
            .AnyAsync(predicate, ct);

  public Task<int> CountAsync<TAgg>(
      Expression<Func<TAgg, bool>>? predicate = null,
      CancellationToken ct = default)
      where TAgg : class
      => predicate is null
          ? _db.Set<TAgg>().CountAsync(ct)
          : _db.Set<TAgg>().CountAsync(predicate, ct);

  // ---------- Commands (write) ----------

  public Task AddAsync<TAgg>(
      TAgg entity,
      CancellationToken ct = default)
      where TAgg : class, IAggregate
      => _db.Set<TAgg>().AddAsync(entity, ct).AsTask();

  public Task AddRangeAsync<TAgg>(
      IEnumerable<TAgg> entities,
      CancellationToken ct = default)
      where TAgg : class, IAggregate
      => _db.Set<TAgg>().AddRangeAsync(entities, ct);

  public void Update<TAgg>(TAgg entity)
      where TAgg : class, IAggregate
      => _db.Set<TAgg>().Update(entity);

  public void Remove<TAgg>(TAgg entity)
      where TAgg : class, IAggregate
      => _db.Set<TAgg>().Remove(entity);

  public void RemoveRange<TAgg>(IEnumerable<TAgg> entities)
      where TAgg : class, IAggregate
      => _db.Set<TAgg>().RemoveRange(entities);

  public Task<int> SaveChangesAsync(CancellationToken ct = default)
      => _db.SaveChangesAsync(ct);
}
