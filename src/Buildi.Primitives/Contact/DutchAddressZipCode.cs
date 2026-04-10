using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Dutch postal code (<c>postcode</c>) — format <c>NNNN AA</c> where the first four characters
/// are digits and the last two are uppercase letters. Administered by PostNL.
/// Rejects non-Dutch formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnl.nl/">PostNL</see> — Dutch postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_the_Netherlands">Wikipedia — Postal codes in the Netherlands</see></description></item>
/// </list>
/// </remarks>
public sealed class DutchAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Dutch Zip Code", "Nederländskt postnummer", "🇳🇱", ["https://www.postnl.nl/", "https://en.wikipedia.org/wiki/Postal_codes_in_the_Netherlands"]);

    private static readonly Regex ParsePattern = new(@"^\s*(\d{4})\s*([A-Za-z]{2})\s*$", RegexOptions.Compiled);

    public override Country Country => Country.Netherlands;

    private DutchAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out DutchAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = input.Replace("-", "").Replace("NL", "").Replace("nl", "");
        var match = ParsePattern.Match(cleaned);
        if (!match.Success) return false;

        var digits = match.Groups[1].Value;
        var letters = match.Groups[2].Value.ToUpperInvariant();

        var compact = $"{digits}{letters}";
        var formatted = $"{digits} {letters}";

        if (!AddressZipCode.TryParseInternational(formatted, Country.Netherlands, out var zipCode)) return false;

        result = new DutchAddressZipCode(compact, formatted, zipCode!);
        return true;
    }

    public static DutchAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Dutch zip code. Expected format NNNN AA (e.g. 1012 AB).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>1012 AB</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the compact form without spacing, e.g. <c>1012AB</c>.
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
