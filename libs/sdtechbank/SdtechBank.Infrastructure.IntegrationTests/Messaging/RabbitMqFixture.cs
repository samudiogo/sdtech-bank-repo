using Testcontainers.RabbitMq;

namespace SdtechBank.Infrastructure.IntegrationTests.Messaging;

public class RabbitMqFixture : IAsyncLifetime
{
    private const string UsernameValue = "admin";
    private const string PasswordValue = "admin";
    private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:3").WithReuse(true).WithUsername(UsernameValue).WithPassword(PasswordValue).Build();


    public string Host => _container.Hostname;
    public int Port => _container.GetMappedPublicPort(5672);
    public string Username => UsernameValue;
    public string Password => PasswordValue;
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
