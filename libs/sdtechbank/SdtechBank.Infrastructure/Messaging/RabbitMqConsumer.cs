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

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _channel = await GetChannelAsync(cancellationToken: ct);
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
                await ProcessMessage(envelope, args.BasicProperties, ct);
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
                    await PublishWithDelayAsync(retryEnvelope, args.RoutingKey, delay, ct);
                    await _channel.BasicAckAsync(args.DeliveryTag, false);
                    return;
                }
                // DLQ
                logger.LogError("Mensagem enviada para DLQ após {Attempts} tentativas", attempt);
                await dlqPublisher.PublishAsync(json, ex.Message, args.BasicProperties);
                await _channel.BasicAckAsync(args.DeliveryTag, false);
            }
        };

        var arguments = new Dictionary<string, object?> { { "x-queue-type", _settings.QueueType }, { "x-message-ttl", _settings.MessageTtlMs } };

        await _channel.QueueDeclareAsync(queue: _settings.DefaultQueue, durable: true, exclusive: false, autoDelete: false, arguments: arguments, cancellationToken: ct);

        foreach (var (eventName, __) in registry.GetAll())
        {
            await _channel.QueueBindAsync(queue: _settings.DefaultQueue, exchange: _settings.Exchange, routingKey: eventName, cancellationToken: ct);
        }

        await _channel.BasicConsumeAsync(queue: _settings.DefaultQueue, autoAck: false, consumer: consumer, cancellationToken: ct);
        logger.LogInformation("Consumer iniciado");
        await Task.Delay(Timeout.Infinite, ct);
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
                logger.LogInformation("Processando mensagem {MessageId} Attempt {Attempt}", envelope.MessageId, envelope.Attempt);
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

    private async Task PublishWithDelayAsync(RabbitMqMessageEnvelope envelope, string routingKey, TimeSpan delay, CancellationToken ct)
    {
        var retryQueue = $"{_settings.DefaultQueue}.retry.{delay.TotalSeconds}s";
        Dictionary<string, object?> arguments = new()
        {
            { "x-message-ttl", (int)delay.TotalMilliseconds },
            { "x-dead-letter-exchange", _settings.Exchange },
            { "x-dead-letter-routing-key", routingKey }
        };
        _channel = await GetChannelAsync(cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue: retryQueue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        var props = new BasicProperties
        {
            Persistent = true,
            MessageId = envelope.MessageId
        };

        await _channel.BasicPublishAsync(exchange: "", routingKey: retryQueue, mandatory: false, basicProperties: props, body: body);
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            return _channel;

        if (_channel is null)
        {
            var conn = await rabbitConnection.GetConnectionAsync();
            _channel = await conn.CreateChannelAsync(cancellationToken: cancellationToken);
            await _channel.ExchangeDeclareAsync(exchange: _settings.Exchange, type: ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.ExchangeDeclareAsync(exchange: _settings.DlqExchange, type: ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(queue: _settings.DlqQueue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: _settings.DlqQueue, exchange: _settings.DlqExchange, routingKey: "", cancellationToken: cancellationToken);

            logger.LogInformation("Channel criado com sucesso");
        }
        return _channel;
    }
}
