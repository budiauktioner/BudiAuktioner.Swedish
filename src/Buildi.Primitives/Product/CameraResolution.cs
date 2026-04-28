using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// A camera image-sensor resolution expressed in megapixels (<c>kamerapixel</c>),
/// e.g. <c>12 MP</c>, <c>108 megapixels</c>. Stored internally in megapixels.
/// </summary>
/// <remarks>
/// <para>Accepted suffixes (case-insensitive): <c>MP</c>, <c>Mpx</c>, <c>Mpix</c>,
/// <c>megapixel</c>, <c>megapixels</c>, <c>megapixlar</c>. Bare numbers and total
/// pixel counts (e.g. <c>12000000</c>) are not accepted in order to avoid ambiguity.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Pixel">Wikipedia — Pixel</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Image_resolution">Wikipedia — Image resolution</see></description></item>
/// </list>
/// </remarks>
public sealed class CameraResolution : IComparable<CameraResolution>, IEquatable<CameraResolution>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Camera Resolution",
        "Kameraupplösning",
        "📷",
        ["https://en.wikipedia.org/wiki/Pixel", "https://en.wikipedia.org/wiki/Image_resolution"]);

    private const decimal MaxMegapixels = 100_000m;

    private static readonly HashSet<string> RecognizedSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "MP", "Mp", "mp", "M", "Mpx", "MPx", "mpx", "Mpix", "mpix", "MPIX",
        "megapixel", "megapixels", "Megapixel", "Megapixels",
        "mpixel", "mpixels", "Mpixel", "Mpixels",
        "megapixlar", "Megapixlar"
    };

    private readonly decimal _megapixels;

    private CameraResolution(decimal megapixels)
    {
        _megapixels = megapixels;
    }

    /// <summary>Resolution in megapixels (millions of pixels), e.g. <c>12.2</c>.</summary>
    public decimal Megapixels => _megapixels;

    /// <summary>Total pixel count, e.g. <c>12200000</c> for 12.2 MP.</summary>
    public long TotalPixels => (long)Math.Round(_megapixels * 1_000_000m, MidpointRounding.AwayFromZero);

    /// <summary>Creates a <see cref="CameraResolution"/> from a megapixel value, e.g. <c>FromMegapixels(12.2m)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="megapixels"/> is negative or above the supported maximum.</exception>
    public static CameraResolution FromMegapixels(decimal megapixels)
    {
        if (megapixels < 0m) throw new ArgumentOutOfRangeException(nameof(megapixels), "Megapixels must be non-negative.");
        if (megapixels > MaxMegapixels) throw new ArgumentOutOfRangeException(nameof(megapixels), $"Megapixels must be <= {MaxMegapixels}.");
        return new CameraResolution(megapixels);
    }

    /// <summary>Creates a <see cref="CameraResolution"/> from a total pixel count, e.g. <c>FromTotalPixels(12_200_000)</c>.</summary>
    public static CameraResolution FromTotalPixels(long totalPixels)
    {
        if (totalPixels < 0L) throw new ArgumentOutOfRangeException(nameof(totalPixels), "Total pixels must be non-negative.");
        return FromMegapixels(totalPixels / 1_000_000m);
    }

    public static bool TryParse(string? input, out CameraResolution? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;
        if (value < 0m || value > MaxMegapixels) return false;
        if (!RecognizedSuffixes.Contains(unitSuffix)) return false;

        result = new CameraResolution(value);
        return true;
    }

    public static CameraResolution Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid camera resolution.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display string in megapixels, e.g. <c>12.2 MP</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
            return decimals is not null ? FormatWithDecimals(r._megapixels, decimals.Value) : r.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns the value in megapixels with the canonical <c>MP</c> suffix, e.g. <c>12.2 MP</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the value with canonical <c>MP</c> suffix, e.g. <c>12.2 MP</c>.</summary>
    public string ToNormalizedString() => $"{FormatDecimal(_megapixels)} MP";

    /// <summary>Returns the display form, e.g. <c>12.2 MP</c>.</summary>
    public override string ToString() => ToNormalizedString();

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    private static string FormatWithDecimals(decimal value, int decimals)
    {
        var rounded = Math.Round(value, decimals, MidpointRounding.AwayFromZero);
        var s = rounded.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return $"{s} MP";
    }

    public static bool operator ==(CameraResolution? a, CameraResolution? b) => a?._megapixels == b?._megapixels;
    public static bool operator !=(CameraResolution? a, CameraResolution? b) => !(a == b);
    public static bool operator <(CameraResolution a, CameraResolution b) => a._megapixels < b._megapixels;
    public static bool operator >(CameraResolution a, CameraResolution b) => a._megapixels > b._megapixels;
    public static bool operator <=(CameraResolution a, CameraResolution b) => a._megapixels <= b._megapixels;
    public static bool operator >=(CameraResolution a, CameraResolution b) => a._megapixels >= b._megapixels;

    public int CompareTo(CameraResolution? other) => other is null ? 1 : _megapixels.CompareTo(other._megapixels);
    public bool Equals(CameraResolution? other) => other is not null && _megapixels == other._megapixels;
    public override bool Equals(object? obj) => obj is CameraResolution other && Equals(other);
    public override int GetHashCode() => _megapixels.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)\d+[0-9 .,]*\s*(?:megapixels?|megapixlar|MPix|Mpix|MPx|Mpx|MP|Mp)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like camera resolution values
    /// (e.g. <c>12 MP</c>, <c>108 megapixels</c>) and returns successfully parsed candidates.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<CameraResolution>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<CameraResolution>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var resolution)) continue;
            results.Add(new TextCandidate<CameraResolution>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(CameraResolution), TextCandidateCategory.Product,
                resolution!.ToNormalizedString(), resolution.ToString(),
                resolution.ToMaskedString(),
                TextMatchConfidence.Medium,
                resolution));
        }
        return results;
    }
}
