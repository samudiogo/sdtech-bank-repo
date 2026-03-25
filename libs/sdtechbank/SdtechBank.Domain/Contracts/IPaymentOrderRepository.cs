using SdtechBank.Domain.Entities;

namespace SdtechBank.Domain.Contracts;

public interface IPaymentOrderRepository
{
    Task SaveAsync(PaymentOrder paymentOrder);
}
