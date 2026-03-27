namespace SdtechBank.Domain.Enums;

public enum PaymentStatusEnum
{
    CREATED = 1,
    WAITING_CONFIRMATION = 2,
    READY_TO_TRANSFER = 3,
    IN_TRANSFER = 4,
    CONFIRMED = 5,
    FAILED = 6
}
