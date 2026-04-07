using Microsoft.Extensions.DependencyInjection;
namespace SdtechBank.Application.IntegrationEvents;

public interface IIntegrationEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}

public interface IIntegrationEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
}
public sealed class IntegrationEventDispatcher(IServiceProvider serviceProvider) : IIntegrationEventDispatcher
{
    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();       

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}


