using Testcontainers.MongoDb;

namespace SdtechBank.Infrastructure.IntegrationTests.Shared;

public sealed class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:8").Build();
    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}