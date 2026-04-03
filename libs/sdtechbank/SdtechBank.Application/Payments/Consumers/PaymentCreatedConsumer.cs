using Microsoft.Extensions.Logging;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.UseCases.ValidatePayment;

namespace SdtechBank.Application.Payments.Consumers;

public sealed class PaymentCreatedConsumer(IValidatePaymentUseCase useCase, ILogger<PaymentCreatedConsumer> logger)
{
    public async Task HandleAsync(PaymentCreatedIntegrationEvent @event, CancellationToken cancellation)
    {
        logger.LogInformation("entrei aqui no PaymentCreatedConsumer {event}", @event);

        await useCase.ExecuteAsync(@event.PaymentId, cancellation);
    }
}
