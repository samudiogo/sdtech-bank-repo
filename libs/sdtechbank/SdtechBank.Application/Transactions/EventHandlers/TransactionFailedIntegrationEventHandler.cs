using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Payments.UseCases.FailPayment;
using SdtechBank.Application.Transactions.Contracts.Events;

namespace SdtechBank.Application.Transactions.EventHandlers;

public sealed class TransactionFailedIntegrationEventHandler(IFailPaymentUseCase useCase) : IIntegrationEventHandler<TransactionFailedIntegrationEvent>
{
    public async Task HandleAsync(TransactionFailedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(@event.PaymentId, @event.Reason, cancellationToken);
    }
}
