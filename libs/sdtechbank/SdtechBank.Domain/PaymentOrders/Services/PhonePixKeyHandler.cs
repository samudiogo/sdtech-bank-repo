using System.Text.RegularExpressions;
using SdtechBank.Domain.PaymentOrders.Enums;

namespace SdtechBank.Domain.PaymentOrders.Services;

public sealed partial class PhonePixKeyHandler : IPixKeyHandler
{
    public PixKeyType Type => PixKeyType.PHONE;

    [GeneratedRegex(@"^\+55\d{10,11}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PhoneRegex();

    public bool CanResolve(string value)
    {
        var normalized = Normalize(value);
        return PhoneRegex().IsMatch(normalized);
    }

    public string Normalize(string value)
    {
        var digits = value.OnlyDigits();

        if (!digits.StartsWith("55"))
            digits = "55" + digits;

        return "+" + digits;
    }
}
