using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Transactions.UseCases.ProcessPayment;

namespace SdtechBank.Application.Payments.Consumers;

public sealed class PaymentCreatedConsumer(IProcessPaymentCreatedUseCase useCase)
{
    public async Task HandleAsync(PaymentCreatedIntegrationEvent @event)
    {
       // await useCase.ExcecuteAsync(@event.PaymentId, @event.CorrelationId);
    }
}
