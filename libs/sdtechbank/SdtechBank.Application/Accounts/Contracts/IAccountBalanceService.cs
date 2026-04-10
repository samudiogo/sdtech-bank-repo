using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Accounts.Contracts;

public interface IAccountBalanceService
{
    Task<Money> GetBalanceAsync(Guid accountId);
}
