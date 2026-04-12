
namespace SdtechBank.Domain.OutboxInbox;

public enum InboxStatus
{
    Processing = 1,
    Processed = 2,
    Failed = 3
}
public sealed class InboxMessage
{
    public Guid Id { get; private set; }
    public string MessageId { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public InboxStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }


    private InboxMessage() { }
    public InboxMessage(string messageId, string type)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        Type = type;
        Status = InboxStatus.Processing;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        Status = InboxStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = InboxStatus.Failed;
        FailedAt = DateTime.UtcNow;
    }
}
