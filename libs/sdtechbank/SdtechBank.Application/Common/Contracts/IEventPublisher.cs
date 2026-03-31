namespace SdtechBank.Application.Common.Contracts;

public interface IEventPublisher
{
    Task PublishRawAsync(string messageId, string type, string payload);
}