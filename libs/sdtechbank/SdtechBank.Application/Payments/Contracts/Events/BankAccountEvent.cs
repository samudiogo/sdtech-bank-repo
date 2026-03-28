namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record BankAccountEvent
{
    public required string FullName { get; init; }
    public required string Cpf { get; init; }
    public required string BankCode { get; init; }
    public required string Branch { get; init; }
    public required string Account { get; init; }
}

