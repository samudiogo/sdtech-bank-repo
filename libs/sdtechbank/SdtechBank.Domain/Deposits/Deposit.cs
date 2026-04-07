

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

    private Deposit() { }

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
        IReadOnlyCollection<DepositStatus> validStatusForComplete = [DepositStatus.CREATED];
        if (Status == DepositStatus.COMPLETED)
            return;
        
        if (!validStatusForComplete.Any(s => s.Equals(Status)))
            throw new InvalidOperationException($"Operação inválida: depósito com status '{Enum.GetName<DepositStatus>(Status)}' não pode ser marcado como 'COMPLETED'.");
        
        CompletedAt = DateTime.UtcNow;
        Status = DepositStatus.COMPLETED;
    }

    public void MarkAsFailed()
    {
        if (Status == DepositStatus.COMPLETED)
            throw new InvalidOperationException("Operação inválida: depósito com status 'COMPLETED' não pode ser marcado como falha.");

        FailedAt = DateTime.UtcNow;
        Status = DepositStatus.FAILED;
    }
}
