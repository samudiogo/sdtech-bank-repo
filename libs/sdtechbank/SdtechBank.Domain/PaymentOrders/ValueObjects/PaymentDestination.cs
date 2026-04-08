namespace SdtechBank.Domain.PaymentOrders.ValueObjects;

public sealed record PaymentDestination
{
    public string? PixKey { get; init; }
    public BankAccount? BankAccount { get; init; }

    private PaymentDestination(string? pixKey, BankAccount? bankAccount)
    {
        PixKey = pixKey;
        BankAccount = bankAccount;
    }

    public static PaymentDestination FromPixKey(string pixKey) => new(pixKey, null);
    public static PaymentDestination FromBankAccount(BankAccount bankAccount) => new(null, bankAccount);

    public bool IsPix() => !string.IsNullOrWhiteSpace(PixKey);

    public bool HasBankAccount() => BankAccount is not null;
}

public sealed record BankAccount
{
    public required string FullName { get; init; }
    public required string Cpf { get; init; }
    public required string BankCode { get; init; }
    public required string Branch { get; init; }
    public required string Account { get; init; }
}
