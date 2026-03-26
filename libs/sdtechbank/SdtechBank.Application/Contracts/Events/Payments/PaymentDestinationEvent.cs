namespace SdtechBank.Application.Contracts.Events.Payments;

public sealed record PaymentDestinationEvent
{
    public string? PixKey { get; init; }
    public BankAccountEvent? BankAccount { get; init; }
}

