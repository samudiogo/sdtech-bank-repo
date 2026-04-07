using SdtechBank.Application.Common.Contracts;
using SdtechBank.Domain.Shared.Messaging;
using System.Text.Json;

namespace SdtechBank.Application.Messaging;

public sealed class OutboxService(IOutboxRepository repository) : IOutboxService
{
    public async Task AddEventAsync<T>(T @event, CancellationToken ct)
    {
        var routingKey = ((IIntegrationEvent)@event!).RoutingKey;

        var payload = JsonSerializer.Serialize(@event);

        var message = new OutboxMessage(type: typeof(T).Name, routingKey: routingKey, payload: payload);

        await repository.AddAsync(message, ct);
    }
}