using Application.Common.Interfaces;
using Domain.PostCycle;
using MongoDB.Bson;
using MongoDB.Driver;
using SportifyCore.Domain;
using SportifyCore.Persistence;

namespace Infrastructure.Repositories;

public class PostCycleRepository : MongoRepository<PostCycleJob, string>, IPostCycleRepository
{
    public PostCycleRepository(ISocialMediaDbContext dbContext)
        : base(dbContext.GetCollection<PostCycleJob>("postCycleJobs"))
    {
    }

    public new async Task AddAsync(PostCycleJob entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = ObjectId.GenerateNewId().ToString();

        entity.CreatedAt = DateTime.UtcNow;

        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task<List<PostCycleJob>> GetStaleRunningJobsAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<PostCycleJob>.Filter.And(
            Builders<PostCycleJob>.Filter.Eq(j => j.Status, "Running"),
            Builders<PostCycleJob>.Filter.Lt(j => j.CreatedAt, olderThan));

        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }
}
