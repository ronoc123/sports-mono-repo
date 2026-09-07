using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Data;

public class SocialMediaDbContext : ISocialMediaDbContext
{
    private readonly IMongoDatabase _database;

    public SocialMediaDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
        => _database.GetCollection<T>(collectionName);
}
