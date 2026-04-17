
using SdtechBank.Application.Accounts.Contracts;
using StackExchange.Redis;

namespace SdtechBank.Infrastructure.Shared.Concurrency;

public class RedisAccountLockService(IConnectionMultiplexer redis) : IAccountLockService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan AcquireTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan RenewInterval = TimeSpan.FromSeconds(5);
    public async Task<IDisposable> AcquireLockAsync(Guid accountId, CancellationToken ct)
    {
        var key = $"lock:account:{accountId}";
        var token = Guid.NewGuid().ToString();
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        timeoutCts.CancelAfter(AcquireTimeout);

        while (!timeoutCts.Token.IsCancellationRequested)
        {
            var acquired = await _db.StringSetAsync(key, token, LockTtl, When.NotExists);

            if (acquired)
                return new RedisLock(_db, key, token, RenewInterval, LockTtl);

            await Task.Delay(50,ct);
        }
        throw new TimeoutException($"Timeout acquiring lock for account {accountId}");
    }

    private sealed class RedisLock : IDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _token;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _renewTask;

        public RedisLock(IDatabase db, string key, string token, TimeSpan renewInterval, TimeSpan ttl)
        {
            _db = db;
            _key = key;
            _token = token;

            _renewTask = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(renewInterval, _cts.Token);

                    await RenewAsync(ttl);
                }
            });
        }

        private async Task RenewAsync(TimeSpan ttl)
        {
            var value = await _db.StringGetAsync(_key);

            if (value == _token)
                await _db.KeyExpireAsync(_key, ttl);
        }
        private async Task ReleaseAsync()
        {
            const string script = """
                if redis.call("get", KEYS[1]) == ARGV[1]
                then
                    return redis.call("del",KEYS[1])
                else
                    return 0
                end
                """;
            await _db.ScriptEvaluateAsync(script, keys: [_key], values: [_token]);
        }

        public void Dispose()
        {
            _cts.Cancel();

            ReleaseAsync().GetAwaiter().GetResult();
        }
    }
}
