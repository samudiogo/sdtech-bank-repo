using MongoDB.Driver;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Transactions.Persistence;

public class TransactionRepository(MongoDbContext context) : ITransactionRepository
{
    private readonly IMongoCollection<Transaction> _collection = context.GetCollection<Transaction>("pix-transactions");
    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Transaction>.Filter.Eq(o => o.Id, id);

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Transaction?> GetByIdempontencyKeyAsync(string idempontencyKey)
    {
        var filter = Builders<Transaction>.Filter.Eq(o => o.IdempotencyKey, idempontencyKey);

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task SaveAsync(Transaction transaction)
    {
        var filter = Builders<Transaction>.Filter.Eq(o => o.Id, transaction.Id);
        var options = new ReplaceOptions { IsUpsert = true };
        await _collection.ReplaceOneAsync(filter, transaction, options);
    }
}
