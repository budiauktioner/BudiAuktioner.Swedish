using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Swedish postal code (<c>postnummer</c>) is a 5-digit code in the format <c>NNN NN</c>, administered by PostNord. International formats (for example Danish <c>DK-9000</c>, Dutch <c>1012 AB</c>, British <c>W1A 1AB</c>) are also supported.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnord.se/">PostNord</see> — Swedish postal service</description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Postnummer_i_Sverige">Wikipedia — Postnummer i Sverige</see></description></item>
/// </list>
/// </remarks>
public sealed class AddressZipCode : IEquatable<AddressZipCode>, IComparable<AddressZipCode>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Zip Code", "Postnummer", "📮", ["https://www.postnord.se/", "https://sv.wikipedia.org/wiki/Postnummer_i_Sverige"]);

    private const int MaxInputLength = 20;

    private static readonly Regex SwedishDigitsPattern = new(@"^\d{5}$", RegexOptions.Compiled);
    private static readonly Regex InternationalPattern = new(
        @"^(?:" +
        @"[0-9]{3}|" +
        @"[0-9]{4}|" +
        @"[0-9]{3}\s?[0-9]{2}|" +
        @"[0-9]{6}|" +
        @"[0-9]{7}|" +
        @"[0-9]{4}\s?[A-Z]{2}|" +
        @"[A-Z]{3}\s?[0-9]{4}|" +
        @"980[0-9]{2}|" +
        @"(?:[A-Z][0-9]|[A-Z][0-9][A-Z]|[A-Z]{2}[0-9]|[A-Z]{2}[0-9][A-Z])\s?[0-9][A-Z]{2}|" +
        @"[A-Z0-9]{7}|" +
        @"[A-Z]{2}[0-9]{3}|" +
        @"LT-?[0-9]{5}|" +
        @"LV-?[0-9]{4}|" +
        @"MD-?[0-9]{4}|" +
        @"DK[-\s]?[0-9]{4}" +
        @")$",
        RegexOptions.Compiled);

    /// <summary>
    /// Normalized value. For Swedish codes the 5 raw digits (e.g. "11453"),
    /// for international codes the cleaned/uppercased form (e.g. "DK-9000", "W1A1AB").
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Human-readable display form. For Swedish codes "XXX XX" (e.g. "114 53"),
    /// for international codes same as <see cref="Value"/>.
    /// </summary>
    public string Formatted { get; }

    /// <summary>
    /// The country this zip code belongs to, or <see langword="null"/> when the country could not be determined.
    /// Swedish 5-digit codes are always <see cref="Country.Sweden"/>.
    /// International codes with a known country prefix (e.g. <c>DK-</c>) resolve to the corresponding country.
    /// </summary>
    public Country? Country { get; }

    /// <summary>
    /// True if this is a Swedish 5-digit postal code.
    /// </summary>
    public bool IsSwedish => Country?.Alpha2Code == "SE";

    private AddressZipCode(string value, string formatted, Country? country)
    {
        Value = value;
        Formatted = formatted;
        Country = country;
    }

    /// <summary>
    /// Parse a zip code using the default country from <see cref="PrimitivesDefaults.CountryAlpha2Code"/>.
    /// When the default country is Sweden, tries Swedish format first, then international.
    /// </summary>
    public static bool TryParse(string? input, out AddressZipCode? result)
        => TryParse(input, (Country?)null, out result);

    /// <summary>
    /// Parse a zip code with an optional country hint. When <paramref name="country"/> is <see langword="null"/>,
    /// uses <see cref="PrimitivesDefaults.CountryAlpha2Code"/> to determine format priority.
    /// When the country is <see cref="Country.Sweden"/>, tries Swedish 5-digit format first.
    /// For any other country, only the international format is tried
    /// and <see cref="Country"/> is set to the given country on the result.
    /// </summary>
    public static bool TryParse(string? input, Country? country, out AddressZipCode? result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = null;
            return false;
        }

        input = InputSanitization.SanitizeInput(input!).Trim();
        if (input.Length > MaxInputLength)
        {
            result = null;
            return false;
        }

        var effectiveCountryCode = country?.Alpha2Code ?? PrimitivesDefaults.CountryAlpha2Code;

        if (country?.Alpha2Code == "SE")
            return TryParseSwedish(input, out result);

        if (country is not null)
        {
            if (TryParseWithCountryParser(input, country, out result))
                return true;
            return TryParseInternational(input, country, out result);
        }

        if (effectiveCountryCode == "SE")
        {
            if (TryParseSwedish(input, out result)) return true;
            return TryParseInternational(input, null, out result);
        }

        if (TryParseInternational(input, null, out result)) return true;
        return TryParseSwedish(input, out result);
    }

    /// <summary>
    /// Parse as a Swedish 5-digit postal code only.
    /// </summary>
    public static bool TryParseSwedish(string? input, out AddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = input!;
        for (var i = 0; i < cleaned.Length; i++)
        {
            if (char.IsLetter(cleaned[i]))
            {
                cleaned = cleaned[..i].TrimEnd();
                break;
            }
        }

        if (cleaned.Length == 0) return false;

        var digits = InputSanitization.KeepDigits(cleaned);
        if (!SwedishDigitsPattern.IsMatch(digits)) return false;

        var formatted = $"{digits[..3]} {digits[3..]}";
        result = new AddressZipCode(digits, formatted, Country.Sweden);
        return true;
    }

    /// <summary>
    /// Parse as an international (non-Swedish) postal code.
    /// </summary>
    public static bool TryParseInternational(string? input, out AddressZipCode? result)
        => TryParseInternational(input, null, out result);

    /// <summary>
    /// Parse as an international (non-Swedish) postal code and associate the result with the given country.
    /// When <paramref name="country"/> is <see langword="null"/>, the country is inferred from a known prefix
    /// (e.g. <c>DK-</c> → Denmark) when possible, otherwise <see cref="Country"/> is <see langword="null"/>.
    /// </summary>
    public static bool TryParseInternational(string? input, Country? country, out AddressZipCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        if (input!.Any(char.IsControl)) return false;
        if (input.Any(ch => !(char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch) || ch == '-')))
            return false;

        var normalized = NormalizeInternational(input);
        if (string.IsNullOrEmpty(normalized)) return false;
        if (!InternationalPattern.IsMatch(normalized)) return false;

        var resolvedCountry = country ?? InferCountryFromPrefix(normalized);
        result = new AddressZipCode(normalized, normalized, resolvedCountry);
        return true;
    }

    public static AddressZipCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid zip code.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the zip code in display form, for example <c>114 53</c> for Swedish codes or <c>DK-9000</c> for international codes.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized zip code, for example <c>11453</c> for Swedish codes or <c>DK-9000</c> for international codes.
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

    /// <summary>
    /// Returns the normalized zip code, for example <c>11453</c> for Swedish codes or <c>DK-9000</c> for international codes.
    /// </summary>
    public string ToNormalizedString() => Value;
    /// <summary>
    /// Returns the zip code in display form, for example <c>114 53</c> for Swedish codes or <c>DK-9000</c> for international codes.
    /// </summary>
    public override string ToString() => Formatted;

    private static Country? InferCountryFromPrefix(string normalized)
    {
        if (normalized.StartsWith("DK-", StringComparison.Ordinal))
            return Country.Denmark;

        if (normalized.StartsWith("LV-", StringComparison.Ordinal))
            return Country.Latvia;

        if (normalized.StartsWith("MD-", StringComparison.Ordinal))
        {
            Country.TryParse("MD", out var md);
            return md;
        }

        if (normalized.StartsWith("AD", StringComparison.Ordinal) && normalized.Length == 5 && normalized[2..].All(char.IsDigit))
        {
            Country.TryParse("AD", out var ad);
            return ad;
        }

        return null;
    }

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{3}\s?\d{2}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swedish 5-digit zip codes (e.g. <c>114 53</c> or <c>11453</c>).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real zip code in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<AddressZipCode>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<AddressZipCode>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParseSwedish(match.Value, out var zip)) continue;
            results.Add(new TextCandidate<AddressZipCode>(
                match.Index,
                match.Length,
                match.Value,
                nameof(AddressZipCode),
                TextCandidateCategory.Contact,
                zip!.ToNormalizedString(),
                zip.ToString(),
                zip.ToMaskedString(),
                TextMatchConfidence.Low,
                zip));
        }
        return results;
    }

    private static string NormalizeInternational(string input)
    {
        var cleaned = Regex.Replace(input.Trim().ToUpper(), @"\s+", "");

        if (cleaned.StartsWith("DK", StringComparison.OrdinalIgnoreCase))
        {
            var digits = InputSanitization.KeepDigits(cleaned);
            if (digits.Length == 4)
                return $"DK-{digits}";
        }

        if (cleaned.StartsWith("LV", StringComparison.OrdinalIgnoreCase))
        {
            var digits = InputSanitization.KeepDigits(cleaned);
            if (digits.Length == 4)
                return $"LV-{digits}";
        }

        if (cleaned.StartsWith("MD", StringComparison.OrdinalIgnoreCase))
        {
            var digits = InputSanitization.KeepDigits(cleaned);
            if (digits.Length == 4)
                return $"MD-{digits}";
        }

        if (cleaned.StartsWith("AD", StringComparison.OrdinalIgnoreCase))
        {
            var digits = InputSanitization.KeepDigits(cleaned);
            if (digits.Length == 3)
                return $"AD{digits}";
        }

        var normalized = Regex.Replace(cleaned, @"[^A-Z0-9\-]", "");

        if (Regex.IsMatch(normalized, @"^\d+\-\d+$"))
            return normalized.Replace("-", "");

        return normalized;
    }

    private static bool TryParseWithCountryParser(string? input, Country country, out AddressZipCode? result)
    {
        result = null;

        switch (country.Alpha2Code)
        {
            case "AT": if (AustrianAddressZipCode.TryParse(input, out var at)) { result = at!.ZipCode; return true; } return false;
            case "BE": if (BelgianAddressZipCode.TryParse(input, out var be)) { result = be!.ZipCode; return true; } return false;
            case "BG": if (BulgarianAddressZipCode.TryParse(input, out var bg)) { result = bg!.ZipCode; return true; } return false;
            case "CH": if (SwissAddressZipCode.TryParse(input, out var ch)) { result = ch!.ZipCode; return true; } return false;
            case "CY": if (CypriotAddressZipCode.TryParse(input, out var cy)) { result = cy!.ZipCode; return true; } return false;
            case "CZ": if (CzechAddressZipCode.TryParse(input, out var cz)) { result = cz!.ZipCode; return true; } return false;
            case "DE": if (GermanAddressZipCode.TryParse(input, out var de)) { result = de!.ZipCode; return true; } return false;
            case "DK": if (DanishAddressZipCode.TryParse(input, out var dk)) { result = dk!.ZipCode; return true; } return false;
            case "EE": if (EstonianAddressZipCode.TryParse(input, out var ee)) { result = ee!.ZipCode; return true; } return false;
            case "ES": if (SpanishAddressZipCode.TryParse(input, out var es)) { result = es!.ZipCode; return true; } return false;
            case "FI": if (FinnishAddressZipCode.TryParse(input, out var fi)) { result = fi!.ZipCode; return true; } return false;
            case "FR": if (FrenchAddressZipCode.TryParse(input, out var fr)) { result = fr!.ZipCode; return true; } return false;
            case "GB": if (BritishAddressZipCode.TryParse(input, out var gb)) { result = gb!.ZipCode; return true; } return false;
            case "GR": if (GreekAddressZipCode.TryParse(input, out var gr)) { result = gr!.ZipCode; return true; } return false;
            case "HR": if (CroatianAddressZipCode.TryParse(input, out var hr)) { result = hr!.ZipCode; return true; } return false;
            case "HU": if (HungarianAddressZipCode.TryParse(input, out var hu)) { result = hu!.ZipCode; return true; } return false;
            case "IE": if (IrishAddressZipCode.TryParse(input, out var ie)) { result = ie!.ZipCode; return true; } return false;
            case "IS": if (IcelandicAddressZipCode.TryParse(input, out var @is)) { result = @is!.ZipCode; return true; } return false;
            case "IT": if (ItalianAddressZipCode.TryParse(input, out var it)) { result = it!.ZipCode; return true; } return false;
            case "LI": if (LiechtensteinAddressZipCode.TryParse(input, out var li)) { result = li!.ZipCode; return true; } return false;
            case "LT": if (LithuanianAddressZipCode.TryParse(input, out var lt)) { result = lt!.ZipCode; return true; } return false;
            case "LU": if (LuxembourgishAddressZipCode.TryParse(input, out var lu)) { result = lu!.ZipCode; return true; } return false;
            case "LV": if (LatvianAddressZipCode.TryParse(input, out var lv)) { result = lv!.ZipCode; return true; } return false;
            case "MT": if (MalteseAddressZipCode.TryParse(input, out var mt)) { result = mt!.ZipCode; return true; } return false;
            case "NL": if (DutchAddressZipCode.TryParse(input, out var nl)) { result = nl!.ZipCode; return true; } return false;
            case "NO": if (NorwegianAddressZipCode.TryParse(input, out var no)) { result = no!.ZipCode; return true; } return false;
            case "PL": if (PolishAddressZipCode.TryParse(input, out var pl)) { result = pl!.ZipCode; return true; } return false;
            case "PT": if (PortugueseAddressZipCode.TryParse(input, out var pt)) { result = pt!.ZipCode; return true; } return false;
            case "RO": if (RomanianAddressZipCode.TryParse(input, out var ro)) { result = ro!.ZipCode; return true; } return false;
            case "SI": if (SlovenianAddressZipCode.TryParse(input, out var si)) { result = si!.ZipCode; return true; } return false;
            case "SK": if (SlovakAddressZipCode.TryParse(input, out var sk)) { result = sk!.ZipCode; return true; } return false;
            default: return false;
        }
    }

    public bool Equals(AddressZipCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is AddressZipCode other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(AddressZipCode? a, AddressZipCode? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(AddressZipCode? a, AddressZipCode? b) => !(a == b);
    public int CompareTo(AddressZipCode? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(AddressZipCode left, AddressZipCode right) => left.CompareTo(right) < 0;
    public static bool operator >(AddressZipCode left, AddressZipCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(AddressZipCode left, AddressZipCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(AddressZipCode left, AddressZipCode right) => left.CompareTo(right) >= 0;
}
