namespace SdtechBank.Application.IntegrationEvents;

public interface IIntegrationEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
}


