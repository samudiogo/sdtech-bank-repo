using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentCreatedIntegrationEvent : IntegrationEvent
{
    public override string RoutingKey => "payment.created";
    public required Guid PaymentId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required Guid PayerId { get; init; }
    public required PaymentDestinationSnapshot Destination { get; init; }
}
