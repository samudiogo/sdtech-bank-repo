using SdtechBank.Domain.Entities;
using SdtechBank.Domain.Enums;
using SdtechBank.Domain.ValueObjects;

namespace SdtechBank.Domain.Tests;

public class PaymentOrderTests
{
    private static PaymentOrder CreateValidOrder()
    {
        return PaymentOrder.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(100, CurrencyEnum.BRL)
        );
    }

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var order = CreateValidOrder();

        Assert.Equal(PaymentStatusEnum.CREATED, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsProcessing_WhenCreated_ShouldChangeStatus()
    {
        var order = CreateValidOrder();

        order.MarkAsProcessing();

        Assert.Equal(PaymentStatusEnum.PROCESSING, order.PaymentStatus);
    }

    [Fact]
    public void FullFlow_ShouldTransitionCorrectly()
    {
        var order = CreateValidOrder();

        order.MarkAsProcessing();
        order.MarkAsConfirmed();

        Assert.Equal(PaymentStatusEnum.CONFIRMED, order.PaymentStatus);
    }

    [Fact]
    public void MarkAsFailed_WhenCreated_ShouldChangeStatus()
    {
        var order = CreateValidOrder();

        order.MarkAsProcessing();
        order.MarkAsFailed();

        Assert.Equal(PaymentStatusEnum.FAILED, order.PaymentStatus);
    }

    [Fact]
    public void Create_WithSamePayerAndReceiver_ShouldThrow()
    {
        var id = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() => PaymentOrder.Create(id, id, new Money(100, CurrencyEnum.BRL)));
    }

    [Fact]
    public void MarkAsProcessing_WhenNotCreated_ShouldThrow()
    {
        var order = CreateValidOrder();
        order.MarkAsProcessing();

        Assert.Throws<InvalidOperationException>(() => order.MarkAsProcessing());
    }

    [Fact]
    public void MarkAsConfirmed_WhenNotProcessing_ShouldThrow()
    {
        var order = CreateValidOrder();        

        Assert.Throws<InvalidOperationException>(() => order.MarkAsConfirmed());
    }

    [Fact]
    public void MarkAsFailed_WhenConfirmed_ShouldThrow()
    {
        var order = CreateValidOrder();

        order.MarkAsProcessing();
        order.MarkAsConfirmed();

        Assert.Throws<InvalidOperationException>(() => order.MarkAsConfirmed());
    }   

}
