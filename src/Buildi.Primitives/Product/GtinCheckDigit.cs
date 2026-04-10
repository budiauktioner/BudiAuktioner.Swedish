namespace Buildi.Primitives.Product;

internal static class GtinCheckDigit
{
    internal static readonly HashSet<int> ValidLengths = [8, 12, 13, 14];

    internal static string? ExtractDigits(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var chars = new char[input!.Length];
        var len = 0;
        foreach (var c in input)
        {
            if (c is >= '0' and <= '9')
                chars[len++] = c;
            else if (c is not (' ' or '-'))
                return null;
        }

        return len == 0 ? null : new string(chars, 0, len);
    }

    internal static bool Validate(string digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length - 1; i++)
        {
            var digit = digits[i] - '0';
            var posFromRight = digits.Length - 1 - i;
            var weight = posFromRight % 2 == 1 ? 3 : 1;
            sum += digit * weight;
        }

        var expected = (10 - sum % 10) % 10;
        var actual = digits[^1] - '0';
        return expected == actual;
    }
}
