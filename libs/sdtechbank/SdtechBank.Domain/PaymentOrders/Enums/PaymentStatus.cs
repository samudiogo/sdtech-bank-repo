namespace SdtechBank.Domain.PaymentOrders.Enums;

public enum PaymentStatus
{
    CREATED = 1,
    WAITING_CONFIRMATION = 2,
    READY_TO_TRANSFER = 3,
    IN_TRANSFER = 4,
    COMPLETED = 5,
    FAILED = 6
}
