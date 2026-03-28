namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentDestinationEvent
{
    public string? PixKey { get; init; }
    public BankAccountEvent? BankAccount { get; init; }
}

