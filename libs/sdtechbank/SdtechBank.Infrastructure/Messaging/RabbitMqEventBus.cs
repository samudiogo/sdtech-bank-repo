using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SdtechBank.Application.Common.Contracts;
using System.Text;
using System.Text.Json;

namespace SdtechBank.Infrastructure.Messaging;

public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqEventBus(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task PublishAsync<T>(T @event) where T : IDomainIntegrationEvent
    {
        await EnsureChannelAsync();
        await using var channel = await _connection!.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: _settings.Exchange, type: ExchangeType.Direct, durable: true);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent, //abordagem para sobreviver a restarts do broker
            MessageId = @event.EventId.ToString(),
            Timestamp = new AmqpTimestamp(@event.OccurredAt.ToUnixTimeSeconds()),
            CorrelationId = @event.CorrelationId,
        };

        await channel.BasicPublishAsync(exchange: _settings.Exchange, routingKey: @event.RoutingKey, mandatory: false, basicProperties: props, body: body);

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
