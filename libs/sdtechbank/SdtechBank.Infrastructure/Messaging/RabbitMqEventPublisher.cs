using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SdtechBank.Application.Common.Contracts;
using System.Text;
using System.Text.Json;

namespace SdtechBank.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher(IOptions<RabbitMqSettings> settings, IRabbitMqConnection rabbitConnection, ILogger<RabbitMqEventPublisher> logger) : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private IChannel? _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task PublishRawAsync(string messageId, string type, string payload)
    {
        var channel = await GetChannelAsync();
        var envelope = new RabbitMqMessageEnvelope
        {
            MessageId = messageId,
            Type = type,
            Payload = payload,
            Attempt = 1
        };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        var props = new BasicProperties
        {
            MessageId = messageId,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(exchange: _settings.Exchange, routingKey: type, mandatory: false, basicProperties: props, body: body);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Mensagem publicada. MessageId: {MessageId}, Type: {Type}", messageId, type);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        _lock.Dispose();
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel is not null)
            return _channel;

        await _lock.WaitAsync();

        try
        {
            if (_channel is null)
            {
                var conn = await rabbitConnection.GetConnectionAsync();
                _channel = await conn.CreateChannelAsync();
                await _channel.ExchangeDeclareAsync(exchange: _settings.Exchange, type: ExchangeType.Direct, durable: true);
            }
            return _channel;
        }
        finally
        {
            _lock.Release();
        }
    }
}
