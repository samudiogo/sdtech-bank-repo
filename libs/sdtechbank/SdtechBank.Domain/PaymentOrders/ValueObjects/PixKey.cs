using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.Services;

namespace SdtechBank.Domain.PaymentOrders.ValueObjects;

public sealed record PixKey
{
    public string Value { get; init; } = default!;
    public PixKeyType Type { get; init; } = default!;

    public PixKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Chave pix não pode ser vazia ou nula");

        var handler = PixKeyResolver.Resolve(key);

        Value = handler.Normalize(key);

        Type = handler.Type;
    }    
}
