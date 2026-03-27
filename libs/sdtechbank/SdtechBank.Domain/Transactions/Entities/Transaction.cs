using SdtechBank.Domain.Transactions.Enums;

namespace SdtechBank.Domain.Transactions.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid PaymentOrderId { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transaction(Guid id, Guid paymentOrderId, TransactionStatus status, DateTime createdAt)
    {
        Id = id;
        PaymentOrderId = paymentOrderId;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Transaction Create(Guid paymentOrderId) => new(Guid.NewGuid(), paymentOrderId, TransactionStatus.CREATED, DateTime.UtcNow);

}
