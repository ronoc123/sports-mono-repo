using Application.Common.Interfaces;
using Domain.VideoGenerationJob;
using MongoDB.Bson;
using MongoDB.Driver;
using SportifyCore.Domain;
using SportifyCore.Persistence;

namespace Infrastructure.Repositories;

public class VideoGenerationJobRepository
    : MongoRepository<VideoGenerationJob, string>, IVideoGenerationJobRepository
{
    public VideoGenerationJobRepository(ISocialMediaDbContext dbContext)
        : base(dbContext.GetCollection<VideoGenerationJob>("videoGenerationJobs"))
    {
    }

    public new async Task AddAsync(VideoGenerationJob entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = ObjectId.GenerateNewId().ToString();

        entity.CreatedAt = DateTime.UtcNow;

        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task<List<VideoGenerationJob>> GetStaleJobsAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        var staleStatuses = new[] { "Queued", "Generating" };

        var filter = Builders<VideoGenerationJob>.Filter.And(
            Builders<VideoGenerationJob>.Filter.In(j => j.Status, staleStatuses),
            Builders<VideoGenerationJob>.Filter.Lt(j => j.CreatedAt, olderThan));

        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }
}
