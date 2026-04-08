using Microsoft.Extensions.Logging;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Domain.PaymentOrders.Contracts;

namespace SdtechBank.Application.Payments.UseCases.ValidatePayment;

public interface IValidatePaymentUseCase
{
    Task ExecuteAsync(Guid paymentId, CancellationToken cancellation);
}

public sealed class ValidatePaymentUseCase(IPaymentOrderRepository paymentOrderRepository,
                                           IOutboxService outboxService,
                                           IReceiverResolver receiverResolver,
                                           ILogger<ValidatePaymentUseCase> logger) : IValidatePaymentUseCase
{
    public async Task ExecuteAsync(Guid paymentId, CancellationToken cancellation)
    {
        var payment = await paymentOrderRepository.GetByIdAsync(paymentId);
        
        if (payment is null)
        {
            logger.LogWarning("Pagamento {PaymentId} não encontrado para a validação", paymentId);
            return;
        }
        if (payment.Destination.HasBankAccount())
        {
            var receiverId = await receiverResolver.ResolveAsync(payment, cancellation);
            if (receiverId is null)
            {
                payment.MarkAsFailed("Receiver not found");
                await paymentOrderRepository.SaveAsync(payment);
                return;
            }

            payment.MarkAsWaitingConfirmation();
            payment.MarkAsReadyToTransfer();
            await paymentOrderRepository.SaveAsync(payment);
            await outboxService.AddEventAsync(payment.ToPaymentValidatedIntegrationEvent(receiverId: receiverId!.Value, correlationId: payment.IdempotencyKey.ToString()), cancellation);
        }

        else
        {
            payment.MarkAsWaitingDict();
            await paymentOrderRepository.SaveAsync(payment);
            await outboxService.AddEventAsync(payment.ToPaymentNeedsAccountDataIntegrationEvent(), cancellation);

        }

    }
}
