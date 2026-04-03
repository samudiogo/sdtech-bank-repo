using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentNeedsAccountDataIntegrationEvent : IntegrationEvent
{
     public required Guid PaymentId { get; init; }   
    public required string PixKey { get; init; }
    public override string RoutingKey => "payment.waiting_for_dict";
}
