
namespace SdtechBank.Infrastructure.Messaging;

public sealed record RabbitMqMessageEnvelope
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string Type { get; init; } = default!;
    public string Payload { get; init; } = default!;

    public int Attempt { get; init; } = 1;

    public RabbitMqMessageEnvelope IncrementAttempt()
        => this with { Attempt = Attempt + 1 };
}
