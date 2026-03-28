using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Create_ValidAmount_ShouldCreateInstance()
    {
        var amount = new Money(100, CurrencyEnum.BRL);

        Assert.Equal(100, amount.Amount);
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        var money = new Money(100, CurrencyEnum.BRL);

        var result = money.Add(new Money(50, CurrencyEnum.BRL));

        Assert.Equal(150, result.Amount);
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowException()
    {
        var money = new Money(100, CurrencyEnum.BRL);

        Assert.Throws<InvalidOperationException>(() =>
            money.Add(new Money(50, CurrencyEnum.USD))
        );
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldReturnDifference()
    {
        var money = new Money(100, CurrencyEnum.BRL);

        var result = money.Subtract(new Money(40, CurrencyEnum.BRL));

        Assert.Equal(60, result.Amount);
    }

    [Fact]
    public void Subtract_InsufficientAmount_ShouldThrowException()
    {
        var money = new Money(10, CurrencyEnum.BRL);

        Assert.Throws<InvalidOperationException>(() =>
            money.Subtract(new Money(20, CurrencyEnum.BRL))
        );
    }
}
