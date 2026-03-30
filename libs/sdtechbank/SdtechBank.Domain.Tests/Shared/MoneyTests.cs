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
}
