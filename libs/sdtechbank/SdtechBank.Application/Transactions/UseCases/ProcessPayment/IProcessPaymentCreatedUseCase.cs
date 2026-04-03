using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Transactions.UseCases.ProcessPayment;

public interface IProcessPaymentCreatedUseCase
{
    Task ExecuteAsync(Guid paymentId, Guid payerId, Guid receiverId, Money amount, string idempotencyKey, CancellationToken cancellationToken);
}
