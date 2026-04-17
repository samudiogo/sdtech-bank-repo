using Microsoft.Extensions.Logging;
using SdtechBank.Application.Abstractions.Persistence;
using SdtechBank.Application.Accounts.Contracts;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Transactions.Contracts.Events;
using SdtechBank.Application.Transactions.Exceptions;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Domain.Transactions.Contracts;
using SdtechBank.Domain.Transactions.Entities;

namespace SdtechBank.Application.Transactions.UseCases.ProcessPayment;

public class ProcessPaymentCreatedUseCase(
                                          IUnitOfWork unitOfWork,
                                          ITransactionRepository transactionRepository,
                                          IPaymentOrderRepository paymentOrderRepository,
                                          ILedgerRepository ledgerRepository,
                                          IAccountBalanceService balanceService,
                                          IAccountLockService lockService,
                                          IOutboxService outboxService,
                                          ILogger<ProcessPaymentCreatedUseCase> logger) : IProcessPaymentCreatedUseCase
{
    public async Task ExecuteAsync(Guid paymentId, Guid payerId, Guid receiverId, Money amount, string idempotencyKey, CancellationToken cancellationToken)
    {
        await using var accountLock = await lockService.AcquireLockAsync(payerId, cancellationToken);

        var alreadyProcessed = await transactionRepository.GetByIdempotencyKeyAsync(idempotencyKey);

        if (alreadyProcessed is not null) return;

        try
        {
            await ProcessSuccessFlow(paymentId, payerId, receiverId, amount, idempotencyKey, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar PaymentId {PaymentId}", paymentId);

            await HandleFailureFlow(paymentId, idempotencyKey, ex, cancellationToken);
            throw;
        }
    }

    private async Task ProcessSuccessFlow(Guid paymentId, Guid payerId, Guid receiverId, Money amount, string idempotencyKey, CancellationToken ct)
    {
        await unitOfWork.BeginAsync(ct);
        try
        {
            var paymentOrder =
                await paymentOrderRepository.GetByIdAsync(paymentId);

            if (paymentOrder is null)
                throw new InvalidOperationException($"PaymentOrder {paymentId} não encontrada.");

            var transaction = Transaction.Create(paymentId, idempotencyKey);

            transaction.StartProcessing();
            paymentOrder.MarkAsInTransfer();

            await ValidateBalance(payerId, amount);

            var debit = LedgerEntry.CreateDebit(transaction.Id, payerId, amount);

            var credit = LedgerEntry.CreateCredit(transaction.Id, receiverId, amount);

            transaction.MarkAsCompleted();

            await transactionRepository.SaveAsync(transaction);

            await paymentOrderRepository.SaveAsync(paymentOrder, ct);

            await ledgerRepository.AddRangeAsync(
                [debit, credit]);

            await outboxService.AddEventAsync(
                new TransactionCompletedIntegrationEvent
                {
                    TransactionId = transaction.Id,
                    PaymentId = paymentId,
                    Amount = amount.Value,
                    CorrelationId = idempotencyKey
                }, ct);

            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
    private async Task HandleFailureFlow(Guid paymentId, string idempotencyKey, Exception exception, CancellationToken ct)
    {
        try
        {
            await unitOfWork.BeginAsync(ct);
            var paymentOrder =
                     await paymentOrderRepository.GetByIdAsync(paymentId);

            if (paymentOrder is not null)
            {
                paymentOrder.MarkAsFailed(exception.Message);

                await paymentOrderRepository.SaveAsync(
                    paymentOrder,
                    ct);
            }

            var transaction = await transactionRepository.GetByIdempotencyKeyAsync(idempotencyKey);

            transaction ??= Transaction.Create(paymentId, idempotencyKey);

            transaction.MarkAsFailed();

            await transactionRepository.SaveAsync(transaction);

            await outboxService.AddEventAsync(
                new TransactionFailedIntegrationEvent
                {
                    TransactionId = transaction.Id,
                    PaymentId = paymentId,
                    Reason = exception.Message,
                    CorrelationId = idempotencyKey
                },
                ct);

            await unitOfWork.CommitAsync(ct);
        }
        catch (Exception innerEx)
        {
            await unitOfWork.RollbackAsync(ct);

            logger.LogError(
                innerEx,
                "Erro ao registrar falha do PaymentId {PaymentId}",
                paymentId);
        }
    }
    private async Task ValidateBalance(Guid payerId, Money amount)
    {
        var balance = await balanceService.GetBalanceAsync(payerId);
        if (balance < amount)
            throw new InsufficientFundsException(payerId, amount, balance);

    }
}
