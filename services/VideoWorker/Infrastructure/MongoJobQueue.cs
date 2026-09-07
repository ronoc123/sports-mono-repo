using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace VideoWorker.Infrastructure;

public class MongoJobQueue
{
    private readonly IMongoCollection<RenderJobDocument> _collection;
    private readonly ILogger<MongoJobQueue> _logger;

    public MongoJobQueue(IConfiguration configuration, ILogger<MongoJobQueue> logger)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString is required.");
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "SocialMediaDb";

        var client   = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection  = database.GetCollection<RenderJobDocument>("render_jobs");

        _logger = logger;
    }

    /// <summary>
    /// Atomically claims the oldest Pending job.
    /// Uses findOneAndUpdate — safe for concurrent workers.
    /// Returns null when no Pending jobs exist.
    /// </summary>
    public async Task<RenderJobDocument?> TryClaimNextJobAsync(
        string workerId,
        CancellationToken cancellationToken)
    {
        var filter = Builders<RenderJobDocument>.Filter.Eq(j => j.Status, "Pending");
        var sort   = Builders<RenderJobDocument>.Sort.Ascending(j => j.CreatedAt);
        var update = Builders<RenderJobDocument>.Update
            .Set(j => j.Status,    "Processing")
            .Set(j => j.WorkerId,  workerId)
            .Set(j => j.StartedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<RenderJobDocument>
        {
            Sort           = sort,
            ReturnDocument = ReturnDocument.After,
            IsUpsert       = false,
        };

        var job = await _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

        if (job != null)
            _logger.LogInformation("Claimed job {JobId} (model={Model})", job.Id, job.Model);

        return job;
    }

    public async Task MarkCompletedAsync(
        string jobId,
        string outputVideoKey,
        CancellationToken cancellationToken)
    {
        var filter = Builders<RenderJobDocument>.Filter.Eq(j => j.Id, jobId);
        var update = Builders<RenderJobDocument>.Update
            .Set(j => j.Status,         "Completed")
            .Set(j => j.OutputVideoKey, outputVideoKey)
            .Set(j => j.CompletedAt,    DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        _logger.LogInformation("Job {JobId} marked Completed", jobId);
    }

    public async Task MarkFailedAsync(
        string jobId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var filter = Builders<RenderJobDocument>.Filter.Eq(j => j.Id, jobId);
        var update = Builders<RenderJobDocument>.Update
            .Set(j => j.Status,       "Failed")
            .Set(j => j.ErrorMessage, errorMessage)
            .Set(j => j.CompletedAt,  DateTime.UtcNow)
            .Inc(j => j.RetryCount,   1);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        _logger.LogWarning("Job {JobId} marked Failed: {Error}", jobId, errorMessage);
    }

    /// <summary>Writes a heartbeat timestamp so stale detection can tell an active job from a crashed one.</summary>
    public async Task UpdateHeartbeatAsync(string jobId, CancellationToken cancellationToken)
    {
        var filter = Builders<RenderJobDocument>.Filter.Eq(j => j.Id, jobId);
        var update = Builders<RenderJobDocument>.Update
            .Set(j => j.LastHeartbeatAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Returns Processing jobs whose last heartbeat (or startedAt if no heartbeat yet)
    /// is older than the given threshold.
    /// </summary>
    public async Task<List<RenderJobDocument>> GetStaleProcessingJobsAsync(
        DateTime olderThan,
        CancellationToken cancellationToken)
    {
        // A job is stale when both the heartbeat AND startedAt are older than the threshold
        // (covers jobs that never wrote a heartbeat and jobs whose worker died mid-flight)
        var noHeartbeatFilter = Builders<RenderJobDocument>.Filter.And(
            Builders<RenderJobDocument>.Filter.Eq(j => j.Status, "Processing"),
            Builders<RenderJobDocument>.Filter.Exists(j => j.LastHeartbeatAt, false),
            Builders<RenderJobDocument>.Filter.Lt(j => j.StartedAt, olderThan));

        var staleHeartbeatFilter = Builders<RenderJobDocument>.Filter.And(
            Builders<RenderJobDocument>.Filter.Eq(j => j.Status, "Processing"),
            Builders<RenderJobDocument>.Filter.Lt(j => j.LastHeartbeatAt, olderThan));

        var combinedFilter = Builders<RenderJobDocument>.Filter.Or(noHeartbeatFilter, staleHeartbeatFilter);

        return await _collection.Find(combinedFilter).ToListAsync(cancellationToken);
    }

    /// <summary>Resets a stale Processing job back to Pending.</summary>
    public async Task RequeueAsync(string jobId, CancellationToken cancellationToken)
    {
        var filter = Builders<RenderJobDocument>.Filter.Eq(j => j.Id, jobId);
        var update = Builders<RenderJobDocument>.Update
            .Set(j => j.Status,    "Pending")
            .Unset(j => j.WorkerId)
            .Unset(j => j.StartedAt)
            .Inc(j => j.RetryCount, 1);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        _logger.LogInformation("Job {JobId} requeued to Pending", jobId);
    }
}
