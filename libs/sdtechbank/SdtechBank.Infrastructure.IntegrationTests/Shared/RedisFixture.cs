
using Testcontainers.Redis;
namespace SdtechBank.Infrastructure.IntegrationTests.Shared;

public sealed class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder("redis:8").Build();
    public string ConnectionString => _container.GetConnectionString();



    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync(); ;
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
