namespace SdtechBank.Application.Messaging;

public interface IOutboxService
{
    Task AddEventAsync<T>(T @event, CancellationToken ct);
}
