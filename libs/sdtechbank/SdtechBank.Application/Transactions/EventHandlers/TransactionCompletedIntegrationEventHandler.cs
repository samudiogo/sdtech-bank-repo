using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Application.Transactions.Contracts.Events;

namespace SdtechBank.Application.Transactions.EventHandlers;

public sealed class TransactionCompletedIntegrationEventHandler(ICompletePaymentUseCase useCase) : IIntegrationEventHandler<TransactionCompletedIntegrationEvent>
{
    public async Task HandleAsync(TransactionCompletedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(@event.PaymentId, @event.TransactionId, cancellationToken);
    }
}