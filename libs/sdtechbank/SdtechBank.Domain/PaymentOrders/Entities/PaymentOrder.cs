using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.ValueObjects;
using System.Net.NetworkInformation;

namespace SdtechBank.Domain.PaymentOrders.Entities;

/// <summary>
/// Representa uma ordem de pagamento contendo informações sobre o pagador, recebedor, valor, status e hora de criação.
/// </summary>
public sealed class PaymentOrder
{
    public Guid Id { get; private set; }
    public Guid PayerId { get; private set; }
    public PaymentDestination Destination { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public int Attempts { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public Guid? TransactionId { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;

    private PaymentOrder() { }
    private PaymentOrder(Guid id, Guid payerId, PaymentDestination destination, Money amount, PaymentStatus paymentStatus, DateTime createdAt)
    {
        Id = id;
        PayerId = payerId;
        Destination = destination;
        Amount = amount;
        PaymentStatus = paymentStatus;
        CreatedAt = createdAt;
        Attempts = 0;
        IdempotencyKey= id.ToString();
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
    public static PaymentOrder Create(Guid payerId, PaymentDestination destination, Money amount)
    {
        return new PaymentOrder(Guid.NewGuid(), payerId, destination, amount, PaymentStatus.CREATED, DateTime.UtcNow);
    }

    /// <summary>
    /// Atualiza o pagamento para o status de aguardando confirmação.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Lançada quando o status atual não é 'CREATED'.
    /// </exception>
    public void MarkAsWaitingConfirmation()
    {
        if (PaymentStatus != PaymentStatus.CREATED)
            throw new InvalidOperationException("Transição para 'WAITING_CONFIRMATION' permitida apenas para pagamentos com status 'CREATED'.");

        PaymentStatus = PaymentStatus.WAITING_CONFIRMATION;
    }

    /// <summary>
    /// Atualiza o pagamento para o status de pronto para transferência.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Lançada quando o status atual não é 'WAITING_CONFIRMATION'.
    /// </exception>
    public void MarkAsReadyToTransfer()
    {
        if (PaymentStatus != PaymentStatus.WAITING_CONFIRMATION)
            throw new InvalidOperationException("Transição para 'READY_TO_TRANSFER' requer status 'WAITING_CONFIRMATION'.");

        PaymentStatus = PaymentStatus.READY_TO_TRANSFER;
    }

    /// <summary>
    /// Atualiza o pagamento para o status em processamento de transferência.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Lançada quando o status atual não é 'READY_TO_TRANSFER'.
    /// </exception>
    public void MarkAsInTransfer()
    {
        if (PaymentStatus != PaymentStatus.READY_TO_TRANSFER)
            throw new InvalidOperationException("Transição para 'IN_TRANSFER' requer status 'READY_TO_TRANSFER'.");

        PaymentStatus = PaymentStatus.IN_TRANSFER;
    }

    /// <summary>
    /// Atualiza o pagamento para o status confirmado.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Lançada quando o status atual não é 'IN_TRANSFER'.
    /// </exception>
    public void MarkAsCompleted(Guid transactionId)
    {
        if (PaymentStatus == PaymentStatus.COMPLETED)
            return;

        if (PaymentStatus != PaymentStatus.IN_TRANSFER)
            throw new InvalidOperationException("Confirmação permitida apenas para pagamentos em 'IN_TRANSFER'.");

        PaymentStatus = PaymentStatus.COMPLETED;
        CompletedAt = DateTime.UtcNow;
        TransactionId = transactionId;

    }

    /// <summary>
    /// Atualiza o pagamento para o status de falha.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Lançada quando o pagamento já está confirmado.
    /// </exception>
    public void MarkAsFailed(string reason)
    {
        if (PaymentStatus == PaymentStatus.COMPLETED)
            throw new InvalidOperationException("Operação inválida: pagamento com status 'CONFIRMED' não pode ser marcado como falha.");

        PaymentStatus = PaymentStatus.FAILED;
    }

}