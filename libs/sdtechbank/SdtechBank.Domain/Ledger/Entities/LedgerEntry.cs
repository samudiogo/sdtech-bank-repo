using SdtechBank.Domain.Ledger.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.Ledger.Entities;

public sealed class LedgerEntry
{
    public Guid Id { get; private set; }
    public Guid TransactionId { get; private set; }
    public Guid AccountId { get; private set; }
    public Money Amount { get; private set; }
    public LedgerEntryType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    private LedgerEntry() { }
    private LedgerEntry(Guid transactionId, Guid accountId, Money amount, LedgerEntryType type)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        AccountId = accountId;
        Amount = amount;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public static LedgerEntry CreateDebit(Guid transactionId, Guid accountId, Money amount) => new(transactionId, accountId, amount, LedgerEntryType.DEBIT);

    public static LedgerEntry CreateCredit(Guid transactionId, Guid accountId, Money amount) => new(transactionId, accountId, amount, LedgerEntryType.CREDIT);
}

