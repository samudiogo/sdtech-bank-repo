
namespace SdtechBank.Domain.Shared.Messaging;

public sealed class InboxMessage
{
    public Guid Id { get; private set; }
    public string MessageId { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public DateTime ProcessedAt { get; private set; }

    private InboxMessage() { }
    public InboxMessage(string messageId, string type)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        Type = type;
        ProcessedAt = DateTime.UtcNow;
    }
}
