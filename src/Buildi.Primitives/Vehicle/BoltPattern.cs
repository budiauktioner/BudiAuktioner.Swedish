using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Wheel bolt pattern (<c>bultcirkel</c> / <c>bultcirkelmått</c>), e.g. <c>5x114.3</c>.
/// Describes the number of bolt holes and the pitch circle diameter (PCD) in millimeters.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Bolt_pattern">Wikipedia — Bolt pattern</see></description></item>
/// </list>
/// </remarks>
public sealed class BoltPattern : IEquatable<BoltPattern>, IComparable<BoltPattern>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Bolt Pattern", "Bultcirkelmått", "🔩",
        ["https://en.wikipedia.org/wiki/Bolt_pattern"]);

    private const int MinBoltCount = 3;
    private const int MaxBoltCount = 10;
    private const decimal MinPcd = 50m;
    private const decimal MaxPcd = 250m;

    private static readonly Regex ParsePattern = new(
        @"^\s*(\d{1,2})\s*[xX×]\s*(\d{2,3}(?:[.,]\d{1,2})?)\s*(?:mm)?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Number of bolt holes, e.g. <c>5</c>.</summary>
    public int BoltCount { get; }

    /// <summary>Pitch circle diameter in millimeters, e.g. <c>114.3</c>.</summary>
    public decimal PitchCircleDiameter { get; }

    /// <summary>Normalized compact form, e.g. <c>5x114.3</c>.</summary>
    public string Value { get; }

    private BoltPattern(int boltCount, decimal pitchCircleDiameter, string value)
    {
        BoltCount = boltCount;
        PitchCircleDiameter = pitchCircleDiameter;
        Value = value;
    }

    public static bool TryParse(string? input, out BoltPattern? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var match = ParsePattern.Match(trimmed);
        if (!match.Success) return false;

        var boltCount = int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        var pcdText = match.Groups[2].Value.Replace(',', '.');
        var pcd = decimal.Parse(pcdText, NumberStyles.Number, CultureInfo.InvariantCulture);

        if (boltCount is < MinBoltCount or > MaxBoltCount) return false;
        if (pcd < MinPcd || pcd > MaxPcd) return false;

        var value = BuildNormalized(boltCount, pcd);
        result = new BoltPattern(boltCount, pcd, value);
        return true;
    }

    public static BoltPattern Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid bolt pattern.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form with spaces around the separator, e.g. <c>5 x 114.3</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns normalized compact form, e.g. <c>5x114.3</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input) == input;

    /// <summary>Returns normalized compact form, e.g. <c>5x114.3</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns display form with spaces, e.g. <c>5 x 114.3</c>.</summary>
    public override string ToString() => $"{BoltCount} x {FormatPcd(PitchCircleDiameter)}";

    private static string BuildNormalized(int boltCount, decimal pcd) =>
        $"{boltCount}x{FormatPcd(pcd)}";

    private static string FormatPcd(decimal pcd)
    {
        if (pcd % 1 == 0)
            return ((int)pcd).ToString(CultureInfo.InvariantCulture);
        var s = pcd.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)(\d{1,2})\s*[xX×]\s*(\d{2,3}(?:[.,]\d{1,2})?)(?!\d)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Scans unstructured text for substrings that look like wheel bolt patterns (e.g. <c>5x114.3</c>).
    /// The pattern is fairly distinctive but scanning remains heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<BoltPattern>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<BoltPattern>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var bp)) continue;
            results.Add(new TextCandidate<BoltPattern>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(BoltPattern), TextCandidateCategory.Vehicle,
                bp!.ToNormalizedString(), bp.ToString(),
                bp.ToMaskedString(),
                TextMatchConfidence.Medium,
                bp));
        }
        return results;
    }

    public static bool operator ==(BoltPattern? a, BoltPattern? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(BoltPattern? a, BoltPattern? b) => !(a == b);

    public int CompareTo(BoltPattern? other)
    {
        if (other is null) return 1;
        var c = BoltCount.CompareTo(other.BoltCount);
        return c != 0 ? c : PitchCircleDiameter.CompareTo(other.PitchCircleDiameter);
    }

    public static bool operator <(BoltPattern left, BoltPattern right) => left.CompareTo(right) < 0;
    public static bool operator >(BoltPattern left, BoltPattern right) => left.CompareTo(right) > 0;
    public static bool operator <=(BoltPattern left, BoltPattern right) => left.CompareTo(right) <= 0;
    public static bool operator >=(BoltPattern left, BoltPattern right) => left.CompareTo(right) >= 0;

    public bool Equals(BoltPattern? other) =>
        other is not null && BoltCount == other.BoltCount && PitchCircleDiameter == other.PitchCircleDiameter;
    public override bool Equals(object? obj) => obj is BoltPattern other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(BoltCount, PitchCircleDiameter);
}
