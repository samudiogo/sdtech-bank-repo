using SdtechBank.Domain.Shared.Enums;
using System.Globalization;

namespace SdtechBank.Domain.Shared.ValueObjects;

public sealed record Money
{
    public decimal Value { get; init; }
    public CurrencyType Currency { get; init; }

    public Money(decimal amount, CurrencyType currency)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero");
        Value = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        ValidateCurrency(other);

        return new Money(Value + other.Value, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Moedas diferentes");
        return new Money(Value - other.Value, Currency);
    }

    public override string ToString() => Value.ToString("C", Cultures.GetValueOrDefault(Currency, CultureInfo.InvariantCulture));

    public static bool operator <(Money a, Money b)
    {
        a.ValidateCurrency(b);
        return a.Value < b.Value;
    }

    public static bool operator >(Money a, Money b)
    {
        a.ValidateCurrency(b);
        return a.Value > b.Value;
    }


    internal void ValidateCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Moedas diferentes");
    }


    private static readonly Dictionary<CurrencyType, CultureInfo> Cultures = new() {
        { CurrencyType.BRL, new("pt-BR") },
        { CurrencyType.USD, new("en-US") },
        { CurrencyType.EUR, new("de-DE") }
    };
}
