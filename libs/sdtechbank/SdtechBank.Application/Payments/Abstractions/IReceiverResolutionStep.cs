
using SdtechBank.Domain.PaymentOrders.Entities;

namespace SdtechBank.Application.Payments.Abstractions;

public interface IReceiverResolutionStep
{
    Task<Guid?> ResolveAsync(PaymentOrder payment, CancellationToken cancellationToken);
}
