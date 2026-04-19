using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SdtechBank.Infrastructure.Messaging;
using System.Text;

namespace SdtechBank.Infrastructure.IntegrationTests.Messaging;

[Collection("Infra")]
public class RabbitMqPublisherTests(RabbitMqFixture fixture)
{
    [Fact]
    public async Task PublishRawAsync_ShouldPublishMessageToExchange()
    {
        //arrange
        var settings = Options.Create(new RabbitMqSettings
        {
            Host = fixture.Host,
            Port = fixture.Port,
            Username = fixture.Username,
            Password = fixture.Password,
            Exchange = "sdtech.exchange",
            DefaultQueue = "sdtech.queue",
            DlqExchange = "sdtech.dlq.exchange",
            DlqQueue = "sdtech.dlq"
        });
        var connection = new RabbitMqConnection(settings);
        var publisher = new RabbitMqEventPublisher(settings, connection, NullLogger<RabbitMqEventPublisher>.Instance);
        var ct = TestContext.Current.CancellationToken;

        var conn = await connection.GetConnectionAsync();
        var channel = await conn.CreateChannelAsync(cancellationToken: ct);

        await channel.QueueDeclareAsync(queue: "sdtech.queue", durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.ExchangeDeclareAsync(exchange: "sdtech.exchange", type: "direct", durable: true, cancellationToken: ct);
        await channel.QueueBindAsync(queue: "sdtech.queue", exchange: "sdtech.exchange", routingKey: "payment.created", cancellationToken: ct);

        //act
        await publisher.PublishRawAsync(Guid.NewGuid().ToString(), "payment.created", """{"amount":100}""");
        await Task.Delay(200, ct);
        var result = await channel.BasicGetAsync("sdtech.queue", true, ct);

        //assert
        result.Should().NotBeNull();

        var body = Encoding.UTF8.GetString(result!.Body.ToArray());

        body.Should().Contain("payment.created");
        body.Should().Contain("amount");
    }
}