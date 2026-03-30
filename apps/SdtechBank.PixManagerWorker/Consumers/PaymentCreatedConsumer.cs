using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;

namespace SdtechBank.PixManagerWorker.Consumers;

public sealed class PaymentCreatedConsumer(IProcessPaymentCreatedUseCase useCase)
{
    public async Task HandleAsync(PaymentCreatedEvent @event)
    {
       // await useCase.ExcecuteAsync(@event.PaymentId, @event.CorrelationId);
    }
}
