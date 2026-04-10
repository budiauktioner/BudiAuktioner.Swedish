using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Finance;

/// <summary>
/// An International Securities Identification Number (ISIN) as defined by ISO 6166. An ISIN is a 12-character
/// alphanumeric code that uniquely identifies a financial security. The first two characters are an
/// ISO 3166-1 alpha-2 country code, followed by a 9-character alphanumeric National Securities Identifying
/// Number (NSIN), and a single Luhn check digit.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/standard/78502.html">ISO 6166</see> — International Securities Identification Number (ISIN)</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/International_Securities_Identification_Number">Wikipedia — ISIN</see></description></item>
/// <item><description><see href="https://www.anna-web.org/">ANNA — Association of National Numbering Agencies</see></description></item>
/// </list>
/// </remarks>
public sealed class Isin : IEquatable<Isin>, IComparable<Isin>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("ISIN", "ISIN", "📈", ["https://www.iso.org/standard/78502.html", "https://en.wikipedia.org/wiki/International_Securities_Identification_Number", "https://www.anna-web.org/"]);

    private const int IsinLength = 12;
    private const int MaxInputLength = 30;

    /// <summary>The 12-character ISIN in uppercase, for example <c>SE0000108656</c>.</summary>
    public string Value { get; }

    /// <summary>The ISO 3166-1 alpha-2 country code (first 2 characters), for example <c>SE</c>.</summary>
    public string CountryCode => Value[..2];

    /// <summary>The 9-character National Securities Identifying Number (characters 3–11), for example <c>000010865</c>.</summary>
    public string Nsin => Value[2..11];

    /// <summary>The Luhn check digit (last character), for example <c>6</c>.</summary>
    public char CheckDigit => Value[11];

    private Isin(string value)
    {
        Value = value;
    }

    public static bool TryParse(string? input, out Isin? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = InputSanitization.KeepAsciiAlphanumericUppercase(input);
        if (cleaned.Length > MaxInputLength) return false;
        if (cleaned.Length != IsinLength) return false;

        if (cleaned[0] is < 'A' or > 'Z' || cleaned[1] is < 'A' or > 'Z') return false;
        if (cleaned[11] is < '0' or > '9') return false;

        for (var i = 2; i < 11; i++)
        {
            var c = cleaned[i];
            if (c is not ((>= '0' and <= '9') or (>= 'A' and <= 'Z'))) return false;
        }

        if (!IsValidLuhnCheckDigit(cleaned)) return false;

        result = new Isin(cleaned);
        return true;
    }

    public static Isin Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid ISIN (length, format, or check digit invalid).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the ISIN as 12 uppercase characters, for example <c>SE0000108656</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => TryParse(input, out var r)
            ? r!.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized ISIN as 12 uppercase characters, for example <c>SE0000108656</c>.
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
    /// Returns the ISIN as 12 uppercase characters, for example <c>SE0000108656</c>.
    /// </summary>
    public string ToNormalizedString() => Value;

    /// <summary>
    /// Returns the ISIN as 12 uppercase characters, for example <c>SE0000108656</c>.
    /// </summary>
    public override string ToString() => Value;

    private static bool IsValidLuhnCheckDigit(string isin)
    {
        var sb = new System.Text.StringBuilder(isin.Length * 2);
        foreach (var c in isin)
        {
            if (c is >= '0' and <= '9')
                sb.Append(c);
            else if (c is >= 'A' and <= 'Z')
                sb.Append(c - 'A' + 10);
            else
                return false;
        }

        return Luhn.IsValid(sb.ToString());
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"\b[A-Z]{2}[A-Z0-9]{9}\d\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for substrings that look like ISINs (e.g. <c>SE0000108656</c>).
    /// Each candidate is validated with the Luhn check digit. This is heuristic-based.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Isin>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Isin>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var isin)) continue;
            results.Add(new TextCandidate<Isin>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Isin), TextCandidateCategory.Financial,
                isin!.ToNormalizedString(), isin.ToString(),
                isin.ToMaskedString(),
                TextMatchConfidence.High,
                isin));
        }
        return results;
    }

    public bool Equals(Isin? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Isin other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Isin? a, Isin? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Isin? a, Isin? b) => !(a == b);
    public int CompareTo(Isin? other) => other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);
    public static bool operator <(Isin left, Isin right) => left.CompareTo(right) < 0;
    public static bool operator >(Isin left, Isin right) => left.CompareTo(right) > 0;
    public static bool operator <=(Isin left, Isin right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Isin left, Isin right) => left.CompareTo(right) >= 0;
}
