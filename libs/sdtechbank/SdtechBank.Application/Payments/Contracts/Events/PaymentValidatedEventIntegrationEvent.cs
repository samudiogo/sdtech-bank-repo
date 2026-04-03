using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentValidatedEventIntegrationEvent : IntegrationEvent
{ 
    public required Guid PaymentId { get; init; }
    public required PaymentDestinationSnapshot Destination { get; init; }
    public override string RoutingKey => "payment.validated";
}
