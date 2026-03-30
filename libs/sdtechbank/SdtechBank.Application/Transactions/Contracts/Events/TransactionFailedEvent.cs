using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Transactions.Contracts.Events;

public sealed record TransactionFailedEvent : IDomainIntegrationEvent
{
    public Guid EventId => Guid.NewGuid();
    public DateTimeOffset OccurredAt => DateTime.UtcNow;
    public string RoutingKey => "transaction.completed";
    public string CorrelationId { get; init; } = default!;
    public required Guid TransactionId { get; init; }
    public required Guid PaymentId { get; init; }
    public required string Reason { get; init; }
}