
namespace SdtechBank.Application.Ports;

/// <summary>
/// Defines a contract for consuming messages asynchronously from a message source.
/// </summary>
/// <remarks>Implementations of this interface are responsible for starting the message consumption process and
/// handling messages as they arrive. The consumption process can be canceled by passing a cancellation token.</remarks>
public interface IMessageConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}
