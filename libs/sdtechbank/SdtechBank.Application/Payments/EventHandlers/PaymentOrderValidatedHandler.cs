using Microsoft.Extensions.Logging;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.PaymentOrders.Events;

namespace SdtechBank.Application.Payments.EventHandlers;

public sealed class PaymentOrderValidatedHandler(ILogger<PaymentOrderValidatedHandler> logger) : IEventHandler<PaymentOrderValidatedDomainEvent>
{
    public async Task HandlerAsync(PaymentOrderValidatedDomainEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validando paymentOrder: {PaymentOrderId}", @event.PaymentOrderId);

        await Task.CompletedTask;
    }
}
