using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Domain.Shared.Messaging;
using SdtechBank.Domain.Transactions.Events;
using System.Text;
using System.Text.Json;

namespace SdtechBank.Infrastructure.Messaging;

public class RabbitMqConsumer(IOptions<RabbitMqSettings> settings, IServiceProvider serviceProvider, ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var envelope = JsonSerializer.Deserialize<RabbitMqMessageEnvelope>(json)!;

                await ProcessMessage(envelope, (IBasicProperties)args.BasicProperties, stoppingToken);

                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar mensagem");

                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(queue: _settings.DefaultQueue, autoAck: false, consumer: consumer);

        logger.LogInformation("Consumer iniciado");

        await Task.CompletedTask;
    }

    private async Task ProcessMessage(RabbitMqMessageEnvelope envelope, IBasicProperties props, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();

        var inbox = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

        var messageId = props.MessageId;

        if (string.IsNullOrWhiteSpace(messageId))
            throw new InvalidOperationException("MessageId é obrigatório");


        var exists = await inbox.ExistsAsync(messageId, ct);

        if (exists)
        {
            logger.LogInformation("Mensagem já processada: {messageId}", messageId);
            return;
        }

        var inboxMessage = new InboxMessage(messageId, envelope.Type);
        await inbox.AddAsync(inboxMessage, ct);


        switch (envelope.Type)
        {
            case nameof(PaymentOrderCreatedEvent):
                var evt = JsonSerializer.Deserialize<PaymentCreatedEvent>(envelope.Payload)!;
                await dispatcher.DispatchAsync(evt, ct);
                break;

            default:
                throw new InvalidOperationException($"Tipo desconhecido: {envelope.Type}");
        }
    }
}
