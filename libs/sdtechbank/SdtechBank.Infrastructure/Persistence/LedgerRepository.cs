using MongoDB.Driver;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Infrastructure.MongoDB;

namespace SdtechBank.Infrastructure.Persistence;

public class LedgerRepository(MongoDbContext context) : ILedgerRepository
{

    private readonly IMongoCollection<LedgerEntry> _collection = context.GetCollection<LedgerEntry>("ledger-entries");

    public async Task AddRangeAsync(IEnumerable<LedgerEntry> entries)
    {
        await _collection.InsertManyAsync(entries);
    }

    public async Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId)
    {
        var filter = Builders<LedgerEntry>.Filter.Eq(o => o.AccountId, accountId);
        return await _collection.Find(filter).ToListAsync();
    }
}
