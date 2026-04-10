using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Cypriot postal code (<c>ταχυδρομικός κώδικας</c>) — strictly 4-digit format <c>NNNN</c>,
/// administered by Cyprus Postal Services. Rejects non-Cypriot formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.cypruspost.post/en/find-postal-codes">Cyprus Post — Find postal codes</see> — official Cypriot postal code lookup</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Cyprus">Wikipedia — Postal codes in Cyprus</see></description></item>
/// </list>
/// </remarks>
public sealed class CypriotAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Cypriot Zip Code", "Cypriotiskt postnummer", "🇨🇾", ["https://www.cypruspost.post/en/find-postal-codes", "https://en.wikipedia.org/wiki/Postal_codes_in_Cyprus"]);

    private static readonly Regex DigitsPattern = new(@"^\d{4}$", RegexOptions.Compiled);

    public override Country Country => Country.Cyprus;

    private CypriotAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out CypriotAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = new string(input.Where(c => c is >= '0' and <= '9').ToArray());
        if (!DigitsPattern.IsMatch(digits)) return false;

        if (!AddressZipCode.TryParseInternational(digits, Country.Cyprus, out var zipCode)) return false;

        result = new CypriotAddressZipCode(digits, digits, zipCode!);
        return true;
    }

    public static CypriotAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Cypriot zip code. Expected 4-digit format (NNNN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>1060</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim()
        : null;

    /// <summary>
    /// Returns the normalized 4-digit form, e.g. <c>1060</c>.
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
