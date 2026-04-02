using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentNeedsAccountDataIntegrationEvent(Guid PaymentId, string PixKey) : IntegrationEvent
{
    public override string RoutingKey => "payment.waiting_for_dict";
}
