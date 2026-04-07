
using SdtechBank.Domain.Deposits;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.Tests.Deposits;

public class DepositTests
{
    private static Deposit CreateValidDeposit()
    {
        return Deposit.Create(Guid.NewGuid(), new Money(100, CurrencyType.BRL), DepositSource.ATM);
    }

    [Fact]
    public void Deposit_Create_Should_StatusCreated()
    {
        var deposit = CreateValidDeposit();
        Assert.Equal(DepositStatus.CREATED, deposit.Status);
        Assert.Equal(DepositSource.ATM, deposit.Source);
        Assert.NotEqual(default, deposit.OccurredAt);
    }

    [Fact]
    public void Deposit_MarkAsCompleted_Should_NoErrors()
    {
        var deposit = CreateValidDeposit();

        deposit.MarkAsCompleted();

        Assert.Equal(DepositStatus.COMPLETED, deposit.Status);
        Assert.NotNull(deposit.CompletedAt);
    }

    [Fact]
    public void Deposit_StatusFailed_MarkAsCompleted_Should_RaiseException()
    {
        var deposit = CreateValidDeposit();
        deposit.MarkAsFailed();
        Assert.Throws<InvalidOperationException>(() => deposit.MarkAsCompleted());
    }

    [Fact]
    public void Deposit_MarkAsFailed_Should_NoErrors()
    {
        var deposit = CreateValidDeposit();

        deposit.MarkAsFailed();

        Assert.Equal(DepositStatus.FAILED, deposit.Status);
        Assert.NotNull(deposit.FailedAt);
    }

    [Fact]
    public void Deposit_StatusCompleted_MarkAsFailed_Should_RaiseException()
    {
        var deposit = CreateValidDeposit();
        deposit.MarkAsCompleted();
        Assert.Throws<InvalidOperationException>(() => deposit.MarkAsFailed());
    }


}