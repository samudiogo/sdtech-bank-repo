using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.Shared.Documents;

namespace SdtechBank.Domain.PaymentOrders.Services;

public sealed class CnpjPixKeyHandler : IPixKeyHandler
{
    public PixKeyType Type => PixKeyType.CNPJ;
    public bool CanResolve(string value) => value.PreNormalize().IsCnpjValid();
    public string Normalize(string value)=> value.OnlyDigits();
}
