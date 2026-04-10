using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// An Irish postal code (Eircode) — a 7-character alphanumeric code in the format <c>ANN XXXX</c>,
/// where the first three characters form a routing key and the last four identify a specific address.
/// Administered by An Post. Rejects non-Irish formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.anpost.com/">An Post</see> — Irish postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Eircode">Wikipedia — Eircode</see></description></item>
/// </list>
/// </remarks>
public sealed class IrishAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Irish Eircode", "Irländskt postnummer", "🇮🇪", ["https://www.anpost.com/", "https://en.wikipedia.org/wiki/Eircode"]);

    private static readonly Regex EircodePattern = new(
        @"^\s*([A-Z]\d[A-Z\d])\s?([A-Z\d]{4})\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override Country Country => Country.Ireland;

    private IrishAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out IrishAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = EircodePattern.Match(input);
        if (!match.Success) return false;

        var routing = match.Groups[1].Value.ToUpperInvariant();
        var unique = match.Groups[2].Value.ToUpperInvariant();

        var compact = $"{routing}{unique}";
        var formatted = $"{routing} {unique}";

        if (!AddressZipCode.TryParseInternational(formatted, Country.Ireland, out var zipCode)) return false;

        result = new IrishAddressZipCode(compact, formatted, zipCode!);
        return true;
    }

    public static IrishAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Irish Eircode. Expected format like D02 XR20.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the Eircode in display form <c>D02 XR20</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the compact uppercase form without spacing, e.g. <c>D02XR20</c>.
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
