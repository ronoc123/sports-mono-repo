using Application.Common.Interfaces;
using Domain.Channel;
using MongoDB.Bson;
using MongoDB.Driver;
using SportifyCore.Domain;
using SportifyCore.Persistence;

namespace Infrastructure.Repositories;

public class ChannelRepository : MongoRepository<Channel, string>, IRepository<Channel, string>
{
    public ChannelRepository(ISocialMediaDbContext dbContext)
        : base(dbContext.GetCollection<Channel>("channels"))
    {
    }

    public new async Task AddAsync(Channel entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = ObjectId.GenerateNewId().ToString();

        entity.CreatedAt = DateTime.UtcNow;

        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }
}
