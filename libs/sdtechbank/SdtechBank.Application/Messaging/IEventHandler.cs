using Microsoft.Extensions.DependencyInjection;
namespace SdtechBank.Application.Messaging;

public interface IEventHandler<in TEvent>
{
    Task HandlerAsync(TEvent @event, CancellationToken cancellationToken);
}

public interface IEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
}
public sealed class EventDispatcher(IServiceProvider serviceProvider) : IEventDispatcher
{
    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.HandlerAsync(@event, cancellationToken);
        }
    }
}


