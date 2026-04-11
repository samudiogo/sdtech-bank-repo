using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Messaging;
using System.Text;
using System.Text.Json;
using SdtechBank.Application.Abstractions.Resilience;
using SdtechBank.Domain.OutboxInbox;

namespace SdtechBank.Infrastructure.Messaging;

public class RabbitMqConsumer(
    IOptions<RabbitMqSettings> settings,
    IServiceProvider serviceProvider,
    IRabbitMqConnection rabbitConnection,
    IIntegrationEventTypeRegistry registry,
    IErrorClassifier errorClassifier,
    IRetryPolicy retryPolicy,
    IDlqPublisher dlqPublisher,
    ILogger<RabbitMqConsumer> logger)
    : BackgroundService
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var connection = await rabbitConnection.GetConnectionAsync();
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        if (_channel is null)
            throw new InvalidOperationException("Channel não foi criado");

        logger.LogInformation("Channel criado com sucesso");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            var body = args.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            RabbitMqMessageEnvelope envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<RabbitMqMessageEnvelope>(json)!;
            }
            catch (Exception ex)
            {

                logger.LogError(ex, "Erro ao desserializar mensagem - Indo para DLQ");
                await dlqPublisher.PublishAsync(json, "DeserializationError", args.BasicProperties);
                await _channel.BasicAckAsync(args.DeliveryTag, false);
                return;
            }
            try
            {
                await ProcessMessage(envelope, args.BasicProperties, stoppingToken);

                await _channel.BasicAckAsync(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                var errorCategory = errorClassifier.Classify(ex);
                var attempt = envelope.Attempt;
                logger.LogWarning(ex, "Erro ao processar mensagem. Attempt: {Attempt}, Tipo: {ErrorCategory}", attempt, errorCategory);

                if (retryPolicy.ShouldRetry(errorCategory, attempt))
                {
                    var nextAttempt = attempt + 1;
                    var delay = retryPolicy.GetDelay(nextAttempt);
                    logger.LogWarning("Retry agendado. Attempt: {NextAttempt}, Delay: {Delay}", nextAttempt, delay);

                    var retryEnvelope = envelope with { Attempt = nextAttempt };

                    await PublishWithDelayAsync(retryEnvelope, args.RoutingKey, delay);

                    await _channel.BasicAckAsync(args.DeliveryTag, false);
                    return;
                }
                // DLQ
                logger.LogError("Mensagem enviada para DLQ após {Attempts} tentativas", attempt);

                await dlqPublisher.PublishAsync(json, ex.Message, args.BasicProperties);

                await _channel.BasicAckAsync(args.DeliveryTag, false);
            }
        };

        await _channel.ExchangeDeclareAsync(
            exchange: _settings.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var arguments = new Dictionary<string, object?> { { "x-queue-type", _settings.QueueType }, { "x-message-ttl", _settings.MessageTtlMs } };

        await _channel.QueueDeclareAsync(
            queue: _settings.DefaultQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: stoppingToken);


        foreach (var (eventName, __) in registry.GetAll())
        {

            await _channel.QueueBindAsync(
                queue: _settings.DefaultQueue,
                exchange: _settings.Exchange,
                routingKey: eventName,
                cancellationToken: stoppingToken);


        }

        await _channel.BasicConsumeAsync(queue: _settings.DefaultQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        logger.LogInformation("Consumer iniciado");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessage(RabbitMqMessageEnvelope envelope, IReadOnlyBasicProperties props, CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var inbox = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IIntegrationEventDispatcher>();
        var eventType = registry.Resolve(envelope.Type);

        var messageId = props.MessageId;

        if (string.IsNullOrWhiteSpace(messageId))
            throw new InvalidOperationException("MessageId é obrigatório");


        var inboxMessage = await inbox.GetInboxMessageByMessageIdAsync(messageId, ct);

        if (inboxMessage is not null && logger.IsEnabled(LogLevel.Information))
        {
            if (inboxMessage.Status == InboxStatus.Processed)
            {
                logger.LogInformation("Mensagem já processada: {MessageId}", messageId);
                return;
            }

            if (inboxMessage.Status == InboxStatus.Processing)
            {
                logger.LogInformation("Mensagem já está em processamento: {MessageId}", messageId);
                return;
            }

            if (inboxMessage.Status == InboxStatus.Failed)
            {
                logger.LogInformation("Reprocessando mensagem falhada: {MessageId}", messageId);
            }
        }
        else
        {
            inboxMessage = new InboxMessage(messageId, envelope.Type);
            await inbox.AddAsync(inboxMessage, ct);
        }

        try
        {            
            var @event = JsonSerializer.Deserialize(envelope.Payload, eventType) ?? throw new InvalidOperationException("falha ao desserializar evento");
            var dispatchMethod = typeof(IIntegrationEventDispatcher)
           .GetMethod(nameof(IIntegrationEventDispatcher.DispatchAsync))!
           .MakeGenericMethod(eventType);

            await (Task)dispatchMethod.Invoke(dispatcher, [@event, ct])!;

            await inbox.MarkAsProcessedAsync(messageId, ct);
        }
        catch
        {
            await inbox.MarkAsFailedAsync(messageId, ct);
            throw;
        }
    }

    private async Task PublishWithDelayAsync(RabbitMqMessageEnvelope envelope, string routingKey, TimeSpan delay)
    {
        var retryQueue = $"{_settings.DefaultQueue}.retry.{delay.TotalSeconds}s";
        Dictionary<string, object?> arguments = new()
        {
            { "x-message-ttl", (int)delay.TotalMilliseconds },
            { "x-dead-letter-exchange", _settings.Exchange },
            { "x-dead-letter-routing-key", routingKey }
        };

        //TODO: criar uma anternativa null safely para _channel. Atualemente o seu valor está sendo instanciado apenas no método ExecuteAsync(). Estou pensando em mover para o método construtor
        // ou criar um método privado que valide e retorne uma instancia de _channel
        await _channel!.QueueDeclareAsync(queue: retryQueue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        var props = new BasicProperties
        {
            Persistent = true,
            MessageId = envelope.MessageId
        };

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: retryQueue,
            mandatory: true,
            basicProperties: props,
            body: body);
    }
}
