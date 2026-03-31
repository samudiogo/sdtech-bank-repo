using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.Shared.Messaging;
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

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

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

        await _channel.BasicConsumeAsync(queue: _settings.DefaultQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        logger.LogInformation("Consumer iniciado");

        await Task.CompletedTask;
    }

    private async Task ProcessMessage(RabbitMqMessageEnvelope envelope, IBasicProperties props, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();

        var inbox = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
        var resolver = scope.ServiceProvider.GetRequiredService<IEventTypeResolver>();

        var messageId = props.MessageId;

        if (string.IsNullOrWhiteSpace(messageId))
            throw new InvalidOperationException("MessageId é obrigatório");


        var exists = await inbox.ExistsAsync(messageId, ct);

        if (exists && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Mensagem já processada: {messageId}", messageId);
            return;
        }

        var inboxMessage = new InboxMessage(messageId, envelope.Type);
        await inbox.AddAsync(inboxMessage, ct);


        var eventType = resolver.Resolve(envelope.Type);

        var @event = JsonSerializer.Deserialize(envelope.Payload, eventType) ?? throw new InvalidOperationException("falha ao desserializar evento");

        var dispatchMethod = typeof(IEventDispatcher)
            .GetMethod(nameof(IEventDispatcher.DispatchAsync))!
            .MakeGenericMethod(eventType);

        await (Task)dispatchMethod.Invoke(dispatcher, [@event, ct])!;

    }
}
