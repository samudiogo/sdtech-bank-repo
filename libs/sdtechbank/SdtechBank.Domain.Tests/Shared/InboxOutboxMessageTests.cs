using SdtechBank.Domain.Shared.Messaging;

namespace SdtechBank.Domain.Tests.Shared;

public class InboxOutboxMessageTests
{
    [Fact]
    public void OutboxMessage_MarkAsProcessed_Should_ReturnSuccess()
    {
        var outboxmessage = new OutboxMessage("event", "routing.key", "payload");

        outboxmessage.MarkAsProcessed();

        Assert.NotEqual(default!, outboxmessage.Id);
        Assert.NotEqual(default!, outboxmessage.OccurredAt);
        Assert.NotEqual(default!, outboxmessage.ProcessedAt);
    }

    [Fact]
    public void InboxMessage_InstanceCreated_Should_ReturnSuccess()
    {
        var messageId = Guid.NewGuid().ToString();
        var messageType = "type";
        var outboxmessage = new InboxMessage(messageId, messageType);

        Assert.NotEqual(default!, outboxmessage.Id);
        Assert.Equal(messageId, outboxmessage.MessageId);
        Assert.Equal(messageType, outboxmessage.Type);
        Assert.NotEqual(default!, outboxmessage.ProcessedAt);
    }
}
