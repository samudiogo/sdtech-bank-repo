
using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Domain.PaymentOrders.Events;

public sealed record PaymentOrderValidatedDomainEvent(Guid PaymentOrderId, PaymentDestination Destination);

