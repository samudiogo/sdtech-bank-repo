using SdtechBank.Domain.Transactions.Entities;
using SdtechBank.Domain.Transactions.Enums;

namespace SdtechBank.Domain.Tests.Transactions;

public class TransactionTests
{
    private static Transaction CreateValidTransaction()
    {
        return Transaction.Create(Guid.NewGuid(), "IdempotencyKey");
    }

    [Fact]
    public void Transaction_Create_Should_StatusCreated()
    {
        var transaction = CreateValidTransaction();
        Assert.Equal(TransactionStatus.CREATED, transaction.Status);
        Assert.Equal(0, transaction.Attempts);
        Assert.Equal("IdempotencyKey", transaction.IdempotencyKey);
        Assert.Null(transaction.CompletedAt);
        Assert.Null(transaction.FailedAt);
        Assert.NotEqual(default, transaction.CreatedAt);
    }

    [Fact]
    public void Transaction_MarkAsCompleted_Should_Return_Success()
    {
        var transaction = CreateValidTransaction();

        transaction.StartProcessing();
        transaction.MarkAsCompleted();

        Assert.Equal(TransactionStatus.COMPLETED, transaction.Status);
        Assert.Equal(1, transaction.Attempts);
        Assert.Equal("IdempotencyKey", transaction.IdempotencyKey);
        Assert.NotEqual(default, transaction.CompletedAt);
        Assert.NotNull(transaction.CompletedAt);
        Assert.Null(transaction.FailedAt);
        Assert.NotEqual(default, transaction.CreatedAt);
    }

    [Fact]
    public void Transaction_StartProcessing_Should_Return_Success()
    {
        var transaction = CreateValidTransaction();

        transaction.StartProcessing();

        Assert.Equal(TransactionStatus.IN_PROGRESS, transaction.Status);
        Assert.Equal(1, transaction.Attempts);
        Assert.Equal("IdempotencyKey", transaction.IdempotencyKey);
        Assert.Null(transaction.CompletedAt);
        Assert.Null(transaction.FailedAt); ;
        Assert.NotEqual(default, transaction.CreatedAt);
    }

    [Fact]
    public void Transaction_StartProcessing_Should_Return_Success_WHEN_CALL_TWICE()
    {
        var transaction = CreateValidTransaction();

        transaction.StartProcessing();
        transaction.MarkAsFailed();
        transaction.StartProcessing();

        Assert.Equal(TransactionStatus.IN_PROGRESS, transaction.Status);
        Assert.Equal(2, transaction.Attempts);
        Assert.Equal("IdempotencyKey", transaction.IdempotencyKey);
        Assert.Null(transaction.CompletedAt);
        Assert.NotNull(transaction.FailedAt); ;
        Assert.NotEqual(default, transaction.CreatedAt);
    }
    
    [Fact]
    public void Transaction_Completed_StartProcessing_Should_RaiseException()
    {
        var deposit = CreateValidTransaction();
        deposit.StartProcessing();
        deposit.MarkAsCompleted();
        Assert.Throws<InvalidOperationException>(() => deposit.StartProcessing());
    }

    [Fact]
    public void Transaction_MarkAsFailed_Should_Return_Success()
    {
        var transaction = CreateValidTransaction();

        transaction.StartProcessing();
        transaction.MarkAsFailed();

        Assert.Equal(TransactionStatus.FAILED, transaction.Status);
        Assert.Equal(1, transaction.Attempts);
        Assert.Equal("IdempotencyKey", transaction.IdempotencyKey);
        Assert.Null(transaction.CompletedAt);
        Assert.NotNull(transaction.FailedAt);
        Assert.NotEqual(default, transaction.FailedAt);
        Assert.NotEqual(default, transaction.CreatedAt);
    }
    [Fact]
    public void Transaction_StatusFailed_MarkAsCompleted_Should_RaiseException()
    {
        var deposit = CreateValidTransaction();
        deposit.MarkAsFailed();
        Assert.Throws<InvalidOperationException>(() => deposit.MarkAsCompleted());
    }
}