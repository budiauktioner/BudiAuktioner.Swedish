using Buildi.Primitives;
using Buildi.Primitives.Person;

namespace Buildi.Primitives.Organization;

/// <summary>
/// Extension methods for converting between organization number types.
/// </summary>
public static class OrganizationExtensions
{
    // --- PIN/CN <-> SwedishOrganizationNumber ---

    /// <summary>
    /// Converts a Personal Identity Number to a Swedish Organization Number wrapper.
    /// </summary>
    public static SwedishOrganizationNumber ToSwedishOrganizationNumber(this SwedishPersonalIdentityNumber pin)
    {
        return SwedishOrganizationNumber.Parse(pin.To12DigitString());
    }

    /// <summary>
    /// Converts a Coordination Number to a Swedish Organization Number wrapper.
    /// </summary>
    public static SwedishOrganizationNumber ToSwedishOrganizationNumber(this SwedishCoordinationNumber cn)
    {
        return SwedishOrganizationNumber.Parse(cn.To12DigitString());
    }

    /// <summary>
    /// Attempts to extract the Personal Identity Number from a Swedish Organization Number.
    /// Returns null if the organization number is not a person-based number or is a Coordination Number.
    /// </summary>
    public static SwedishPersonalIdentityNumber? ToPersonalIdentityNumber(this SwedishOrganizationNumber son)
    {
        if (!son.IsPerson)
        {
            return null;
        }
        var s12 = son.To12DigitString();
        return SwedishPersonalIdentityNumber.TryParse(s12, out var pin) ? pin : null;
    }

    /// <summary>
    /// Attempts to extract the Coordination Number from a Swedish Organization Number.
    /// Returns null if the organization number is not a person-based number or is a Personal Identity Number.
    /// </summary>
    public static SwedishCoordinationNumber? ToCoordinationNumber(this SwedishOrganizationNumber son)
    {
        if (!son.IsPerson)
        {
            return null;
        }
        var s12 = son.To12DigitString();
        return SwedishCoordinationNumber.TryParse(s12, out var cn) ? cn : null;
    }

    // --- VAT <-> SwedishOrganizationNumber/PIN/CN ---

    /// <summary>
    /// Returns the underlying Swedish Organization Number if this VAT number is a valid SE VAT number.
    /// </summary>
    public static SwedishOrganizationNumber? ToSwedishOrganizationNumber(this EuVatNumber vat)
    {
        if (!vat.CountryCode.Equals("SE", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        var digits = InputSanitization.KeepDigits(vat.Body);
        if (digits.Length != 12 || !digits.EndsWith("01", StringComparison.Ordinal))
        {
            return null;
        }
        var core = digits.Substring(0, 10);
        return SwedishOrganizationNumber.TryParse(core, out var son) ? son : null;
    }

    /// <summary>
    /// Returns the underlying Personal Identity Number if this VAT number is based on a Swedish PIN.
    /// </summary>
    public static SwedishPersonalIdentityNumber? ToPersonalIdentityNumber(this EuVatNumber vat)
    {
        var son = vat.ToSwedishOrganizationNumber();
        return son?.ToPersonalIdentityNumber();
    }

    /// <summary>
    /// Returns the underlying Coordination Number if this VAT number is based on a Swedish CN.
    /// </summary>
    public static SwedishCoordinationNumber? ToCoordinationNumber(this EuVatNumber vat)
    {
        var son = vat.ToSwedishOrganizationNumber();
        return son?.ToCoordinationNumber();
    }

    /// <summary>
    /// Converts a Swedish Organization Number to its corresponding SE VAT number (SE + 10 digits + 01).
    /// </summary>
    public static EuVatNumber ToEuVatNumber(this SwedishOrganizationNumber son)
    {
        return CreateSeEuVatNumber(son.To10DigitsOnly());
    }

    /// <summary>
    /// Converts a Personal Identity Number to its corresponding SE VAT number (SE + 10 digits + 01).
    /// </summary>
    public static EuVatNumber ToEuVatNumber(this SwedishPersonalIdentityNumber pin)
    {
        var digits = InputSanitization.KeepDigits(pin.To10DigitString());
        return CreateSeEuVatNumber(digits);
    }

    /// <summary>
    /// Converts a Coordination Number to its corresponding SE VAT number (SE + 10 digits + 01).
    /// </summary>
    public static EuVatNumber ToEuVatNumber(this SwedishCoordinationNumber cn)
    {
        var digits = InputSanitization.KeepDigits(cn.To10DigitString());
        return CreateSeEuVatNumber(digits);
    }

    private static EuVatNumber CreateSeEuVatNumber(string tenDigits)
    {
        var vatString = $"SE{tenDigits}01";
        return EuVatNumber.Parse(vatString);
    }
}
