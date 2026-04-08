using MongoDB.Driver;
using SdtechBank.Domain.Accounts;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Accounts;

internal class AccountRepository(MongoDbContext context) : IAccountRepository
{
    private readonly IMongoCollection<Account> _collections = context.GetCollection<Account>("accounts");
    public async Task<Account?> GetByBankAccountAsync(BankAccount bankAccount, CancellationToken cancellationToken)
    {
        var builder = Builders<Account>.Filter;
        var filters = builder.And(
            builder.Eq(a => a.Cpf, bankAccount.Cpf),
            builder.Eq(a => a.BankCode, bankAccount.BankCode),
            builder.Eq(a => a.Branch, bankAccount.Branch),
            builder.Eq(a => a.AccountCode, bankAccount.Account));

        return await _collections.Find(filters).FirstOrDefaultAsync(cancellationToken);
    }
}
