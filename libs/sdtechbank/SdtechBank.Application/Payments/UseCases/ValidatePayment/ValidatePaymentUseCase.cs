using Microsoft.Extensions.Logging;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Domain.PaymentOrders.Contracts;

namespace SdtechBank.Application.Payments.UseCases.ValidatePayment;

public interface IValidatePaymentUseCase
{
    Task ExecuteAsync(Guid paymentId, CancellationToken cancellation);
}

public sealed class ValidatePaymentUseCase(IPaymentOrderRepository paymentOrderRepository,
                                           IOutboxService outboxService,
                                           ILogger<ValidatePaymentUseCase> logger) : IValidatePaymentUseCase
{
    public async Task ExecuteAsync(Guid paymentId, CancellationToken cancellation)
    {
        var payment = await paymentOrderRepository.GetByIdAsync(paymentId);
        Guid tempReceiverId = Guid.Parse("a0ea2495-7027-4695-9aa5-3dbe4f9cb868");
        if (payment is null)
        {
            logger.LogWarning("Pagamento {paymentId} não encontrado para a validação", paymentId);
            return;
        }
        if (payment.IsPaymentDestinationReadyToTransfer)
        {
            payment.MarkAsWaitingConfirmation();
            //TODO: Nesta etapa também estamos confirmando automaticamente, no EPICO 5, deve ser feito pelo usuário
            payment.MarkAsReadyToTransfer();
            await paymentOrderRepository.SaveAsync(payment);
            await outboxService.AddEventAsync(payment.ToPaymentValidatedIntegrationEvent(receiverId: tempReceiverId, correlationId: payment.IdempotencyKey.ToString()), cancellation);
        }

        else
        {
            payment.MarkAsWaitingDict();
            await paymentOrderRepository.SaveAsync(payment);
            await outboxService.AddEventAsync(payment.ToPaymentNeedsAccountDataIntegrationEvent(), cancellation);

        }

    }
}
