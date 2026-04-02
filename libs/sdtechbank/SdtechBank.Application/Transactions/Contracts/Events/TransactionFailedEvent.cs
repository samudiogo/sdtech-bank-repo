using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Transactions.Contracts.Events;

public sealed record TransactionFailedEvent : IntegrationEvent
{
    public override string RoutingKey => "transaction.completed";

    public required Guid TransactionId { get; init; }
    public required Guid PaymentId { get; init; }
    public required string Reason { get; init; }
}