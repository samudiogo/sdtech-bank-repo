using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Transactions.Exceptions;

public class InsufficientFundsException(Guid payerId, Money requested, Money current) : Exception($"O pagador {payerId} tentou retirar {requested}, mas possui apenas {current} em saldo.")
{
    public Guid PayerId { get; } = payerId;
    public Money RequestedAmount { get; } = requested;
    public Money CurrentBalance { get; } = current;
}
