using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases;

namespace SdtechBank.PixManagerWorker.Consumers;

public sealed class PaymentCreatedConsumer(ProcessPaymentCreatedUseCase useCase)
{
    public async Task HandleAsync(PaymentCreatedEvent @event)
    {
        await useCase.ExcecuteAsync(@event.PaymentId, @event.CorrelationId);
    }
}
