namespace SdtechBank.Domain.PaymentOrders.Services;

public static class PixKeyNormalizer
{
    public static string PreNormalize(this string key) => key.Trim().ToLowerInvariant();

    
    public static string OnlyDigits(this string key) => new([.. key.Where(char.IsDigit)]);
}
