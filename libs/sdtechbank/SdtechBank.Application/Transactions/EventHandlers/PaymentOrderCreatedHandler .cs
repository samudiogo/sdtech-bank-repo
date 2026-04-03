
using Microsoft.Extensions.Logging;
using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;


namespace SdtechBank.Application.Transactions.EventHandlers;

public sealed class PaymentOrderCreatedHandler(ILogger<PaymentOrderCreatedHandler> logger, IProcessPaymentCreatedUseCase useCase) : IIntegrationEventHandler<PaymentValidatedIntegrationEvent>
{
    public async Task HandleAsync(PaymentValidatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processando PaymentValidatedIntegrationEvent: {paymentOrder}", @event.PaymentId);

        await useCase.ExecuteAsync(
         paymentId: @event.PaymentId,
         payerId: @event.PayerId,
         receiverId: @event.ReceiverId,
         idempotencyKey: @event.IdempotencyKey,
         amount: @event.Amount, cancellationToken: cancellationToken);

        await Task.CompletedTask;
    }
}
