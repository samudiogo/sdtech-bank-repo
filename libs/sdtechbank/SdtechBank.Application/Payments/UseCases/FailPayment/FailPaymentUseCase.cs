using Microsoft.Extensions.Logging;
using SdtechBank.Domain.PaymentOrders.Contracts;

namespace SdtechBank.Application.Payments.UseCases.FailPayment;

public sealed class FailPaymentUseCase(IPaymentOrderRepository paymentOrderRepository, ILogger<FailPaymentUseCase> logger) : IFailPaymentUseCase
{
    public async Task ExecuteAsync(Guid paymentId, string reason)
    {
        var payment = await paymentOrderRepository.GetByIdAsync(paymentId);

        if (payment is null)
        {
            logger.LogWarning("Pagamento {PaymentId} não encontrado", paymentId);
            return;
        }

        payment.MarkAsFailed(reason);

        await paymentOrderRepository.SaveAsync(payment);

    }
}