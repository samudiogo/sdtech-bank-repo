namespace SdtechBank.Application.Payments.UseCases.FailPayment;

public interface IFailPaymentUseCase
{
    Task ExecuteAsync(Guid paymentId, string reason);
}