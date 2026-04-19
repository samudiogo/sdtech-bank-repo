using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.DictServices;

public interface IDictClient
{
    Task<DictKeyResponse?> GetKeyAsync(PixKey key, CancellationToken ct);
}
