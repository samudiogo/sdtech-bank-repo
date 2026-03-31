using SdtechBank.Domain.Shared.Messaging;
using System.Text.Json;

namespace SdtechBank.Application.Messaging;

public interface IOutboxService
{
    Task AddEventAsync<T>(T @event, CancellationToken ct);
}

public sealed class OutboxService(IOutboxRepository repository) : IOutboxService
{
    public async Task AddEventAsync<T>(T @event, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(@event);

        var message = new OutboxMessage(type: typeof(T).Name, payload: payload);

        await repository.AddAsync(message, ct);
    }
}