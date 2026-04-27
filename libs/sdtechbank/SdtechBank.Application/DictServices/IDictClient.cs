using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Application.DictServices;

public interface IDictClient
{
    Task<DictKeyResponse?> GetKeyAsync(PixKey key, CancellationToken ct);
}
