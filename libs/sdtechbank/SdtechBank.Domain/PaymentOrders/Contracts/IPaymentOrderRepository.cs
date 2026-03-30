using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Domain.PaymentOrders.Contracts;

public interface IPaymentOrderRepository
{
    Task SaveAsync(PaymentOrder paymentOrder);
    Task<PaymentOrder?> GetByIdAsync(Guid paymentId);
}
