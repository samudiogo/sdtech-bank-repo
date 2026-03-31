
using SdtechBank.Domain.Shared.Messaging;

namespace SdtechBank.Application.Messaging;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken);
    Task AddAsync(InboxMessage message, CancellationToken cancellationToken);
}
