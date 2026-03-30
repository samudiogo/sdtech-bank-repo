using Microsoft.Extensions.Logging;
using SdtechBank.Domain.PaymentOrders.Contracts;

namespace SdtechBank.Application.Payments.UseCases.CompletePayment;

public class CompletePaymentUseCase(IPaymentOrderRepository paymentOrderRepository, ILogger<CompletePaymentUseCase> logger) : ICompletePaymentUseCase
{
    public async Task ExecuteAsync(Guid paymentId, Guid transactionId)
    {
        var payment = await paymentOrderRepository.GetByIdAsync(paymentId);

        if(payment is null)
        {
            logger.LogWarning("Pagamento {paymentId} não encontrado para a transação {transactionId}", paymentId, transactionId);
            return;
        }

        payment.MarkAsCompleted(transactionId);

        await paymentOrderRepository.SaveAsync(payment);
    }
}
