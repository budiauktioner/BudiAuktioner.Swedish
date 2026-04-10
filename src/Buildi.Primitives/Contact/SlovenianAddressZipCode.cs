using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Slovenian postal code (<c>poštna številka</c>) — strictly 4-digit format <c>NNNN</c>, administered
/// by Pošta Slovenije. Rejects non-Slovenian formats.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.posta.si/">Pošta Slovenije</see> — Slovenian postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Slovenia">Wikipedia — Postal codes in Slovenia</see></description></item>
/// </list>
/// </remarks>
public sealed class SlovenianAddressZipCode : CountryAddressZipCodeBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Slovenian Zip Code", "Slovenskt postnummer", "🇸🇮", ["https://www.posta.si/", "https://en.wikipedia.org/wiki/Postal_codes_in_Slovenia"]);

    private static readonly Regex DigitsPattern = new(@"^\d{4}$", RegexOptions.Compiled);

    public override Country Country => Country.Slovenia;

    private SlovenianAddressZipCode(string value, string formatted, AddressZipCode zipCode)
        : base(value, formatted, zipCode) { }

    public static bool TryParse(string? input, out SlovenianAddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = new string(input.Where(c => c is >= '0' and <= '9').ToArray());
        if (!DigitsPattern.IsMatch(digits)) return false;

        if (!AddressZipCode.TryParseInternational(digits, Country.Slovenia, out var zipCode)) return false;

        result = new SlovenianAddressZipCode(digits, digits, zipCode!);
        return true;
    }

    public static SlovenianAddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Slovenian zip code. Expected 4-digit format (NNNN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the zip code in display form <c>1000</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized 4-digit form, e.g. <c>1000</c>.
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
