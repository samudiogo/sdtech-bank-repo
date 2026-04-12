namespace SdtechBank.Infrastructure.Messaging;

public class OutboxPublisherSettings
{
    public int MessagesLimit { get; set; } = default!;
    public int IntervalSeconds { get; set; } = default!;
    public int MaxAttempts { get; set; } = default!;
}
