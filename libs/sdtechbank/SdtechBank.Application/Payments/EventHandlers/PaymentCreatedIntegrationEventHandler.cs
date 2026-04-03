using Microsoft.Extensions.Logging;
using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.UseCases.ValidatePayment;

namespace SdtechBank.Application.Transactions.EventHandlers;

public sealed class PaymentCreatedIntegrationEventHandler(ILogger<PaymentCreatedIntegrationEventHandler> logger, IValidatePaymentUseCase useCase) : IIntegrationEventHandler<PaymentCreatedIntegrationEvent>
{
    public async Task HandleAsync(PaymentCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processando PaymentCreatedIntegrationEvent: {PaymentId}", @event.PaymentId);

        await useCase.ExecuteAsync(@event.PaymentId, cancellationToken);

        await Task.CompletedTask;
    }
}