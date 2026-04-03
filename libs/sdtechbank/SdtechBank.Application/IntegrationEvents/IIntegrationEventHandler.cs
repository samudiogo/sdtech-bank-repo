using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace SdtechBank.Application.IntegrationEvents;

public interface IIntegrationEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}

public interface IIntegrationEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
}
public sealed class IntegrationEventDispatcher(IServiceProvider serviceProvider, ILogger<IntegrationEventDispatcher> logger) : IIntegrationEventDispatcher
{
    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
        logger.LogInformation("Dispatching event {EventType} para {HandlersCount} handlers", typeof(TEvent).Name, handlers.Count());

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}


