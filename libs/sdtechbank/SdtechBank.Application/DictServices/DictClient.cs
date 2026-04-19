using System.Net;
using System.Net.Http.Json;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.DictServices;

public sealed class DictClient(HttpClient httpClient) : IDictClient
{
    public async Task<DictKeyResponse?> GetKeyAsync(PixKey key, CancellationToken ct)
    {
        var url = $"/keys/{Uri.EscapeDataString(key.Value)}?keyType={key.Type}";

        var response = await httpClient.GetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DictKeyResponse>(ct);
    }
}