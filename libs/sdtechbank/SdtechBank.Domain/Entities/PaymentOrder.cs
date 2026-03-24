using SdtechBank.Domain.Enums;
using SdtechBank.Domain.ValueObjects;

namespace SdtechBank.Domain.Entities;

public sealed class PaymentOrder
{
    public Guid Id { get; private set; }
    public Guid PayerId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatusEnum PaymentStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PaymentOrder(Guid id, Guid payerId, Guid receiverId, Money amount, PaymentStatusEnum paymentStatus, DateTime createdAt)
    {
        Id = id;
        PayerId = payerId;
        ReceiverId = receiverId;
        Amount = amount;
        PaymentStatus = paymentStatus;
        CreatedAt = createdAt;
    }

    public static PaymentOrder Create(Guid payerId, Guid receiverId, Money amount)
    {
        if (payerId.Equals(receiverId))
            throw new InvalidOperationException("O pagador não pode ser o mesmo que o recebedor");

        return new PaymentOrder(Guid.NewGuid(), payerId, receiverId, amount, PaymentStatusEnum.CREATED, DateTime.UtcNow);
    }

    public void MarkAsProcessing()
    {
        if (PaymentStatus != PaymentStatusEnum.CREATED)
            throw new InvalidOperationException("Só é possível processar pagamentos criados");
        PaymentStatus = PaymentStatusEnum.PROCESSING;
    }

    public void MarkAsConfirmed()
    {
        if (PaymentStatus != PaymentStatusEnum.PROCESSING)
            throw new InvalidOperationException("Só é possível concluir pagamentos em processamento");
        PaymentStatus = PaymentStatusEnum.CONFIRMED;
    }

    public void MarkAsFailed()
    {
        if (PaymentStatus != PaymentStatusEnum.CONFIRMED)
            throw new InvalidOperationException("Pagamento confirmado não pode falhar");
        PaymentStatus = PaymentStatusEnum.FAILED;
    }

}
