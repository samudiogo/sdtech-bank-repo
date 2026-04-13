using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.IntegrationEvents;

public interface IIntegrationEventTypeRegistry
{
    void Register<T>(string name) where T : IIntegrationEvent;

    Type Resolve(string name);

    IReadOnlyCollection<(string Name, Type Type)> GetAll();
}
