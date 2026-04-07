using SdtechBank.Domain.Transactions.Enums;

namespace SdtechBank.Domain.Transactions.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid PaymentOrderId { get; private set; }
    public TransactionStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string IdempotencyKey { get; private set; }



    private Transaction(Guid id, Guid paymentOrderId, string idempotencyKey)
    {
        Id = id;
        PaymentOrderId = paymentOrderId;
        Status = TransactionStatus.CREATED;
        CreatedAt = DateTime.UtcNow;
        Attempts = 0;
        IdempotencyKey = idempotencyKey;
    }

    public static Transaction Create(Guid paymentOrderId, string idempontencyKey) => new(Guid.NewGuid(), paymentOrderId, idempontencyKey);

    public void StartProcessing()
    {
        IReadOnlyCollection<TransactionStatus> validStatusForComplete = [TransactionStatus.CREATED, TransactionStatus.FAILED];
        if (!validStatusForComplete.Any(s => s.Equals(Status)))
            throw new InvalidOperationException("Transição inválida para processamento.");

        Attempts++;
        Status = TransactionStatus.IN_PROGRESS;
    }

    public void MarkAsCompleted()
    {
        if (Status == TransactionStatus.COMPLETED) return;

        if (Status != TransactionStatus.IN_PROGRESS)
            throw new InvalidOperationException("Transição para 'COMPLETED' permitida apenas para transferência com status 'IN_PROGRESS'.");

        Status = TransactionStatus.COMPLETED;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        if (Status == TransactionStatus.FAILED) return;

        if (Status == TransactionStatus.COMPLETED)
            throw new InvalidOperationException("Operação inválida: transferência com status 'COMPLETED' não pode ser marcado como falha.");

        Status = TransactionStatus.FAILED;
        FailedAt = DateTime.UtcNow;
    }   

}
