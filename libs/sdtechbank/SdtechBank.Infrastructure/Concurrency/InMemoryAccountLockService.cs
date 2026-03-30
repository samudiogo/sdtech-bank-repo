
using SdtechBank.Domain.Accounts.Contracts;

namespace SdtechBank.Infrastructure.Concurrency;

public class InMemoryAccountLockService : IAccountLockService
{
    private static readonly Dictionary<Guid, SemaphoreSlim> _locks = [];
    public async Task<IDisposable> AcquireLockAsync(Guid accountId)
    {
        SemaphoreSlim semaphore;
        lock (_locks)
        {
            if (!_locks.TryGetValue(accountId, out semaphore!))
            {
                semaphore = new(1, 1);
                _locks[accountId] = semaphore;
            }
        }
        await semaphore.WaitAsync();

        return new Releaser(() => semaphore.Release());
    }

    private sealed class Releaser(Action release) : IDisposable
    {
        public void Dispose() => release();
    }
}
