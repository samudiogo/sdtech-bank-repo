namespace SdtechBank.Application.Payments.UseCases.CompletePayment;

public interface ICompletePaymentUseCase
{
    Task ExecuteAsync(Guid paymentId, Guid transactionId);
}
