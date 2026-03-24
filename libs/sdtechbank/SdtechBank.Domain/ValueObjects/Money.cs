using SdtechBank.Domain.Enums;

namespace SdtechBank.Domain.ValueObjects;
public sealed record Money
{
    public decimal Amount { get; init; }
    public CurrencyEnum Currency { get; init; }

    public Money(decimal amount, CurrencyEnum currency)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero");
        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if(Currency != other.Currency)
            throw new InvalidOperationException("Moedas diferentes");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Moedas diferentes");

        var result = Amount - other.Amount;

        if (result < 0)
            throw new InvalidOperationException("Saldo insuficiente");
        return new Money(result, Currency);

    }
}
