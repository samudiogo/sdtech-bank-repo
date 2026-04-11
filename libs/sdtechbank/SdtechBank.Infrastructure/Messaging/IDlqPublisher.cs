using RabbitMQ.Client;

namespace SdtechBank.Infrastructure.Messaging;

public interface IDlqPublisher
{
    Task PublishAsync(string message, string reason, IReadOnlyBasicProperties? originalProps);
}
