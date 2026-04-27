namespace SdtechBank.Domain.Shared.Documents;

public static class CpfValidator
{
    public static bool IsCpfValid(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var cpf = OnlyDigits(value);

        if (cpf.Length != 11)
            return false;

        if (AllDigitsEqual(cpf))
            return false;

        var firstDigit = CalculateDigit(cpf[..9], 10);
        var secondDigit = CalculateDigit(cpf[..10], 11);

        return cpf[9] == firstDigit && cpf[10] == secondDigit;
    }

    private static char CalculateDigit(string source, int weightStart)
    {
        var sum = 0;
        var weight = weightStart;

        foreach (var c in source)
        {
            sum += (c - '0') * weight;
            weight--;
        }

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return (char)('0' + digit);
    }

    private static string OnlyDigits(string value) =>
        new(value.Where(char.IsDigit).ToArray());

    private static bool AllDigitsEqual(string value) =>
        value.All(c => c == value[0]);
}
