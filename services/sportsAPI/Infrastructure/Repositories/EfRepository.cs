using System.Linq.Expressions;
using Domain.Abstractions;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
  public class EfRepository<TAgg, TId> : IRepository<TAgg, TId>
    where TAgg : class, IAggregate<TId>
  {
    protected readonly DbContext _db;
    protected readonly DbSet<TAgg> _set;

    public EfRepository(DbContext db)
    {
      _db = db;
      _set = db.Set<TAgg>();
    }

    public async Task<TAgg?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
      // Case 1 — primitive or simple scalar IDs (Guid, int, string)
      var idType = typeof(TId);

      if (idType.IsPrimitive ||
          idType == typeof(Guid) ||
          idType == typeof(string))
      {
        // Old behavior preserved 100%
        return await _set.FindAsync(new object?[] { id! }, ct).AsTask();
      }

      // Case 2 — Value Object IDs
      var props = idType.GetProperties();

      // If VO has no public properties (rare), fallback to old behavior
      if (props.Length == 0)
      {
        return await _set.FindAsync(new object?[] { id! }, ct).AsTask();
      }

      // Extract VO property values in the declared order
      object?[] keyValues = props.Select(p => p.GetValue(id)).ToArray();

      return await _set.FindAsync(keyValues, ct).AsTask();
    }

    public async Task<TAgg?> GetByIdAsync(CancellationToken ct = default, params object[] keyValues)
      => await _set.FindAsync(keyValues, ct).AsTask();

    public IQueryable<TAgg> Query(bool asNoTracking = true)
      => asNoTracking ? _set.AsNoTracking() : _set;

    public async Task<IReadOnlyList<TAgg>> ListAsync(
      Expression<Func<TAgg, bool>>? filter = null,
      Func<IQueryable<TAgg>, IOrderedQueryable<TAgg>>? orderBy = null,
      int? skip = null,
      int? take = null,
      bool asNoTracking = true,
      CancellationToken ct = default,
      params Expression<Func<TAgg, object>>[] includes)
    {
      IQueryable<TAgg> q = _set;

      if (asNoTracking) q = q.AsNoTracking();

      if (includes is { Length: > 0 })
      {
        foreach (var include in includes)
          q = q.Include(include);
      }

      if (filter is not null) q = q.Where(filter);
      if (orderBy is not null) q = orderBy(q);
      if (skip.HasValue) q = q.Skip(skip.Value);
      if (take.HasValue) q = q.Take(take.Value);

      return await q.ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(Expression<Func<TAgg, bool>> predicate, CancellationToken ct = default)
      => _set.AsNoTracking().AnyAsync(predicate, ct);

    public Task<int> CountAsync(Expression<Func<TAgg, bool>>? predicate = null, CancellationToken ct = default)
      => predicate is null
         ? _set.CountAsync(ct)
         : _set.CountAsync(predicate, ct);

    public async Task AddAsync(TAgg entity, CancellationToken ct = default)
      => await _set.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<TAgg> entities, CancellationToken ct = default)
      => await _set.AddRangeAsync(entities, ct);

    public void Update(TAgg entity) => _set.Update(entity);

    public void Remove(TAgg entity) => _set.Remove(entity);

    public void RemoveRange(IEnumerable<TAgg> entities) => _set.RemoveRange(entities);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
      => _db.SaveChangesAsync(ct);
  }
}
