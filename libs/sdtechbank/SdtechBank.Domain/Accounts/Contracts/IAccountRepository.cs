using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Domain.Accounts.Contracts;

public interface IAccountRepository
{
    Task<Account?> GetByBankAccountAsync(BankAccount bankAccount, CancellationToken cancellationToken);
}
