using System.Text.RegularExpressions;

using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Lithuanian postal code (<c>pašto kodas</c>) — strictly 5-digit format, commonly prefixed with
/// <c>LT-</c> for display (e.g. <c>LT-01001</c>). Administered by Lietuvos paštas.
/// Rejects non-Lithuanian formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.post.lt/">Lietuvos paštas</see> — Lithuanian postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Lithuania">Wikipedia — Postal codes in Lithuania</see></description></item>
/// </list>
/// </remarks>
public sealed class LithuanianAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Lithuanian Zip Code", "Litauiskt postnummer", "🇱🇹", ["https://www.post.lt/", "https://en.wikipedia.org/wiki/Postal_codes_in_Lithuania"]);

    private static readonly Regex DigitsPattern = new(@"^\d{5}$", RegexOptions.Compiled);

    public override Country Country => Country.Lithuania;

    private LithuanianAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out LithuanianAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = new string(input.Where(c => c is >= '0' and <= '9').ToArray());
        if (!DigitsPattern.IsMatch(digits)) return false;

        var formatted = $"LT-{digits}";
        if (!AddressZipCode.TryParseInternational(formatted, Country.Lithuania, out var zipCode)) return false;

        result = new LithuanianAddressZipCode(digits, formatted, zipCode!);
        return true;
    }

    public static LithuanianAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Lithuanian zip code. Expected 5-digit format (LT-NNNNN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>LT-01001</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized 5-digit form without prefix, e.g. <c>01001</c>.
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
