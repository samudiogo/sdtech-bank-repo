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
        await _collection.InsertOneAsync(message, cancellationToken: ct);
    }

    public async Task<IList<OutboxMessage>> GetUnprocessedMessagesAsync(int limit, CancellationToken ct)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(x => x.ProcessedAt, null);

        return await _collection.Find(filter).Limit(limit).ToListAsync(ct);
    }

    public async Task MarkAsProcessedAsync(Guid id, CancellationToken ct)
    {
        var update = Builders<OutboxMessage>.Update.Set(x => x.ProcessedAt, DateTime.UtcNow);
        await _collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: ct);
    }
}
