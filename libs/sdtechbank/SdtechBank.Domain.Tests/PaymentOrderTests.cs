using SdtechBank.Domain.Entities;
using SdtechBank.Domain.Enums;
using SdtechBank.Domain.ValueObjects;

namespace SdtechBank.Domain.Tests;

public class PaymentOrderTests
{
    private static PaymentOrder CreateValidOrder()
    {
        var destination = PaymentDestination.FromPixKey("pixkey");
        return PaymentOrder.Create(
            Guid.NewGuid(),
            destination,
            new Money(100, CurrencyEnum.BRL)
        );
    }

    [Fact]
    public void Create_WithBankAccountDestination_ShouldSetStatusCreated()
    {
        var destination = PaymentDestination.FromBankAccount(new BankAccount { FullName = "name", BankCode = "0436", Branch = "1818", Account = "16435-2", Cpf = "00012345678" });
        var order = PaymentOrder.Create(Guid.NewGuid(), destination, new Money(100, CurrencyEnum.BRL));

        Assert.Equal(PaymentStatusEnum.CREATED, order.PaymentStatus);
    }

    [Fact]
    public void Create_WithPixKeyDestination_ShouldSetStatusCreated()
    {
        var order = CreateValidOrder();

        Assert.Equal(PaymentStatusEnum.CREATED, order.PaymentStatus);
    }
    
    [Fact]
    public void FullFlow_ShouldTransitionCorrectly()
    {
        var order = CreateValidOrder();

        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        order.MarkAsConfirmed();

        Assert.Equal(PaymentStatusEnum.CONFIRMED, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsWaitingConfirmation_WhenCreated_ShouldChangeStatus()
    {
        var order = CreateValidOrder();

        order.MarkAsWaitingConfirmation();

        Assert.Equal(PaymentStatusEnum.WAITING_CONFIRMATION, order.PaymentStatus);
    }    

    [Fact]
    public void MarkAsReadyToTransfer_WhenWaitingConfirmation_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        Assert.Equal(PaymentStatusEnum.READY_TO_TRANSFER, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsInTransfer_WhenReadyToTransfer_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        Assert.Equal(PaymentStatusEnum.IN_TRANSFER, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsConfirmed_WhenInTransfer_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        order.MarkAsConfirmed();
        Assert.Equal(PaymentStatusEnum.CONFIRMED, order.PaymentStatus);
    }    

    [Fact]
    public void MarkAsFailed_WhenCreated_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        order.MarkAsFailed();
        Assert.Equal(PaymentStatusEnum.FAILED, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsWaitingConfirmation_WhenNotCreated_ShouldThrow()
    {
        var order = CreateValidOrder();
        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        Assert.Throws<InvalidOperationException>(() => order.MarkAsWaitingConfirmation());
    }

    [Fact]
    public void MarkAsReadyToTransfer_WhenNotAsWaitingConfirmation_ShouldThrow()
    {
        var order = CreateValidOrder();
        Assert.Throws<InvalidOperationException>(() => order.MarkAsReadyToTransfer());
    }

    [Fact]
    public void MarkAsInTransfer_WhenNotAsReadyToTransfer_ShouldThrow()
    {
        var order = CreateValidOrder();
        order.MarkAsWaitingConfirmation();
        Assert.Throws<InvalidOperationException>(() => order.MarkAsInTransfer());
    }    

    [Fact]
    public void MarkAsConfirmed_WhenNotInTransfer_ShouldThrow()
    {
        var order = CreateValidOrder();

        Assert.Throws<InvalidOperationException>(() => order.MarkAsConfirmed());
    }

    [Fact]
    public void MarkAsFailed_WhenConfirmed_ShouldThrow()
    {
        var order = CreateValidOrder();

        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        order.MarkAsConfirmed();

        Assert.Throws<InvalidOperationException>(() => order.MarkAsFailed());
    }

}
