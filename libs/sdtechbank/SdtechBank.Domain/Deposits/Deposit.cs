

using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.Deposits;

public sealed class Deposit
{
    public Guid Id { get; private set; }
    public Guid CreditAccountId { get; private set; }
    public Money Amount { get; private set; }

    public DepositSource Source { get; private set; }
    public DepositStatus Status { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }

    public static Deposit Create(Guid creditAccountId, Money amount, DepositSource source) => new()
    {
        Id = Guid.NewGuid(),
        CreditAccountId = creditAccountId,
        Amount = amount,
        Source = source,
        Status = DepositStatus.CREATED,
        OccurredAt = DateTime.UtcNow
    };

    public void MarkAsCompleted()
    {
        CompletedAt = DateTime.UtcNow;
        Status = DepositStatus.COMPLETED;
    }

    public void MarkAsFailed()
    {
        FailedAt = DateTime.UtcNow;
        Status = DepositStatus.FAILED;
    }
}
