using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// A 12-digit Global Trade Item Number (GTIN-12), also known as UPC-A (Universal Product Code).
/// The standard barcode format for retail products in North America. Validated with the GS1
/// modulo-10 check digit.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gs1.org/standards/id-keys/gtin">GS1 — GTIN</see></description></item>
/// <item><description><see href="https://www.gs1.org/services/check-digit-calculator">GS1 — Check digit calculator</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Universal_Product_Code">Wikipedia — Universal Product Code (UPC-A)</see></description></item>
/// </list>
/// </remarks>
public sealed class Gtin12 : IEquatable<Gtin12>, IComparable<Gtin12>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("GTIN-12", "GTIN-12", "📦", ["https://www.gs1.org/standards/id-keys/gtin", "https://www.gs1.org/services/check-digit-calculator", "https://en.wikipedia.org/wiki/Universal_Product_Code"]);

    /// <summary>The 12-digit GTIN (UPC-A), e.g. <c>614141000036</c>.</summary>
    public string Digits { get; }

    /// <summary>The GS1 modulo-10 check digit (last digit).</summary>
    public int CheckDigit { get; }

    private Gtin12(string digits)
    {
        Digits = digits;
        CheckDigit = digits[^1] - '0';
    }

    /// <summary>Returns the GTIN zero-padded to 14 digits.</summary>
    public string ToGtin14Digits() => Digits.PadLeft(14, '0');

    public static bool TryParse(string? input, out Gtin12? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = GtinCheckDigit.ExtractDigits(InputSanitization.SanitizeInput(input!));
        if (digits is null || digits.Length != 12) return false;
        if (!GtinCheckDigit.Validate(digits)) return false;

        result = new Gtin12(digits);
        return true;
    }

    public static Gtin12 Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid GTIN-12 / UPC-A.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the 12-digit GTIN, e.g. <c>614141000036</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Digits : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the 12-digit GTIN, e.g. <c>614141000036</c>.
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

    /// <summary>Returns the 12-digit GTIN, e.g. <c>614141000036</c>.</summary>
    public string ToNormalizedString() => Digits;

    /// <summary>Returns the 12-digit GTIN, e.g. <c>614141000036</c>.</summary>
    public override string ToString() => Digits;

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{12}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for 12-digit sequences that are valid GTIN-12 (UPC-A) codes.
    /// Validated with the GS1 check digit. This is heuristic-based.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Gtin12>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Gtin12>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var gtin)) continue;
            results.Add(new TextCandidate<Gtin12>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Gtin12), TextCandidateCategory.Product,
                gtin!.ToNormalizedString(), gtin.ToString(),
                gtin.ToMaskedString(),
                TextMatchConfidence.Medium,
                gtin));
        }
        return results;
    }

    public bool Equals(Gtin12? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is Gtin12 other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Gtin12? a, Gtin12? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Gtin12? a, Gtin12? b) => !(a == b);
    public int CompareTo(Gtin12? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Gtin12 left, Gtin12 right) => left.CompareTo(right) < 0;
    public static bool operator >(Gtin12 left, Gtin12 right) => left.CompareTo(right) > 0;
    public static bool operator <=(Gtin12 left, Gtin12 right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Gtin12 left, Gtin12 right) => left.CompareTo(right) >= 0;
}
