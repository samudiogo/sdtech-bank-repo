using SdtechBank.Domain.Enums;
using SdtechBank.Domain.ValueObjects;

namespace SdtechBank.Domain.Entities;

/// <summary>
/// Representa uma ordem de pagamento contendo informações sobre o pagador, recebedor, valor, status e hora de criação.
/// </summary>
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

    /// <summary>
    /// Cria uma nova ordem de pagamento com o pagador, recebedor e valor especificados.
    /// </summary>
    /// <param name="payerId">O identificador único do usuário que realizará o pagamento. Não pode ser igual a <paramref
    /// name="receiverId"/>.</param>
    /// <param name="receiverId">O identificador único do usuário que receberá o pagamento. Não pode ser igual a <paramref
    /// name="payerId"/>.</param>
    /// <param name="amount">O valor a ser transferido na ordem de pagamento.</param>
    /// <returns>Uma nova instância de <see cref="PaymentOrder"/> representando a ordem de pagamento criada.</returns>
    /// <exception cref="InvalidOperationException">Lançada se <paramref name="payerId"/> e <paramref name="receiverId"/> forem iguais.</exception>
    public static PaymentOrder Create(Guid payerId, Guid receiverId, Money amount)
    {
        if (payerId.Equals(receiverId))
            throw new InvalidOperationException("O pagador não pode ser o mesmo que o recebedor");

        return new PaymentOrder(Guid.NewGuid(), payerId, receiverId, amount, PaymentStatusEnum.CREATED, DateTime.UtcNow);
    }

    /// <summary>
    /// Altera o status do pagamento para indicar que o processamento foi iniciado.
    /// </summary>
    /// <remarks>
    /// Chame este método para marcar um pagamento como sendo processado.
    /// Esta operação só é válida quando o pagamento está no estado 'Created'.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Lançada uma exceção se o status atual do pagamento não for 'Created'.</exception>
    public void MarkAsProcessing()
    {
        if (PaymentStatus != PaymentStatusEnum.CREATED)
            throw new InvalidOperationException("Só é possível processar pagamentos criados");
        PaymentStatus = PaymentStatusEnum.PROCESSING;
    }

    /// <summary>
    /// Altera o status do pagamento para indicar que o processamento foi confirmado com sucesso.
    /// </summary>
    /// <remarks>
    /// Chame este método para marcar um pagamento como sendo processado.
    /// Esta operação só é válida quando o pagamento está no estado 'PROCESSING'.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Lançada uma exceção se o status atual do pagamento não for 'PROCESSING'.</exception>
    public void MarkAsConfirmed()
    {
        if (PaymentStatus != PaymentStatusEnum.PROCESSING)
            throw new InvalidOperationException("Só é possível concluir pagamentos em processamento");
        PaymentStatus = PaymentStatusEnum.CONFIRMED;
    }

    /// <summary>
    /// Altera o status do pagamento para indicar que o processamento falhou.
    /// </summary>
    /// <remarks>
    /// Chame este método para marcar um pagamento como sendo processado.
    /// Esta operação só é válida quando o pagamento está no estado 'CONFIRMED'.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Lançada uma exceção se o status atual do pagamento não for 'CONFIRMED'.</exception>
    public void MarkAsFailed()
    {
        if (PaymentStatus == PaymentStatusEnum.CONFIRMED)
            throw new InvalidOperationException("Pagamento confirmado não pode falhar");
        PaymentStatus = PaymentStatusEnum.FAILED;
    }

}