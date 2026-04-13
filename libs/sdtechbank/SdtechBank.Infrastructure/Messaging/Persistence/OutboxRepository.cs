using MongoDB.Driver;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.Shared.Messaging;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Messaging.Persistence;

public class OutboxRepository(MongoDbContext context) : IOutboxRepository
{
    private readonly IMongoCollection<OutboxMessage> _collection = context.GetCollection<OutboxMessage>("outbox_messages");
    public async Task AddAsync(OutboxMessage message, CancellationToken ct)
    {
        if (context.Session is not null)
            await _collection.InsertOneAsync(context.Session, message, cancellationToken: ct);
        else
            await _collection.InsertOneAsync(message, cancellationToken: ct);
    }

    public async Task<IList<OutboxMessage>> GetUnprocessedMessagesAsync(int limit, CancellationToken ct)
    {
        var filter = Builders<OutboxMessage>.Filter.And(
                Builders<OutboxMessage>.Filter.Eq(x => x.ProcessedAt, null),
                Builders<OutboxMessage>.Filter.Ne(x => x.Status, OutboxStatus.Failed));

        return await _collection.Find(filter).Limit(limit).ToListAsync(ct);
    }

    public async Task SaveAsync(OutboxMessage message, CancellationToken ct)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(o => o.Id, message.Id);
        var opts = new ReplaceOptions { IsUpsert = true };

        if (context.Session is not null)
            await _collection.ReplaceOneAsync(context.Session, filter, message, opts, ct);
        else
            await _collection.ReplaceOneAsync(filter, message, opts, ct);
    }
}
