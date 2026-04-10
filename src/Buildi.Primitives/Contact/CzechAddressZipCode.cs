using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Czech postal code (<c>poštovní směrovací číslo</c>, PSČ) — strictly 5-digit format <c>NNN NN</c>,
/// administered by Česká pošta. Rejects non-Czech formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.ceskaposta.cz/">Česká pošta</see> — Czech postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_the_Czech_Republic">Wikipedia — Postal codes in the Czech Republic</see></description></item>
/// </list>
/// </remarks>
public sealed class CzechAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Czech Zip Code", "Tjeckiskt postnummer", "🇨🇿", ["https://www.ceskaposta.cz/", "https://en.wikipedia.org/wiki/Postal_codes_in_the_Czech_Republic"]);

    private static readonly Regex DigitsPattern = new(@"^\d{5}$", RegexOptions.Compiled);

    public override Country Country => Country.CzechRepublic;

    private CzechAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out CzechAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = new string(input.Where(c => c is >= '0' and <= '9').ToArray());
        if (!DigitsPattern.IsMatch(digits)) return false;

        var formatted = $"{digits[..3]} {digits[3..]}";
        if (!AddressZipCode.TryParseInternational(formatted, Country.CzechRepublic, out var zipCode)) return false;

        result = new CzechAddressZipCode(digits, formatted, zipCode!);
        return true;
    }

    public static CzechAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Czech zip code. Expected 5-digit format (NNN NN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>110 00</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized 5-digit form, e.g. <c>11000</c>.
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
