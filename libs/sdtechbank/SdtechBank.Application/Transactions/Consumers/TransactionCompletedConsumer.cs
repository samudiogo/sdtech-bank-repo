using SdtechBank.Application.Payments.UseCases.CompletePayment;
using SdtechBank.Application.Transactions.Contracts.Events;

namespace SdtechBank.Application.Transactions.Consumers;

public sealed class TransactionCompletedConsumer(ICompletePaymentUseCase useCase)
{
    public async Task HandleAsync(TransactionCompletedEvent @event)
    {
        await useCase.ExecuteAsync(@event.PaymentId, @event.TransactionId);
    }
}
