using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SdtechBank.Application.Common.Contracts;
using System.Text;
using System.Text.Json;

namespace SdtechBank.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher(IOptions<RabbitMqSettings> settings, IRabbitMqConnection connection, ILogger<RabbitMqEventPublisher> logger) : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task PublishRawAsync(string messageId, string type, string payload)
    {
        var conn = await connection.GetConnectionAsync();

        await using var channel = await conn.CreateChannelAsync();

        var envelope = new RabbitMqMessageEnvelope { Type = type, Payload = payload };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

        var props = new BasicProperties { MessageId = messageId, ContentType = "application/json", DeliveryMode = DeliveryModes.Persistent };

        logger.LogInformation("BasicPublishAsync params: {exchange},{routingKey}", _settings.Exchange, type);

        await channel.BasicPublishAsync(exchange: _settings.Exchange, routingKey: type, mandatory: true, basicProperties: props, body: body);
    }

    public async ValueTask DisposeAsync()
    {
        _lock.Dispose();
    }
}
