using Application.Common.Interfaces;
using Domain.RenderJob;
using MongoDB.Bson;
using MongoDB.Driver;
using SportifyCore.Persistence;

namespace Infrastructure.Repositories;

public class RenderJobRepository
    : MongoRepository<RenderJob, string>, IRenderJobRepository
{
    public RenderJobRepository(ISocialMediaDbContext dbContext)
        : base(dbContext.GetCollection<RenderJob>("render_jobs"))
    {
        // Ensure index on (status, createdAt) for efficient polling queries
        var indexKeys = Builders<RenderJob>.IndexKeys
            .Ascending(j => j.Status)
            .Ascending(j => j.CreatedAt);
        Collection.Indexes.CreateOne(
            new CreateIndexModel<RenderJob>(indexKeys));
    }

    public new async Task AddAsync(RenderJob entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = ObjectId.GenerateNewId().ToString();

        entity.CreatedAt = DateTime.UtcNow;

        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task<RenderJob?> TryClaimNextJobAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<RenderJob>.Filter.Eq(j => j.Status, "Pending");
        var sort   = Builders<RenderJob>.Sort.Ascending(j => j.CreatedAt);
        var update = Builders<RenderJob>.Update
            .Set(j => j.Status,    "Processing")
            .Set(j => j.WorkerId,  workerId)
            .Set(j => j.StartedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<RenderJob>
        {
            Sort           = sort,
            ReturnDocument = ReturnDocument.After,
            IsUpsert       = false,
        };

        return await Collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
    }

    public async Task MarkCompletedAsync(
        string jobId,
        string outputVideoKey,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<RenderJob>.Filter.Eq(j => j.Id, jobId);
        var update = Builders<RenderJob>.Update
            .Set(j => j.Status,        "Completed")
            .Set(j => j.OutputVideoKey, outputVideoKey)
            .Set(j => j.CompletedAt,   DateTime.UtcNow);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task MarkFailedAsync(
        string jobId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<RenderJob>.Filter.Eq(j => j.Id, jobId);
        var update = Builders<RenderJob>.Update
            .Set(j => j.Status,       "Failed")
            .Set(j => j.ErrorMessage, errorMessage)
            .Set(j => j.CompletedAt,  DateTime.UtcNow)
            .Inc(j => j.RetryCount,   1);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<List<RenderJob>> GetStaleProcessingJobsAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<RenderJob>.Filter.And(
            Builders<RenderJob>.Filter.Eq(j => j.Status, "Processing"),
            Builders<RenderJob>.Filter.Lt(j => j.StartedAt, olderThan));

        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task RequeueAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RenderJob>.Filter.Eq(j => j.Id, jobId);
        var update = Builders<RenderJob>.Update
            .Set(j => j.Status,    "Pending")
            .Unset(j => j.WorkerId)
            .Unset(j => j.StartedAt)
            .Inc(j => j.RetryCount, 1);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<List<RenderJob>> ListByChannelAsync(
        string channelId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<RenderJob>.Filter.Eq(j => j.ChannelId, channelId);
        var sort   = Builders<RenderJob>.Sort.Descending(j => j.CreatedAt);
        return await Collection.Find(filter).Sort(sort).Limit(50).ToListAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RenderJob>.Filter.Eq(j => j.Id, jobId);
        await Collection.DeleteOneAsync(filter, cancellationToken);
    }
}
