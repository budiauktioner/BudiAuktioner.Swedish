using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// A 13-digit Global Trade Item Number (GTIN-13), also known as EAN-13 or International Article Number.
/// The most widely used barcode format for consumer products worldwide. The first 1–3 digits form
/// the GS1 prefix identifying the issuing GS1 member organization. Validated with the GS1 modulo-10
/// check digit.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gs1.org/standards/id-keys/gtin">GS1 — GTIN</see></description></item>
/// <item><description><see href="https://www.gs1.org/services/check-digit-calculator">GS1 — Check digit calculator</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/International_Article_Number">Wikipedia — International Article Number (EAN-13)</see></description></item>
/// </list>
/// </remarks>
public sealed class Gtin13 : IEquatable<Gtin13>, IComparable<Gtin13>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("GTIN-13", "GTIN-13", "📦", ["https://www.gs1.org/standards/id-keys/gtin", "https://www.gs1.org/services/check-digit-calculator", "https://en.wikipedia.org/wiki/International_Article_Number"]);

    /// <summary>The 13-digit GTIN, e.g. <c>5901234123457</c>.</summary>
    public string Digits { get; }

    /// <summary>The GS1 modulo-10 check digit (last digit).</summary>
    public int CheckDigit { get; }

    /// <summary>The 3-digit GS1 prefix identifying the issuing GS1 member organization, e.g. <c>590</c> for Poland.</summary>
    public string Gs1Prefix { get; }

    /// <summary>
    /// The name of the GS1 member organization or country associated with the prefix,
    /// e.g. <c>Sweden</c> for prefix 730–739, or <c>Books (ISBN)</c> for prefix 978–979.
    /// <see langword="null"/> when the prefix is not in a known range.
    /// Based on the <see href="https://www.gs1.org/standards/id-keys/company-prefix">GS1 prefix list</see>.
    /// </summary>
    public string? Gs1PrefixName { get; }

    /// <summary>
    /// The ISO 3166-1 alpha-2 country code associated with the GS1 prefix, e.g. <c>SE</c> for
    /// prefix 730–739. <see langword="null"/> when the prefix maps to a non-country entity
    /// (e.g. ISBN, coupons, restricted distribution) or is not in a known range.
    /// </summary>
    public string? Gs1PrefixCountryCode { get; }

    private Gtin13(string digits)
    {
        Digits = digits;
        CheckDigit = digits[^1] - '0';
        Gs1Prefix = digits[..3];
        var resolved = Gs1PrefixResolver.Resolve(Gs1Prefix);
        Gs1PrefixName = resolved?.Name;
        Gs1PrefixCountryCode = resolved?.CountryCode;
    }

    /// <summary>Returns the GTIN zero-padded to 14 digits.</summary>
    public string ToGtin14Digits() => Digits.PadLeft(14, '0');

    public static bool TryParse(string? input, out Gtin13? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = GtinCheckDigit.ExtractDigits(InputSanitization.SanitizeInput(input!));
        if (digits is null || digits.Length != 13) return false;
        if (!GtinCheckDigit.Validate(digits)) return false;

        result = new Gtin13(digits);
        return true;
    }

    public static Gtin13 Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid GTIN-13.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the 13-digit GTIN, e.g. <c>5901234123457</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Digits : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the 13-digit GTIN, e.g. <c>5901234123457</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Digits;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the 13-digit GTIN, e.g. <c>5901234123457</c>.</summary>
    public string ToNormalizedString() => Digits;

    /// <summary>Returns the 13-digit GTIN, e.g. <c>5901234123457</c>.</summary>
    public override string ToString() => Digits;

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{13}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential GTIN-13 / EAN-13 barcodes.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real product code in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Gtin13>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Gtin13>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var gtin)) continue;
            results.Add(new TextCandidate<Gtin13>(
                match.Index,
                match.Length,
                match.Value,
                nameof(Gtin13),
                TextCandidateCategory.Product,
                gtin!.ToNormalizedString(),
                gtin.ToString(),
                gtin.ToMaskedString(),
                TextMatchConfidence.High,
                gtin));
        }
        return results;
    }

    public bool Equals(Gtin13? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is Gtin13 other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Gtin13? a, Gtin13? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Gtin13? a, Gtin13? b) => !(a == b);
    public int CompareTo(Gtin13? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Gtin13 left, Gtin13 right) => left.CompareTo(right) < 0;
    public static bool operator >(Gtin13 left, Gtin13 right) => left.CompareTo(right) > 0;
    public static bool operator <=(Gtin13 left, Gtin13 right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Gtin13 left, Gtin13 right) => left.CompareTo(right) >= 0;
}
