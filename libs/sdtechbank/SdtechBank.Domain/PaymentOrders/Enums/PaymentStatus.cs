namespace SdtechBank.Domain.PaymentOrders.Enums;

public enum PaymentStatus
{
    CREATED = 1,
    WAITING_CONFIRMATION = 2,
    WAITING_FOR_DICT = 3,
    READY_TO_TRANSFER = 4,
    IN_TRANSFER = 5,
    COMPLETED = 6,
    FAILED = 7
}
