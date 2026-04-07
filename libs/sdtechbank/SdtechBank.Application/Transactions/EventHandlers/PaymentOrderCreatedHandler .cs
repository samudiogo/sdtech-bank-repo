using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;


namespace SdtechBank.Application.Transactions.EventHandlers;

public sealed class PaymentOrderCreatedHandler(IProcessPaymentCreatedUseCase useCase) : IIntegrationEventHandler<PaymentValidatedIntegrationEvent>
{
    public async Task HandleAsync(PaymentValidatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(
         paymentId: @event.PaymentId,
         payerId: @event.PayerId,
         receiverId: @event.ReceiverId,
         idempotencyKey: @event.IdempotencyKey,
         amount: @event.Amount, cancellationToken: cancellationToken);

        await Task.CompletedTask;
    }
}
