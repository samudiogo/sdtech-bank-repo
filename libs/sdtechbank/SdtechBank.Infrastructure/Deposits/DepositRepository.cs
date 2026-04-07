using MongoDB.Driver;
using SdtechBank.Domain.Deposits;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Deposits;

public class DepositRepository(MongoDbContext context) : IDepositRepository
{
    private readonly IMongoCollection<Deposit> _collections = context.GetCollection<Deposit>("deposits");
    public async Task SaveAsync(Deposit deposit)
    {
        var filter = Builders<Deposit>.Filter.Eq(o => o.Id, deposit.Id);
        var opts = new ReplaceOptions { IsUpsert = true };
        await _collections.ReplaceOneAsync(filter, deposit, opts);
    }
}
