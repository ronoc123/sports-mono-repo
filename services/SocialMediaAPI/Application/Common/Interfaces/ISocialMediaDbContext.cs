using MongoDB.Driver;

namespace Application.Common.Interfaces;

public interface ISocialMediaDbContext
{
    IMongoCollection<T> GetCollection<T>(string collectionName);
}
