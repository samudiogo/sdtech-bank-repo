using System.Collections.Concurrent;

namespace SdtechBank.Infrastructure.Messaging;

public interface IEventTypeResolver
{
    Type Resolve(string typeName);
}

public sealed class EventTypeResolver : IEventTypeResolver
{
    private static readonly ConcurrentDictionary<string, Type> Cache = [];
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
}
