namespace Buildi.Primitives.Organization;

/// <summary>
/// Extension methods for masking sensitive organization identifiers in display strings.
/// </summary>
public static class OrganizationMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked display string. Person-based numbers (sole traders, personal identity numbers)
    /// are always masked, e.g. <c>990807-****</c>. Organization numbers are returned unmasked by default
    /// since they are public information.
    /// Set <paramref name="maskOrganizationNumbers"/> to <see langword="true"/> to mask non-person
    /// numbers too: <c>559246-****</c>.
    /// Set <paramref name="maskBirthDate"/> to <see langword="true"/> to also mask the date portion
    /// of person-based numbers: <c>******-****</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishOrganizationNumber orgNumber,
        bool maskOrganizationNumbers = false,
        bool maskBirthDate = false)
    {
        var display = orgNumber.To10DigitString();

        if (orgNumber.IsPerson)
        {
            var sep = display[6];
            if (maskBirthDate)
                return $"{new string(MaskChar, 6)}{sep}{new string(MaskChar, 4)}";
            return $"{display[..6]}{sep}{new string(MaskChar, 4)}";
        }

        if (maskOrganizationNumbers)
            return $"{display[..7]}{new string(MaskChar, 4)}";

        return display;
    }

    /// <summary>
    /// Returns a masked display string. Swedish VAT numbers based on a person's identity number
    /// are masked automatically, e.g. <c>SE990807****01</c>.
    /// Set <paramref name="alwaysMask"/> to <see langword="true"/> to mask all VAT numbers
    /// regardless of whether they are person-based.
    /// </summary>
    public static string ToMaskedString(this EuVatNumber vat, bool alwaysMask = false)
    {
        if (vat.CountryCode.Equals("SE", StringComparison.OrdinalIgnoreCase))
        {
            var body = vat.Body;
            if (body.Length >= 10)
            {
                var orgPart = body[..10];
                if (SwedishOrganizationNumber.TryParse(orgPart, out var org))
                {
                    if (org!.IsPerson || alwaysMask)
                    {
                        var suffix = body.Length > 10 ? body[10..] : "";
                        return $"{vat.VatPrefix}{orgPart[..6]}{new string(MaskChar, 4)}{suffix}";
                    }
                }
            }
        }
        else if (alwaysMask)
        {
            var visibleChars = Math.Min(4, vat.Body.Length);
            var masked = new string(MaskChar, vat.Body.Length - visibleChars);
            return $"{vat.VatPrefix}{vat.Body[..visibleChars]}{masked}";
        }

        return vat.ToString();
    }
}
