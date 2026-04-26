using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Domain.Accounts.Contracts;

public interface IAccountRepository
{
    Task<Account?> GetByBankAccountAsync(BankAccount bankAccount, CancellationToken cancellationToken);
    Task<Account?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken);
    Task SaveAsync(Account account, CancellationToken cancellationToken);
}
