using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.Tests.Shared;

public class MoneyTests
{
    [Fact]
    public void Create_ValidAmount_ShouldCreateInstance()
    {
        var amount = new Money(100, CurrencyType.BRL);

        Assert.Equal(100, amount.Value);
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        var money = new Money(100, CurrencyType.BRL);

        var result = money.Add(new Money(50, CurrencyType.BRL));

        Assert.Equal(150, result.Value);
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowException()
    {
        var money = new Money(100, CurrencyType.BRL);

        Assert.Throws<InvalidOperationException>(() =>
            money.Add(new Money(50, CurrencyType.USD))
        );
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldReturnDifference()
    {
        var money = new Money(100, CurrencyType.BRL);

        var result = money.Subtract(new Money(40, CurrencyType.BRL));

        Assert.Equal(60, result.Value);
    }

    [Fact]
    public void MoneyToString_Should_ReturnSuccess()
    {
        var money = new Money(100, CurrencyType.BRL);
        var expected = "R$ 100,00";

        var result = money.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Money_BiggerOperator_ShouldReturn_True()
    {
        var moneyA = new Money(100, CurrencyType.BRL);
        var moneyB = new Money(10, CurrencyType.BRL);

        var result = moneyA > moneyB;

        Assert.True(result);
    }

    [Fact]
    public void Money_LowerOperator_ShouldReturn_True()
    {
        var moneyA = new Money(100, CurrencyType.BRL);
        var moneyB = new Money(10, CurrencyType.BRL);

        var result = moneyB < moneyA;

        Assert.True(result);
    }
}
