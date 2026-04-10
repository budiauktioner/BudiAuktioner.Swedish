using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A German postal code (<c>Postleitzahl</c>, PLZ) — strictly 5-digit format <c>NNNNN</c>, administered
/// by Deutsche Post. Rejects non-German formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.deutschepost.de/">Deutsche Post</see> — German postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Germany">Wikipedia — Postal codes in Germany</see></description></item>
/// </list>
/// </remarks>
public sealed class GermanAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("German Zip Code", "Tyskt postnummer", "🇩🇪", ["https://www.deutschepost.de/", "https://en.wikipedia.org/wiki/Postal_codes_in_Germany"]);

    private static readonly Regex DigitsPattern = new(@"^\d{5}$", RegexOptions.Compiled);

    public override Country Country => Country.Germany;

    private GermanAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out GermanAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = new string(input.Where(c => c is >= '0' and <= '9').ToArray());
        if (!DigitsPattern.IsMatch(digits)) return false;

        if (!AddressZipCode.TryParseInternational(digits, Country.Germany, out var zipCode)) return false;

        result = new GermanAddressZipCode(digits, digits, zipCode!);
        return true;
    }

    public static GermanAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid German zip code. Expected 5-digit format (NNNNN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>10115</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized 5-digit form, e.g. <c>10115</c>.
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
