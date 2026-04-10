using System.Text.RegularExpressions;

using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Maltese postal code — format <c>AAA NNNN</c> where the first three characters are uppercase
/// letters identifying the locality and the last four are digits. Administered by MaltaPost.
/// Rejects non-Maltese formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.maltapost.com/">MaltaPost</see> — Maltese postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Malta">Wikipedia — Postal codes in Malta</see></description></item>
/// </list>
/// </remarks>
public sealed class MalteseAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Maltese Zip Code", "Maltesiskt postnummer", "🇲🇹", ["https://www.maltapost.com/", "https://en.wikipedia.org/wiki/Postal_codes_in_Malta"]);

    private static readonly Regex ParsePattern = new(
        @"^\s*([A-Za-z]{3})\s*(\d{4})\s*$",
        RegexOptions.Compiled);

    public override Country Country => Country.Malta;

    private MalteseAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out MalteseAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = ParsePattern.Match(input);
        if (!match.Success) return false;

        var letters = match.Groups[1].Value.ToUpperInvariant();
        var digits = match.Groups[2].Value;

        var compact = $"{letters}{digits}";
        var formatted = $"{letters} {digits}";

        if (!AddressZipCode.TryParseInternational(formatted, Country.Malta, out var zipCode)) return false;

        result = new MalteseAddressZipCode(compact, formatted, zipCode!);
        return true;
    }

    public static MalteseAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Maltese zip code. Expected format AAA NNNN (e.g. VLT 1535).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>VLT 1535</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the compact uppercase form without spacing, e.g. <c>VLT1535</c>.
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
