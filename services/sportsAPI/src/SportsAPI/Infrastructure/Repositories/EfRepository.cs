using Domain.Abstractions;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public Task<TAgg?> FindAsync(TId id, CancellationToken ct = default)
        => _set.FindAsync(new object?[] { id! }, ct).AsTask();
  }
}
