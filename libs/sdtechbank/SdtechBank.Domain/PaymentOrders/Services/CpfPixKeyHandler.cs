using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.Shared.Documents;

namespace SdtechBank.Domain.PaymentOrders.Services;

public sealed class CpfPixKeyHandler : IPixKeyHandler
{
    public PixKeyType Type => PixKeyType.CPF;

    public bool CanResolve(string value)=> value.PreNormalize().IsCpfValid();

    public string Normalize(string value) => value.OnlyDigits();
}
