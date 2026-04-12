
namespace SdtechBank.Domain.Shared.Messaging;

public enum OutboxStatus
{
    Processing = 1,
    Processed = 2,
    Failed = 3
}

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = default!;
    public string RoutingKey { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? LastAttemptedAt { get; private set; }
    public int Attempt { get; private set; } = 1;
    public OutboxStatus Status { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(string type, string routingKey, string payload)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        RoutingKey = routingKey;
        OccurredAt = DateTime.UtcNow;
        Status = OutboxStatus.Processing;
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Status = OutboxStatus.Processed;
    }

    public void MarkAsFailed()
    {
        FailedAt = DateTime.UtcNow;
        Status = OutboxStatus.Failed;
    }
    public void IncrementAttempt()
    {
        Attempt++;
        LastAttemptedAt = DateTime.UtcNow;
    }
}
