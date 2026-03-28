namespace SdtechBank.Application.Common.Contracts;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : IDomainIntegrationEvent;
}