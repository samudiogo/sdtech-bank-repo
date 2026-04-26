using SdtechBank.Domain.PaymentOrders.Enums;

namespace SdtechBank.Domain.PaymentOrders.Services;

public interface IPixKeyHandler
{
    PixKeyType Type { get; }
    bool CanResolve(string value);
    string Normalize(string value);
}
