using FluentAssertions;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.Tests.Payments;

public class PaymentOrderTests
{
    private static PaymentOrder CreateValidOrder()
    {
        var destination = PaymentDestination.FromPixKey("pixkey@pixkey.com");
        return PaymentOrder.Create(
            new IdempotencyKey(Guid.NewGuid().ToString()),
            Guid.NewGuid(),
            destination,
            new Money(100, CurrencyType.BRL)
        );
    }

    [Fact]
    public void Create_WithBankAccountDestination_ShouldSetStatusCreated()
    {
        var destination = PaymentDestination.FromBankAccount(new BankAccount { FullName = "name", BankCode = "0436", Branch = "1818", Account = "16435-2", Document = "00012345678" });
        var order = PaymentOrder.Create(new IdempotencyKey(Guid.NewGuid().ToString()), Guid.NewGuid(), destination, new Money(100, CurrencyType.BRL));

        Assert.Equal(PaymentStatus.CREATED, order.PaymentStatus);
    }

    [Fact]
    public void Create_WithPixKeyDestination_ShouldSetStatusCreated()
    {
        var order = CreateValidOrder();

        Assert.Equal(PaymentStatus.CREATED, order.PaymentStatus);
    }

    [Fact]
    public void FullFlow_ShouldTransitionCorrectly()
    {
        var order = CreateValidOrder();
        var transactionId = Guid.NewGuid();

        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        order.MarkAsCompleted(transactionId);

        Assert.Equal(PaymentStatus.COMPLETED, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsWaitingConfirmation_WhenCreated_ShouldChangeStatus()
    {
        var order = CreateValidOrder();

        order.MarkAsWaitingConfirmation();

        Assert.Equal(PaymentStatus.WAITING_CONFIRMATION, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsReadyToTransfer_WhenWaitingConfirmation_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        Assert.Equal(PaymentStatus.READY_TO_TRANSFER, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsInTransfer_WhenReadyToTransfer_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        Assert.Equal(PaymentStatus.IN_TRANSFER, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsCompleted_WhenInTransfer_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        var transactionId = Guid.NewGuid();
        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        order.MarkAsCompleted(transactionId);
        Assert.Equal(PaymentStatus.COMPLETED, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsFailed_WhenCreated_ShouldChangeStatus()
    {
        var order = CreateValidOrder();
        var errorMessage = "Error";
        order.MarkAsFailed(errorMessage);
        Assert.Equal(PaymentStatus.FAILED, order.PaymentStatus);
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
    public void MarkAsCompleted_WhenNotInTransfer_ShouldThrow()
    {
        var order = CreateValidOrder();
        var transactionId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() => order.MarkAsCompleted(transactionId));
    }

    [Fact]
    public void MarkAsFailed_WhenCompleted_ShouldThrow()
    {
        var order = CreateValidOrder();
        var transactionId = Guid.NewGuid();

        order.MarkAsWaitingConfirmation();
        order.MarkAsReadyToTransfer();
        order.MarkAsInTransfer();
        order.MarkAsCompleted(transactionId);

        Assert.Throws<InvalidOperationException>(() => order.MarkAsFailed(string.Empty));
    }

    [Fact]
    public void Should_Identify_Pix_Key_Destination()
    {
        var destination = PaymentDestination.FromPixKey("email@email.com");

        destination.IsPix().Should().BeTrue();
        destination.HasBankAccount().Should().BeFalse();
    }

    [Fact]
    public void Should_Identify_Bank_Account_Destination()
    {
        var destination = PaymentDestination.FromBankAccount(new()
        {
            FullName = "Teste",
            Document = "123",
            BankCode = "001",
            Branch = "0001",
            Account = "12345"
        });

        destination.IsPix().Should().BeFalse();
        destination.HasBankAccount().Should().BeTrue();
    }

}
