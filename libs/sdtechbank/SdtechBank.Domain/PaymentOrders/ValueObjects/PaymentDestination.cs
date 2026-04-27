namespace SdtechBank.Domain.PaymentOrders.ValueObjects;

public sealed record PaymentDestination
{
    public PixKey? PixKey { get; init; }
    public BankAccount? BankAccount { get; init; }

    private PaymentDestination(string? pixKey, BankAccount? bankAccount)
    {
        var hasPix = !string.IsNullOrWhiteSpace(pixKey);
        var hasBank = bankAccount is not null;

        if (hasPix == hasBank)
            throw new InvalidOperationException("Destino inválido.");

        if (hasPix)
            PixKey = new PixKey(pixKey!);

        BankAccount = bankAccount;
    }

    public static PaymentDestination FromPixKey(string pixKey) => new(pixKey, null);
    public static PaymentDestination FromBankAccount(BankAccount bankAccount) => new(null, bankAccount);
    public PaymentDestination SetDestinationBankAccount(BankAccount bankAccount)
    {
        return this with { BankAccount = bankAccount };
    }

    public bool IsPix() => PixKey is not null;

    public bool HasBankAccount() => BankAccount is not null;

}

public sealed record BankAccount
{
    public required string FullName { get; init; }
    public required string Document { get; init; }
    public required string BankCode { get; init; }
    public required string Branch { get; init; }
    public required string Account { get; init; }
}
