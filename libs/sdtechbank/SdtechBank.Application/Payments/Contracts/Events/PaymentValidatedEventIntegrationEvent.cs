using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentValidatedEventIntegrationEvent(Guid PaymentId, PaymentDestinationSnapshot Destination) : IntegrationEvent
{
    public override string RoutingKey => "payment.validated";
}
