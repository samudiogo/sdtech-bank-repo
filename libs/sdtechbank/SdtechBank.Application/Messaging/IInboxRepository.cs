
using SdtechBank.Domain.OutboxInbox;

namespace SdtechBank.Application.Messaging;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken);
    Task<InboxMessage> GetInboxMessageByMessageIdAsync(string messageId, CancellationToken cancellationToken);
    Task AddAsync(InboxMessage message, CancellationToken cancellationToken);
    Task MarkAsProcessedAsync(string messageId, CancellationToken cancellationToken);
    Task MarkAsFailedAsync(string messageId, CancellationToken cancellationToken);
    Task SaveAsync(InboxMessage message, CancellationToken cancellationToken);
}
