
namespace SdtechBank.Domain.Transactions.Events;

public sealed record PaymentOrderCreatedEvent(Guid PaymentOrderId, decimal Amount, string CorrelationId);
