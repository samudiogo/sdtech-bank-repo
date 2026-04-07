using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Domain.PaymentOrders.Events;

namespace SdtechBank.Application.Payments.EventHandlers;

public sealed class PaymentOrderValidatedHandler : IIntegrationEventHandler<PaymentOrderValidatedDomainEvent>
{
    public async Task HandleAsync(PaymentOrderValidatedDomainEvent @event, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
