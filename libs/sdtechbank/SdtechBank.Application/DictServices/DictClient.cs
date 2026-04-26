using System.Net;
using System.Text.Json;
using SdtechBank.Domain.PaymentOrders.ValueObjects;

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

        var res = await response.Content.ReadAsStringAsync(ct);       

        return JsonSerializer.Deserialize<DictKeyResponse>(res);
        
    }
}