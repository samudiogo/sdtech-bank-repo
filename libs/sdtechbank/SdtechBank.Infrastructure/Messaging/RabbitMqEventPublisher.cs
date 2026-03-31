using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SdtechBank.Application.Common.Contracts;
using System.Text;
using System.Text.Json;

namespace SdtechBank.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher(IOptions<RabbitMqSettings> settings) : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task PublishRawAsync(string messageId, string type, string payload)
    {
        await EnsureChannelAsync();

        await using var channel = await _connection!.CreateChannelAsync();

        var envelope = new RabbitMqMessageEnvelope { Type = type, Payload = payload };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

        var props = new BasicProperties { MessageId = messageId, ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent };

        await channel.BasicPublishAsync(exchange: _settings.Exchange, routingKey: type, mandatory: false, basicProperties: props, body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _lock.Dispose();
    }

    private async Task EnsureChannelAsync()
    {
        if (_connection is { IsOpen: true }) return;

        await _lock.WaitAsync();
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync();

        }
        finally
        {
            _lock.Release();
        }
    }
}
