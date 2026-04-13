
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Messaging;

namespace SdtechBank.Infrastructure.Messaging;

public class OutboxPublisher(IOptions<OutboxPublisherSettings> settings, IServiceProvider provider, ILogger<OutboxPublisher> logger) : BackgroundService
{
    private readonly OutboxPublisherSettings _settings = settings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = provider.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var messages = await repository.GetUnprocessedMessagesAsync(_settings.MessagesLimit, stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishRawAsync(message.Id.ToString(), message.RoutingKey, message.Payload);
                    message.MarkAsProcessed();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao publicar outboxMessage {Id} (Attempt: {Attempt})", message.Id, message.Attempt);
                    message.IncrementAttempt();

                    if (message.Attempt >= _settings.MaxAttempts)
                    {
                        logger.LogError("Mensagem {Id} excedeu tentativas. Considerar DLQ", message.Id);
                        message.MarkAsFailed();
                    }
                }
                finally
                {
                    await repository.SaveAsync(message, stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken);
        }
    }
}
