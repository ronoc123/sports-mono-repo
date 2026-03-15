using System.Linq.Expressions;
using Domain.Abstractions;

namespace Domain.Repositories;

public interface IRepository
{
    Task<TAgg?> GetByIdAsync<TAgg, TId>(TId id, CancellationToken ct = default)
        where TAgg : class, IAggregate<TId>;

    IQueryable<TAgg> Query<TAgg>(bool asNoTracking = true)
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

    Task<bool> ExistsAsync<TAgg>(Expression<Func<TAgg, bool>> predicate, CancellationToken ct = default)
        where TAgg : class;

    Task<int> CountAsync<TAgg>(Expression<Func<TAgg, bool>>? predicate = null, CancellationToken ct = default)
        where TAgg : class;

    Task AddAsync<TAgg>(TAgg entity, CancellationToken ct = default)
        where TAgg : class, IAggregate;

    void Update<TAgg>(TAgg entity)
        where TAgg : class, IAggregate;

    void Remove<TAgg>(TAgg entity)
        where TAgg : class, IAggregate;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
