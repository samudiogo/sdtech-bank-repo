
using MongoDB.Driver;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.Shared.Messaging;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Messaging.Persistence;

public class InboxRepository(MongoDbContext context) : IInboxRepository
{
    private readonly IMongoCollection<InboxMessage> _collection = context.GetCollection<InboxMessage>("ledger-entries");
    public async Task AddAsync(InboxMessage message, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(message, null, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken)
    {
        var filter = Builders<InboxMessage>.Filter.Eq(x => x.MessageId, messageId);

        return await _collection.Find(filter).AnyAsync(cancellationToken);
    }
}
