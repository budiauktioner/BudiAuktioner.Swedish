using System.Text.RegularExpressions;

using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Portuguese postal code (<c>código postal</c>) — 7-digit format <c>NNNN-NNN</c>, administered
/// by CTT (Correios de Portugal). Rejects non-Portuguese formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.ctt.pt/">CTT — Correios de Portugal</see> — Portuguese postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Portugal">Wikipedia — Postal codes in Portugal</see></description></item>
/// </list>
/// </remarks>
public sealed class PortugueseAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Portuguese Zip Code", "Portugisiskt postnummer", "🇵🇹", ["https://www.ctt.pt/", "https://en.wikipedia.org/wiki/Postal_codes_in_Portugal"]);

    private static readonly Regex DigitsPattern = new(@"^\d{7}$", RegexOptions.Compiled);

    public override Country Country => Country.Portugal;

    private PortugueseAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out PortugueseAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = new string(input.Where(c => c is >= '0' and <= '9').ToArray());
        if (!DigitsPattern.IsMatch(digits)) return false;

        var formatted = $"{digits[..4]}-{digits[4..]}";
        if (!AddressZipCode.TryParseInternational(formatted, Country.Portugal, out var zipCode)) return false;

        result = new PortugueseAddressZipCode(digits, formatted, zipCode!);
        return true;
    }

    public static PortugueseAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Portuguese zip code. Expected 7-digit format (NNNN-NNN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>1100-148</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized 7-digit form, e.g. <c>1100148</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;
}
