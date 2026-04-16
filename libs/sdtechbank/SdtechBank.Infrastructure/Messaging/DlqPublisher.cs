using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace SdtechBank.Infrastructure.Messaging;

public class DlqPublisher(IRabbitMqConnection connection, IOptions<RabbitMqSettings> settings) : IDlqPublisher
{
    private readonly RabbitMqSettings _settings = settings.Value;

    public async Task PublishAsync(string message, string reason, IReadOnlyBasicProperties? originalProps)
    {
        var conn = await connection.GetConnectionAsync();
        var channel = await conn.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _settings.DlqExchange,
            type: ExchangeType.Fanout,
            durable: true);

        var body = Encoding.UTF8.GetBytes(message);

        var props = new BasicProperties
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>
            {
                { "x-error-reason", reason },
                { "x-original-message-id", originalProps?.MessageId },
                { "x-original-routing-key", originalProps?.Headers?["routing-key"] },
                { "x-dead-lettered-at", DateTime.UtcNow.ToString("O") }
            }
        };

        await channel.BasicPublishAsync(
            exchange: _settings.DlqExchange,
            routingKey: "",
            mandatory: false,
            basicProperties: props,
            body: body);
    }
}