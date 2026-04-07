
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace SdtechBank.Infrastructure.Messaging;

public interface IRabbitMqConnection
{
    Task<IConnection> GetConnectionAsync();
    Task<bool> IsConnectedAsync();
}

public class RabbitMqConnection(IOptions<RabbitMqSettings> options) : IRabbitMqConnection, IAsyncDisposable
{
    private readonly ConnectionFactory _factory = CreateFactory(options.Value);

    private IConnection? _connection;

    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            var connection = await GetConnectionAsync();
            return connection?.IsOpen == true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        _connection = await _factory.CreateConnectionAsync();
        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private static ConnectionFactory CreateFactory(RabbitMqSettings settings) => new()
    {
        HostName = settings.Host,
        UserName = settings.Username,
        Password = settings.Password,
        VirtualHost = settings.VirtualHost,
        AutomaticRecoveryEnabled = true
    };

}
