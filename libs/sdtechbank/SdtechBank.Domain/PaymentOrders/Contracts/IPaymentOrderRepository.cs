using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Domain.PaymentOrders.Contracts;

public interface IPaymentOrderRepository
{
    Task SaveAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken);
    Task<PaymentOrder?> GetByIdAsync(Guid paymentId);
    Task<PaymentOrder?> GetByIdempotencyKeyAsync(IdempotencyKey key, CancellationToken cancellationToken);

    Task<bool> ExistsRecentSimilarAsync(Guid payerId, PaymentDestination destination, Money amount, TimeSpan window, CancellationToken cancellationToken);
    Task<IEnumerable<PaymentOrder>> GetAllAsync(CancellationToken ct);
}
