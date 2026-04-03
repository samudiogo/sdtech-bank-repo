using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Messaging;

public interface IIntegrationEventTypeRegistry
{
    void Register<T>(string name) where T : IIntegrationEvent;

    Type Resolve(string name);

    IReadOnlyCollection<(string Name, Type Type)> GetAll();
}

public sealed class IntegrationEventTypeRegistry : IIntegrationEventTypeRegistry
{
    private readonly Dictionary<string, Type> _map = [];

    public void Register<T>(string name) where T : IIntegrationEvent
    {
        _map[name] = typeof(T);
    }

    public Type Resolve(string name)
    {
        if (_map.TryGetValue(name, out var type))
            return type;

        throw new InvalidOperationException($"Evento não registrado: {name}");
    }

    public IReadOnlyCollection<(string Name, Type Type)> GetAll()
        => _map.Select(x => (x.Key, x.Value)).ToList();
}