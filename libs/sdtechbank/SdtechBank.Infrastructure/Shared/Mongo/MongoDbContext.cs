using MongoDB.Driver;
namespace SdtechBank.Infrastructure.Shared.Mongo;

public class MongoDbContext(IMongoClient mongoClient, string database)
{
    private readonly IMongoDatabase _database = mongoClient.GetDatabase(database);
    public IClientSessionHandle? Session { get; private set; }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

    public async Task StartSessionAsync(CancellationToken ct)
    {
        Session = await mongoClient.StartSessionAsync(cancellationToken: ct);
        Session.StartTransaction();
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        if (Session is not null)
        {
            await Session.CommitTransactionAsync(ct);
            Session.Dispose();
            Session = null;
        }
    }
    public async Task RollbackAsync(CancellationToken ct)
    {
        if (Session is not null)
        {
            await Session.AbortTransactionAsync(ct);
            Session.Dispose();
            Session = null;
        }
    }
}

