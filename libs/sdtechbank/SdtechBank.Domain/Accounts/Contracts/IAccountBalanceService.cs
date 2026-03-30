using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.Accounts.Contracts;

public interface IAccountBalanceService
{
    Task<Money> GetBalanceAsync(Guid accountId);
}
