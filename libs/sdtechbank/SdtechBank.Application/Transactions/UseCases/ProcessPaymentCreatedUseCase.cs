using Microsoft.Extensions.Logging;
using SdtechBank.Application.Transactions.Exceptions;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;

namespace SdtechBank.Application.Transactions.UseCases;

public class ProcessPaymentCreatedUseCase(
                                          ITransactionRepository transactionRepository,
                                          ILedgerRepository ledgerRepository,
                                          IAccountBalanceService balanceService,
                                          IAccountLockService lockService,
                                          ILogger<ProcessPaymentCreatedUseCase> logger)
{
    public async Task ExcecuteAsync(Guid paymentId, Guid payerId, Guid receiverId, Money amount, string idempotencyKey)
    {
        using (await lockService.AcquireLockAsync(payerId))
        {
            var existing = await transactionRepository.GetByIdempontencyKeyAsync(idempotencyKey);

            if (existing is not null) return;

            var transaction = Transaction.Create(paymentId, idempotencyKey);

            try
            {
                transaction.StartProcessing();
                await transactionRepository.SaveAsync(transaction);

                await ValidateBalance(payerId, amount);

                var debit = LedgerEntry.CreateDebit(transaction.Id, payerId, amount);

                var credit = LedgerEntry.CreateCreditt(transaction.Id, receiverId, amount);

                await ledgerRepository.AddRangeAsync([debit, credit]);

                transaction.MarkAsCompleted();

                await transactionRepository.SaveAsync(transaction);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar transação para PaymentId: {paymentId}", paymentId);
                try
                {
                    transaction.MarkAsFailed();
                    await transactionRepository.SaveAsync(transaction);
                }
                catch (Exception innerEx)
                {
                    logger.LogError(innerEx, "Erro ao marcar a transação como FAILED para PaymentId: {paymentId}", paymentId);
                }
                throw;
            }
        }
    }

    private async Task ValidateBalance(Guid payerId, Money amount)
    {
        var balance = await balanceService.GetBalanceAsync(payerId);
        if (balance < amount)
            throw new InsufficientFundsException(payerId, amount, balance);

    }
}
