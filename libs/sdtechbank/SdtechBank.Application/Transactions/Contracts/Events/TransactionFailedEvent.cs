using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Transactions.Contracts.Events;

public sealed record TransactionFailedIntegrationEvent : IntegrationEvent
{
    public override string RoutingKey => "transaction.failed";

    public required Guid TransactionId { get; init; }
    public required Guid PaymentId { get; init; }
    public required string Reason { get; init; }
}