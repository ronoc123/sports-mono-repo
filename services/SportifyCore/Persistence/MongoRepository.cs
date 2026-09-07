using MongoDB.Driver;
using SportifyCore.Domain;

namespace SportifyCore.Persistence;

public abstract class MongoRepository<T, TId> : IRepository<T, TId>
    where T : Entity<TId>
{
    protected readonly IMongoCollection<T> Collection;

    protected MongoRepository(IMongoCollection<T> collection)
    {
        Collection = collection;
    }

    public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
        await Collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, id);
        await Collection.DeleteOneAsync(filter, cancellationToken);
    }
}
