using MongoDB.Driver;
using SdtechBank.Domain.Accounts;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Accounts;

public class AccountRepository(MongoDbContext context) : IAccountRepository
{
    private readonly IMongoCollection<Account> _collection = context.GetCollection<Account>("accounts");

    public async Task<Account?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken)
    {
        var filter = Builders<Account>.Filter.Eq(o => o.AccountCode, accountCode);

        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Account?> GetByBankAccountAsync(BankAccount bankAccount, CancellationToken cancellationToken)
    {
        var builder = Builders<Account>.Filter;
        var filters = builder.And(
            builder.Eq(a => a.Document, bankAccount.Document),
            builder.Eq(a => a.BankCode, bankAccount.BankCode),
            builder.Eq(a => a.Branch, bankAccount.Branch),
            builder.Eq(a => a.AccountCode, bankAccount.Account));

        return await _collection.Find(filters).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAsync(Account account, CancellationToken cancellationToken)
    {
        var filter = Builders<Account>.Filter.Eq(o => o.Id, account.Id);
        var options = new ReplaceOptions { IsUpsert = true };
        await _collection.ReplaceOneAsync(filter, account, options, cancellationToken);
    }
}
