using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Organization;

/// <summary>
/// A D-U-N-S number is a global business identifier commonly used in company master data and organization classification. This type stores the 9-digit identifier in normalized form.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.dnb.com/duns.html">Dun &amp; Bradstreet — D-U-N-S Number</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Data_Universal_Numbering_System">Wikipedia — Data Universal Numbering System</see></description></item>
/// </list>
/// </remarks>
public sealed class DunsNumber : IEquatable<DunsNumber>, IComparable<DunsNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("D-U-N-S Number", "D-U-N-S-nummer", "🔢", ["https://www.dnb.com/duns.html", "https://en.wikipedia.org/wiki/Data_Universal_Numbering_System"]);

    private const int MaxInputLength = 20;

    public string Digits { get; }

    private DunsNumber(string digits)
    {
        Digits = digits;
    }

    public static bool TryParse(string? input, out DunsNumber? duns)
    {
        duns = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }
        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length > MaxInputLength) return false;
        if (digits.Length != 9)
        {
            return false;
        }
        duns = new DunsNumber(digits);
        return true;
    }

    public static DunsNumber Parse(string input)
    {
        if (!TryParse(input, out var duns))
        {
            throw new ArgumentException("Invalid D-U-N-S number.", nameof(input));
        }
        return duns!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the D-U-N-S number as 9 digits, for example <c>123456789</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Digits : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized D-U-N-S number as 9 digits, for example <c>123456789</c>.
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

    /// <summary>
    /// Returns the normalized D-U-N-S number as 9 digits, for example <c>123456789</c>.
    /// </summary>
    public string ToNormalizedString() => Digits;
    /// <summary>
    /// Returns the D-U-N-S number as 9 digits, for example <c>123456789</c>.
    /// </summary>
    public override string ToString() => Digits;

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{9}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential D-U-N-S numbers (9-digit sequences).
    /// Results are heuristic-based candidates and have a high false-positive rate since
    /// D-U-N-S numbers have no checksum. No guarantee is made that a candidate represents
    /// a real D-U-N-S number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<DunsNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<DunsNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var duns)) continue;
            results.Add(new TextCandidate<DunsNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(DunsNumber),
                TextCandidateCategory.OrganizationIdentifier,
                duns!.ToNormalizedString(),
                duns.ToString(),
                duns.ToMaskedString(),
                TextMatchConfidence.Low,
                duns));
        }
        return results;
    }

    public bool Equals(DunsNumber? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is DunsNumber other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(DunsNumber? a, DunsNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(DunsNumber? a, DunsNumber? b) => !(a == b);
    public int CompareTo(DunsNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(DunsNumber left, DunsNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(DunsNumber left, DunsNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(DunsNumber left, DunsNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(DunsNumber left, DunsNumber right) => left.CompareTo(right) >= 0;
}
