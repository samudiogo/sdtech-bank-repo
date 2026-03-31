using MongoDB.Driver;
namespace SdtechBank.Infrastructure.Shared.Mongo;

public class MongoDbContext(IMongoClient mongoClient, string database)
{
    private readonly IMongoDatabase _database = mongoClient.GetDatabase(database);

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}

