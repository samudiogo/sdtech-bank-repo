namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentDestinationSnapshot
{
    public string? PixKey { get; init; }
    public BankAccountSnapshot? BankAccount { get; init; }
}

