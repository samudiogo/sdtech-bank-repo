using System.Text.RegularExpressions;
using SdtechBank.Domain.PaymentOrders.Enums;

namespace SdtechBank.Domain.PaymentOrders.Services;

public sealed class PhonePixKeyHandler : IPixKeyHandler
{
    public PixKeyType Type => PixKeyType.PHONE;

    public bool CanResolve(string value)
    {
        var normalized = Normalize(value);
        return Regex.IsMatch(normalized, @"^\+55\d{10,11}$");
    }

    public string Normalize(string value)
    {
        var digits = value.OnlyDigits();

        if (!digits.StartsWith("55"))
            digits = "55" + digits;

        return "+" + digits;
    }
}
