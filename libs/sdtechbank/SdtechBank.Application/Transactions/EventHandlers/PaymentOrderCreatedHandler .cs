
using Microsoft.Extensions.Logging;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.Transactions.Events;

namespace SdtechBank.Application.Transactions.EventHandlers;

public sealed class PaymentOrderCreatedHandler(ILogger<PaymentOrderCreatedHandler> logger) : IEventHandler<PaymentOrderCreatedEvent>
{
    public async Task HandlerAsync(PaymentOrderCreatedEvent @event, CancellationToken cancellationToken)
    {
       logger.LogInformation("Processando paymentOrder: {paymentOrder}", @event.PaymentOrderId);

        await Task.CompletedTask;
    }
}
