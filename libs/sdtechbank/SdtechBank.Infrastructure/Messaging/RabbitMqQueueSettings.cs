namespace SdtechBank.Infrastructure.Messaging;

public class RabbitMqQueueSettings
{
    public Dictionary<string, string> Queues { get; set; } = [];
   
    public string Resolve<T>()
    {
        var key = typeof(T).Name;
        return Queues.TryGetValue(key, out var name) ? name : ToKebabCase(key);
    }

    private static string ToKebabCase( string name)
    {
        return string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? $"-{char.ToLower(c)}" : char.ToLower(c).ToString()));
    }
}
