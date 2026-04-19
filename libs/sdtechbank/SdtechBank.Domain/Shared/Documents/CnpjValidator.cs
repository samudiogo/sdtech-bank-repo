namespace SdtechBank.Domain.Shared.Documents;

public static class CnpjValidator
{
    private static readonly int[] FirstWeights =
        [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private static readonly int[] SecondWeights =
        [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static bool IsCnpjValid(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var cnpj = OnlyDigits(value);

        if (cnpj.Length != 14)
            return false;

        if (AllDigitsEqual(cnpj))
            return false;

        var firstDigit = CalculateDigit(cnpj[..12], FirstWeights);
        var secondDigit = CalculateDigit(cnpj[..13], SecondWeights);

        return cnpj[12] == firstDigit && cnpj[13] == secondDigit;
    }

    private static char CalculateDigit(string source, int[] weights)
    {
        var sum = 0;

        for (var i = 0; i < source.Length; i++)
            sum += (source[i] - '0') * weights[i];

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return (char)('0' + digit);
    }

    private static string OnlyDigits(string value) =>
        new(value.Where(char.IsDigit).ToArray());

    private static bool AllDigitsEqual(string value) =>
        value.All(c => c == value[0]);
}