using System.Collections.Concurrent;
using SdtechBank.Application.Messaging;

namespace SdtechBank.Infrastructure.Messaging;

public interface IEventTypeResolver
{
    IReadOnlyCollection<Type> GetAllRegisteredTypes();
    Type Resolve(string typeName);
}

public sealed class EventTypeResolver : IEventTypeResolver
{
    private static readonly ConcurrentDictionary<string, Type> Cache = [];
    private static readonly IReadOnlyCollection<Type> RegisteredTypes;
    static EventTypeResolver()
    {
        var eventTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            .Select(i => i.GetGenericArguments()[0])  // extrai o T de IEventHandler<T>
            .Distinct()
            .ToList()
            .AsReadOnly();

        RegisteredTypes = eventTypes;

        // Pré-popula o cache de resolução por nome
        foreach (var type in eventTypes)
            Cache.TryAdd(type.Name, type);
    }
    public Type Resolve(string typeName)
    {
        return Cache.GetOrAdd(typeName, ResolveType);
    }

    private static Type ResolveType(string typeName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
            if (type is not null)
                return type;
        }
        throw new InvalidOperationException($"Tipo de evento não encontrado: {typeName}");
    }

    public IReadOnlyCollection<Type> GetAllRegisteredTypes() => RegisteredTypes;
}
