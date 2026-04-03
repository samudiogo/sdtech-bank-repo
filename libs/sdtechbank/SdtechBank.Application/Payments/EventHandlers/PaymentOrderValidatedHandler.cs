using Microsoft.Extensions.Logging;
using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Domain.PaymentOrders.Events;

namespace SdtechBank.Application.Payments.EventHandlers;

public sealed class PaymentOrderValidatedHandler(ILogger<PaymentOrderValidatedHandler> logger) : IIntegrationEventHandler<PaymentOrderValidatedDomainEvent>
{
    public async Task HandleAsync(PaymentOrderValidatedDomainEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validando paymentOrder: {PaymentOrderId}", @event.PaymentOrderId);

        await Task.CompletedTask;
    }
}
