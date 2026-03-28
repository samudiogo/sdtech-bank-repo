namespace SdtechBank.Application.Common.Contracts;

public interface IMessageConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}
