namespace SdtechBank.Application.Accounts.Contracts;

public interface IAccountLockService
{
    Task<IDisposable> AcquireLockAsync(Guid accountId, CancellationToken ct);
}
