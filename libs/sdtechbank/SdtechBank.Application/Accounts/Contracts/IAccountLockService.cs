namespace SdtechBank.Application.Accounts.Contracts;

public interface IAccountLockService
{
    Task<IAsyncDisposable> AcquireLockAsync(Guid accountId, CancellationToken ct);
}
