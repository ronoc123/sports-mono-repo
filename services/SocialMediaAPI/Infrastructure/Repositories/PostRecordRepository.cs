using Application.Common.Interfaces;
using Domain.Records;
using MongoDB.Bson;
using MongoDB.Driver;
using SportifyCore.Domain;
using SportifyCore.Persistence;

namespace Infrastructure.Repositories;

public class PostRecordRepository : MongoRepository<PostRecord, string>, IPostRecordRepository
{
    public PostRecordRepository(ISocialMediaDbContext dbContext)
        : base(dbContext.GetCollection<PostRecord>("postRecords"))
    {
    }

    public new async Task AddAsync(PostRecord entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = ObjectId.GenerateNewId().ToString();

        entity.CreatedAt = DateTime.UtcNow;

        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task<List<PostRecord>> GetRecentByChannelIdAsync(
        string channelId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await Collection
            .Find(Builders<PostRecord>.Filter.Eq(r => r.ChannelId, channelId))
            .SortByDescending(r => r.CreatedAt)
            .Limit(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<PostRecord> Records, long TotalCount)> GetPagedByChannelIdAsync(
        string channelId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<PostRecord>.Filter.Eq(r => r.ChannelId, channelId);

        var totalCount = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var records = await Collection
            .Find(filter)
            .SortByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (records, totalCount);
    }
}
