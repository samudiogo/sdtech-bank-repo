
using SdtechBank.Domain.Shared.Messaging;

namespace SdtechBank.Application.Messaging;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct);
    Task<IList<OutboxMessage>> GetUnprocessedMessagesAsync(int limit, CancellationToken ct);    
    Task SaveAsync(OutboxMessage message, CancellationToken ct);
}
