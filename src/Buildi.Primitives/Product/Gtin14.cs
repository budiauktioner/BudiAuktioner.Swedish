using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// A 14-digit Global Trade Item Number (GTIN-14), also known as ITF-14. Used to identify trade items
/// at various packaging levels (case, pallet, etc.). The first digit is the indicator digit specifying
/// the packaging level (1–8 for defined levels, 9 for variable-measure items). Validated with the
/// GS1 modulo-10 check digit.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gs1.org/standards/id-keys/gtin">GS1 — GTIN</see></description></item>
/// <item><description><see href="https://www.gs1.org/services/check-digit-calculator">GS1 — Check digit calculator</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/ITF-14">Wikipedia — ITF-14</see></description></item>
/// </list>
/// </remarks>
public sealed class Gtin14 : IEquatable<Gtin14>, IComparable<Gtin14>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("GTIN-14", "GTIN-14", "📦", ["https://www.gs1.org/standards/id-keys/gtin", "https://www.gs1.org/services/check-digit-calculator", "https://en.wikipedia.org/wiki/ITF-14"]);

    /// <summary>The 14-digit GTIN, e.g. <c>10614141000415</c>.</summary>
    public string Digits { get; }

    /// <summary>The GS1 modulo-10 check digit (last digit).</summary>
    public int CheckDigit { get; }

    /// <summary>
    /// The packaging indicator digit (first digit). Values 1–8 indicate defined packaging levels,
    /// 9 indicates a variable-measure trade item, and 0 indicates no specific packaging level.
    /// </summary>
    public int IndicatorDigit { get; }

    /// <summary>The inner 13 digits (positions 2–14), representing the contained GTIN-13.</summary>
    public string InnerGtin13Digits { get; }

    private Gtin14(string digits)
    {
        Digits = digits;
        CheckDigit = digits[^1] - '0';
        IndicatorDigit = digits[0] - '0';
        InnerGtin13Digits = digits[1..];
    }

    public static bool TryParse(string? input, out Gtin14? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = GtinCheckDigit.ExtractDigits(InputSanitization.SanitizeInput(input!));
        if (digits is null || digits.Length != 14) return false;
        if (!GtinCheckDigit.Validate(digits)) return false;

        result = new Gtin14(digits);
        return true;
    }

    public static Gtin14 Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid GTIN-14.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the 14-digit GTIN, e.g. <c>10614141000415</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Digits : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the 14-digit GTIN, e.g. <c>10614141000415</c>.
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

    /// <summary>Returns the 14-digit GTIN, e.g. <c>10614141000415</c>.</summary>
    public string ToNormalizedString() => Digits;

    /// <summary>Returns the 14-digit GTIN, e.g. <c>10614141000415</c>.</summary>
    public override string ToString() => Digits;

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{14}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for 14-digit sequences that are valid GTIN-14 codes.
    /// Validated with the GS1 check digit. This is heuristic-based.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Gtin14>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Gtin14>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var gtin)) continue;
            results.Add(new TextCandidate<Gtin14>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Gtin14), TextCandidateCategory.Product,
                gtin!.ToNormalizedString(), gtin.ToString(),
                gtin.ToMaskedString(),
                TextMatchConfidence.Medium,
                gtin));
        }
        return results;
    }

    public bool Equals(Gtin14? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is Gtin14 other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Gtin14? a, Gtin14? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Gtin14? a, Gtin14? b) => !(a == b);
    public int CompareTo(Gtin14? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Gtin14 left, Gtin14 right) => left.CompareTo(right) < 0;
    public static bool operator >(Gtin14 left, Gtin14 right) => left.CompareTo(right) > 0;
    public static bool operator <=(Gtin14 left, Gtin14 right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Gtin14 left, Gtin14 right) => left.CompareTo(right) >= 0;
}
