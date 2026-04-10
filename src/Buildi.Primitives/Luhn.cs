namespace Buildi.Primitives;

internal static class Luhn
{
    /// <summary>
    /// Validates a standard Luhn (mod 10) checksum.
    /// Checks the entire string: the last digit is assumed to be the control digit.
    /// </summary>
    internal static bool IsValid(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return false;


        var sum = 0;
        var len = digits.Length;

        for (var i = 0; i < len; i++)
        {
            var d = digits[i] - '0';
            if (d < 0 || d > 9) return false;

            if ((len - i) % 2 == 0)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }

            sum += d;
        }

        return (sum % 10) == 0;
    }
}
