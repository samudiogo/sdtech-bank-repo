using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;

namespace SdtechBank.Application.Transactions.UseCases;

public class ProcessPaymentCreatedUseCase (ITransactionRepository transactionRepository)
{
    public async Task ExcecuteAsync(Guid paymentId, string idempotencyKey)
    {
        var existing = await transactionRepository.GetByIdempontencyKeyAsync(idempotencyKey);

        if (existing is not null) return;

        var transaction = Transaction.Create(paymentId, idempotencyKey);
        
        transaction.StartProcessing();

        await transactionRepository.SaveAsync(transaction);
    }
}
