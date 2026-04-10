using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Danish postal code (<c>postnummer</c>) — strictly 4-digit format <c>NNNN</c>, administered
/// by PostNord Denmark. Rejects non-Danish formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnord.dk/">PostNord Denmark</see> — Danish postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Denmark">Wikipedia — Postal codes in Denmark</see></description></item>
/// </list>
/// </remarks>
public sealed class DanishAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Danish Zip Code", "Danskt postnummer", "🇩🇰", ["https://www.postnord.dk/", "https://en.wikipedia.org/wiki/Postal_codes_in_Denmark"]);

    private static readonly Regex DigitsPattern = new(@"^\d{4}$", RegexOptions.Compiled);

    public override Country Country => Country.Denmark;

    private DanishAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out DanishAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = new string(input.Where(c => c is >= '0' and <= '9').ToArray());
        if (!DigitsPattern.IsMatch(digits)) return false;

        if (!AddressZipCode.TryParseInternational(digits, Country.Denmark, out var zipCode)) return false;

        result = new DanishAddressZipCode(digits, digits, zipCode!);
        return true;
    }

    public static DanishAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Danish zip code. Expected 4-digit format (NNNN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>1050</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized 4-digit form, e.g. <c>1050</c>.
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
