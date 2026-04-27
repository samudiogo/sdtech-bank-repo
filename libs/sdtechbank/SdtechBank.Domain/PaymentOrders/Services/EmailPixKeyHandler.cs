using System.Net.Mail;
using SdtechBank.Domain.PaymentOrders.Enums;

namespace SdtechBank.Domain.PaymentOrders.Services;

public sealed class EmailPixKeyHandler : IPixKeyHandler
{
    public PixKeyType Type => PixKeyType.EMAIL;

    public bool CanResolve(string value)
    {
        value = value.PreNormalize();
        if (string.IsNullOrWhiteSpace(value))
            return false;
        try
        {
            var email = new MailAddress(value);
            return email.Address == value;
        }
        catch
        {
            return false;
        }
    }

    public string Normalize(string value) => value.PreNormalize();
}
