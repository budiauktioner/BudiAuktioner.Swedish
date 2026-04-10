using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// An 8-digit Global Trade Item Number (GTIN-8), formerly known as EAN-8. Used for small consumer
/// packages where a full GTIN-13 barcode would not fit. Validated with the GS1 modulo-10 check digit.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gs1.org/standards/id-keys/gtin">GS1 — GTIN</see></description></item>
/// <item><description><see href="https://www.gs1.org/services/check-digit-calculator">GS1 — Check digit calculator</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/EAN-8">Wikipedia — EAN-8</see></description></item>
/// </list>
/// </remarks>
public sealed class Gtin8 : IEquatable<Gtin8>, IComparable<Gtin8>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("GTIN-8", "GTIN-8", "📦", ["https://www.gs1.org/standards/id-keys/gtin", "https://www.gs1.org/services/check-digit-calculator", "https://en.wikipedia.org/wiki/EAN-8"]);

    /// <summary>The 8-digit GTIN, e.g. <c>96385074</c>.</summary>
    public string Digits { get; }

    /// <summary>The GS1 modulo-10 check digit (last digit).</summary>
    public int CheckDigit { get; }

    private Gtin8(string digits)
    {
        Digits = digits;
        CheckDigit = digits[^1] - '0';
    }

    /// <summary>Returns the GTIN zero-padded to 14 digits.</summary>
    public string ToGtin14Digits() => Digits.PadLeft(14, '0');

    public static bool TryParse(string? input, out Gtin8? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = GtinCheckDigit.ExtractDigits(InputSanitization.SanitizeInput(input!));
        if (digits is null || digits.Length != 8) return false;
        if (!GtinCheckDigit.Validate(digits)) return false;

        result = new Gtin8(digits);
        return true;
    }

    public static Gtin8 Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid GTIN-8.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the 8-digit GTIN, e.g. <c>96385074</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Digits : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the 8-digit GTIN, e.g. <c>96385074</c>.
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

    /// <summary>Returns the 8-digit GTIN, e.g. <c>96385074</c>.</summary>
    public string ToNormalizedString() => Digits;

    /// <summary>Returns the 8-digit GTIN, e.g. <c>96385074</c>.</summary>
    public override string ToString() => Digits;

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{8}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential GTIN-8 / EAN-8 barcodes.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real product code in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Gtin8>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Gtin8>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var gtin)) continue;
            results.Add(new TextCandidate<Gtin8>(
                match.Index,
                match.Length,
                match.Value,
                nameof(Gtin8),
                TextCandidateCategory.Product,
                gtin!.ToNormalizedString(),
                gtin.ToString(),
                gtin.ToMaskedString(),
                TextMatchConfidence.High,
                gtin));
        }
        return results;
    }

    public bool Equals(Gtin8? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is Gtin8 other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Gtin8? a, Gtin8? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Gtin8? a, Gtin8? b) => !(a == b);
    public int CompareTo(Gtin8? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Gtin8 left, Gtin8 right) => left.CompareTo(right) < 0;
    public static bool operator >(Gtin8 left, Gtin8 right) => left.CompareTo(right) > 0;
    public static bool operator <=(Gtin8 left, Gtin8 right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Gtin8 left, Gtin8 right) => left.CompareTo(right) >= 0;
}
