using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
  public class EfReadRepository<TEntity, TId> : IReadRepository<TEntity, TId>
    where TEntity : class
  {
    private readonly DbContext _db;
    public EfReadRepository(DbContext db) => _db = db;

    public IQueryable<TEntity> Query(bool asNoTracking = true)
      => asNoTracking ? _db.Set<TEntity>().AsNoTracking() : _db.Set<TEntity>();

    public async Task<IReadOnlyList<TEntity>> ListAsync(
      Expression<Func<TEntity, bool>>? filter = null,
      Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
      int? skip = null,
      int? take = null,
      bool asNoTracking = true,
      CancellationToken ct = default,
      params Expression<Func<TEntity, object>>[] includes)
    {
      IQueryable<TEntity> q = Query(asNoTracking);

      if (includes?.Length > 0)
        q = includes.Aggregate(q, (cur, inc) => cur.Include(inc));

      if (filter != null) q = q.Where(filter);
      if (orderBy != null) q = orderBy(q);
      if (skip.HasValue) q = q.Skip(skip.Value);
      if (take.HasValue) q = q.Take(take.Value);

      return await q.ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> p, CancellationToken ct = default)
      => Query().AnyAsync(p, ct);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? p = null, CancellationToken ct = default)
      => p is null ? Query().CountAsync(ct) : Query().CountAsync(p, ct);
  }

}
