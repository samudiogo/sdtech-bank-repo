using SdtechBank.Domain.PaymentOrders.Enums;

namespace SdtechBank.Domain.PaymentOrders.Services;

public sealed class RandomPixKeyHandler : IPixKeyHandler
{
    public PixKeyType Type => PixKeyType.RANDOM;
    public bool CanResolve(string value) => Guid.TryParse(value, out var _);

    public string Normalize(string value)=> Guid.Parse(value.PreNormalize()).ToString("D").ToLowerInvariant();
}
