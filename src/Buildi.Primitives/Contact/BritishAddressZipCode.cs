using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A British postcode — alphanumeric format with an outward code and an inward code separated by a space,
/// for example <c>SW1A 1AA</c>. Administered by Royal Mail. Rejects non-UK formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.royalmail.com/">Royal Mail</see> — UK postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postcodes_in_the_United_Kingdom">Wikipedia — Postcodes in the United Kingdom</see></description></item>
/// </list>
/// </remarks>
public sealed class BritishAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("British Postcode", "Brittiskt postnummer", "🇬🇧", ["https://www.royalmail.com/", "https://en.wikipedia.org/wiki/Postcodes_in_the_United_Kingdom"]);

    private static readonly Regex PostcodePattern = new(
        @"^\s*([A-Z]{1,2}\d[A-Z\d]?)\s*(\d[A-Z]{2})\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override Country Country => Country.UnitedKingdom;

    private BritishAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out BritishAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = PostcodePattern.Match(input);
        if (!match.Success) return false;

        var outward = match.Groups[1].Value.ToUpperInvariant();
        var inward = match.Groups[2].Value.ToUpperInvariant();

        var compact = $"{outward}{inward}";
        var formatted = $"{outward} {inward}";

        if (!AddressZipCode.TryParseInternational(formatted, Country.UnitedKingdom, out var zipCode)) return false;

        result = new BritishAddressZipCode(compact, formatted, zipCode!);
        return true;
    }

    public static BritishAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid British postcode. Expected format like SW1A 1AA.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the postcode in display form <c>SW1A 1AA</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the compact uppercase form without spacing, e.g. <c>SW1A1AA</c>.
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
