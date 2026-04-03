
namespace SdtechBank.Domain.Shared.Messaging;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = default!;
    public string RoutingKey { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(string type, string routingKey, string payload)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        RoutingKey = routingKey;
        OccurredAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed() => ProcessedAt = DateTime.UtcNow;
}
