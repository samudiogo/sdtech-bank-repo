using Microsoft.Extensions.DependencyInjection;
namespace SdtechBank.Application.IntegrationEvents;

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


