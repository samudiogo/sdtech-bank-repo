
namespace SdtechBank.Infrastructure.Messaging;

public sealed class RabbitMqMessageEnvelope
{
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
}
