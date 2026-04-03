
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Messaging;

namespace SdtechBank.Infrastructure.Messaging;

public class OutboxPublisher(IServiceProvider provider, ILogger<OutboxPublisher> logger) : BackgroundService
{
    private readonly int MessagesLimit = 50;
    private readonly int IntervalBetweenMessages = 5;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = provider.CreateAsyncScope();

            var repo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            var messages = await repo.GetUnprocessedMessagesAsync(MessagesLimit, stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    logger.LogInformation("Publicando mensagem: Id={Id}, RoutingKey={RoutingKey}", message.Id, message.RoutingKey);
                    
                    await publisher.PublishRawAsync(message.Id.ToString(), message.RoutingKey, message.Payload);

                    await repo.MarkAsProcessedAsync(message.Id, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao publicar outboxMessage {Id}", message.Id);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(IntervalBetweenMessages), stoppingToken);
        }
    }
}
