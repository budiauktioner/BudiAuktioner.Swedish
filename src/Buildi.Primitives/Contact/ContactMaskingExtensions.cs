namespace Buildi.Primitives.Contact;

/// <summary>
/// Extension methods for masking sensitive contact information in display strings.
/// </summary>
public static class ContactMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked phone number showing the area/country prefix and optionally the last digits.
    /// Swedish example: <c>070-174 06 33</c> → <c>070-*** ** 33</c>.
    /// International example: <c>+44701234567</c> → <c>+44*******67</c>.
    /// Set <paramref name="visibleDigitsAtEnd"/> to <c>0</c> to mask all subscriber digits.
    /// </summary>
    public static string ToMaskedString(this PhoneNumber phone, int visibleDigitsAtEnd = 2)
    {
        var formatted = phone.ToString();

        int visiblePrefix;
        if (phone.IsSwedish)
        {
            var localStr = phone.ToLocalString();
            var dashPos = localStr.IndexOf('-');
            visiblePrefix = dashPos > 0
                ? localStr[..dashPos].Count(char.IsDigit)
                : 3;
        }
        else
        {
            visiblePrefix = phone.CountryCallingCode.Value.Length;
        }

        var totalDigits = formatted.Count(char.IsDigit);
        var revealEnd = Math.Min(Math.Max(0, visibleDigitsAtEnd), Math.Max(0, totalDigits - visiblePrefix));
        var maskEnd = totalDigits - revealEnd;

        var result = formatted.ToCharArray();
        var digitIdx = 0;
        for (var i = 0; i < result.Length; i++)
        {
            if (char.IsDigit(result[i]))
            {
                if (digitIdx >= visiblePrefix && digitIdx < maskEnd)
                    result[i] = MaskChar;
                digitIdx++;
            }
        }

        return new string(result);
    }
}
