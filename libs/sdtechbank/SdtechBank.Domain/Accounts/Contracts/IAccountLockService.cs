namespace SdtechBank.Domain.Accounts.Contracts;

public interface IAccountLockService
{
    Task<IDisposable> AcquireLockAsync(Guid accountId);
}
