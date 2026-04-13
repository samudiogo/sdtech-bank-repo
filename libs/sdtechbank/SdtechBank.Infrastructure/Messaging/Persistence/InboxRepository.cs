
using MongoDB.Driver;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.OutboxInbox;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Messaging.Persistence;

public class InboxRepository(MongoDbContext context) : IInboxRepository
{
    private readonly IMongoCollection<InboxMessage> _collection = context.GetCollection<InboxMessage>("inbox_messages");

    public async Task<InboxMessage> GetInboxMessageByMessageIdAsync(string messageId, CancellationToken cancellationToken)
    {
        var filter = Builders<InboxMessage>.Filter.Eq(x => x.MessageId, messageId);
        return  await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
    public async Task AddAsync(InboxMessage message, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(message, null, cancellationToken);
    }    

    public async Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken)
    {
        var filter = Builders<InboxMessage>.Filter.Eq(x => x.MessageId, messageId);

        return await _collection.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task MarkAsFailedAsync(string messageId, CancellationToken cancellationToken)
    {        
        var message = await GetInboxMessageByMessageIdAsync(messageId, cancellationToken);
        if(message is not null)
        {
            message.MarkAsFailed();
            await SaveAsync(message, cancellationToken);
        }
    }

    public async Task MarkAsProcessedAsync(string messageId, CancellationToken cancellationToken)
    {
        var message = await GetInboxMessageByMessageIdAsync(messageId, cancellationToken);
        if(message is not null)
        {
            message.MarkAsProcessed();
            await SaveAsync(message, cancellationToken);
        }
    }

    public async Task SaveAsync(InboxMessage message, CancellationToken cancellationToken)
    {
        var filter = Builders<InboxMessage>.Filter.Eq(o => o.Id, message.Id);
        var opts = new ReplaceOptions { IsUpsert = true };
        await _collection.ReplaceOneAsync(filter, message, opts,cancellationToken);
    }
}
