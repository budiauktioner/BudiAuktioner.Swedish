using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Metric tire size notation (<c>däckdimension</c>) per ISO 4000-1, e.g. <c>205/55R16</c>
/// or <c>315/70R22.5 154/150L</c> for commercial truck tires
/// (section width, aspect ratio, construction, rim diameter) with optional load index, speed symbol,
/// dual load index for twin-mounted tires, and commercial <c>C</c> designation.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/standard/4600.html">ISO 4000-1 — Passenger car tyres and rims</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Tire_code">Wikipedia — Tire code</see></description></item>
/// </list>
/// </remarks>
public sealed class TireDimension : IEquatable<TireDimension>, IComparable<TireDimension>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Tire Dimension", "Däckdimension", "🛞", ["https://www.iso.org/standard/4600.html", "https://en.wikipedia.org/wiki/Tire_code"]);

    private const int MinWidthMm = 100;
    private const int MaxWidthMm = 400;
    private const int MinAspectRatio = 20;
    private const int MaxAspectRatio = 90;
    private const decimal MinRimInches = 10m;
    private const decimal MaxRimInches = 26.5m;
    private const int MinLoadIndex = 0;
    private const int MaxLoadIndex = 279;

    private static readonly Regex ParsePattern = new(
        @"^\s*(\d{3})/(\d{2})\s*([RDBrdb])\s*(\d{2}(?:[.,]\d)?)\s*(C)?\s*(?:[\(\s]*(\d{2,3})(?:/(\d{2,3}))?\s*([A-Za-z])?\s*\)?\s*)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)(\d{3})/(\d{2})\s*([RDBrdb])\s*(\d{2}(?:[.,]\d)?)C?(?:\s+(\d{2,3})(?:/(\d{2,3}))?\s*([A-Za-z])?)?(?!\d)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Tread width in millimetres, e.g. <c>205</c>.</summary>
    public int WidthMm { get; }

    /// <summary>Aspect ratio (sidewall height as a percentage of section width), e.g. <c>55</c>.</summary>
    public int AspectRatio { get; }

    /// <summary>Construction code: <c>R</c> radial, <c>D</c> diagonal, <c>B</c> bias belt.</summary>
    public char Construction { get; }

    /// <summary>Rim diameter in inches, e.g. <c>16</c> or <c>22.5</c> for half-inch commercial rims.</summary>
    public decimal RimDiameterInches { get; }

    /// <summary>Whether the tire has a commercial <c>C</c> designation for light truck / van use.</summary>
    public bool IsCommercial { get; }

    /// <summary>Optional load index, e.g. <c>91</c> in <c>205/55R16 91H</c>.</summary>
    public int? LoadIndex { get; }

    /// <summary>Optional dual (twin-mounted) load index, e.g. <c>150</c> in <c>315/70R22.5 154/150L</c>.</summary>
    public int? DualLoadIndex { get; }

    /// <summary>Optional speed rating letter, e.g. <c>H</c> in <c>205/55R16 91H</c>.</summary>
    public char? SpeedRating { get; }

    /// <summary>Normalized compact form, e.g. <c>205/55R16</c>, <c>205/55R16 91H</c>, or <c>315/70R22.5 154/150L</c>.</summary>
    public string Value { get; }

    private TireDimension(
        int widthMm,
        int aspectRatio,
        char construction,
        decimal rimDiameterInches,
        bool isCommercial,
        int? loadIndex,
        int? dualLoadIndex,
        char? speedRating,
        string value)
    {
        WidthMm = widthMm;
        AspectRatio = aspectRatio;
        Construction = construction;
        RimDiameterInches = rimDiameterInches;
        IsCommercial = isCommercial;
        LoadIndex = loadIndex;
        DualLoadIndex = dualLoadIndex;
        SpeedRating = speedRating;
        Value = value;
    }

    public static bool TryParse(string? input, out TireDimension? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var match = ParsePattern.Match(trimmed);
        if (!match.Success) return false;

        var width = int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        var aspect = int.Parse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        var construction = char.ToUpperInvariant(match.Groups[3].Value[0]);
        var rimText = match.Groups[4].Value.Replace(',', '.');
        var rim = decimal.Parse(rimText, NumberStyles.Number, CultureInfo.InvariantCulture);

        if (width is < MinWidthMm or > MaxWidthMm) return false;
        if (aspect is < MinAspectRatio or > MaxAspectRatio) return false;
        if (rim < MinRimInches || rim > MaxRimInches) return false;

        var isCommercial = match.Groups[5].Success;

        int? loadIndex = null;
        int? dualLoadIndex = null;
        char? speedRating = null;
        if (match.Groups[6].Success)
        {
            var load = int.Parse(match.Groups[6].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (load is < MinLoadIndex or > MaxLoadIndex) return false;
            loadIndex = load;

            if (match.Groups[7].Success)
            {
                var dual = int.Parse(match.Groups[7].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                if (dual is < MinLoadIndex or > MaxLoadIndex) return false;
                dualLoadIndex = dual;
            }

            if (match.Groups[8].Success)
                speedRating = char.ToUpperInvariant(match.Groups[8].Value[0]);
        }

        var value = BuildNormalized(width, aspect, construction, rim, isCommercial, loadIndex, dualLoadIndex, speedRating);
        result = new TireDimension(width, aspect, construction, rim, isCommercial, loadIndex, dualLoadIndex, speedRating, value);
        return true;
    }

    public static TireDimension Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid tire dimension.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form with spaces around the construction letter, e.g. <c>205/55 R 16</c>,
    /// <c>205/55 R 16 91H</c>, or <c>315/70 R 22.5 154/150L</c> for truck tires with dual load index.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var d) && d is not null
            ? BuildFormatted(d.WidthMm, d.AspectRatio, d.Construction, d.RimDiameterInches, d.IsCommercial, d.LoadIndex, d.DualLoadIndex, d.SpeedRating)
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input)
                ? input!.Trim()
                : null;

    /// <summary>
    /// Returns normalized compact form, e.g. <c>205/55R16</c>, <c>205/55R16 91H</c>,
    /// or <c>315/70R22.5 154/150L</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var d)) return d?.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already identical to its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input.Trim()) == input.Trim();

    /// <summary>Returns normalized compact form, e.g. <c>205/55R16</c> or <c>315/70R22.5 154/150L</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns display form with spaces, e.g. <c>205/55 R 16</c> or <c>315/70 R 22.5 154/150L</c>.</summary>
    public override string ToString() =>
        BuildFormatted(WidthMm, AspectRatio, Construction, RimDiameterInches, IsCommercial, LoadIndex, DualLoadIndex, SpeedRating);

    /// <summary>
    /// Scans unstructured text for substrings that look like ISO metric tire dimensions.
    /// The pattern is distinctive but scanning remains heuristic-based and may still produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<TireDimension>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<TireDimension>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var dim)) continue;
            results.Add(new TextCandidate<TireDimension>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(TireDimension),
                TextCandidateCategory.Vehicle,
                dim!.ToNormalizedString(),
                dim.ToString(),
                dim.ToMaskedString(),
                TextMatchConfidence.High,
                dim));
        }

        return results;
    }

    private static string FormatRimDiameter(decimal rim) =>
        rim % 1 == 0
            ? FormattableString.Invariant($"{(int)rim:D2}")
            : rim.ToString("0.0", CultureInfo.InvariantCulture);

    private static string BuildNormalized(
        int width,
        int aspect,
        char construction,
        decimal rim,
        bool isCommercial,
        int? loadIndex,
        int? dualLoadIndex,
        char? speedRating)
    {
        var rimStr = FormatRimDiameter(rim);
        var commercial = isCommercial ? "C" : "";
        var core = FormattableString.Invariant($"{width}/{aspect}{construction}{rimStr}{commercial}");
        if (!loadIndex.HasValue) return core;
        var load = dualLoadIndex.HasValue
            ? FormattableString.Invariant($"{loadIndex}/{dualLoadIndex}")
            : FormattableString.Invariant($"{loadIndex}");
        return speedRating.HasValue
            ? $"{core} {load}{speedRating}"
            : $"{core} {load}";
    }

    private static string BuildFormatted(
        int width,
        int aspect,
        char construction,
        decimal rim,
        bool isCommercial,
        int? loadIndex,
        int? dualLoadIndex,
        char? speedRating)
    {
        var rimStr = FormatRimDiameter(rim);
        var commercial = isCommercial ? " C" : "";
        var core = FormattableString.Invariant($"{width}/{aspect} {construction} {rimStr}{commercial}");
        if (!loadIndex.HasValue) return core;
        var load = dualLoadIndex.HasValue
            ? FormattableString.Invariant($"{loadIndex}/{dualLoadIndex}")
            : FormattableString.Invariant($"{loadIndex}");
        return speedRating.HasValue
            ? $"{core} {load}{speedRating}"
            : $"{core} {load}";
    }

    public static bool operator ==(TireDimension? a, TireDimension? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(TireDimension? a, TireDimension? b) => !(a == b);
    public int CompareTo(TireDimension? other)
    {
        if (other is null) return 1;
        var c = WidthMm.CompareTo(other.WidthMm);
        if (c != 0) return c;
        c = AspectRatio.CompareTo(other.AspectRatio);
        if (c != 0) return c;
        c = RimDiameterInches.CompareTo(other.RimDiameterInches);
        if (c != 0) return c;
        c = Construction.CompareTo(other.Construction);
        if (c != 0) return c;
        c = IsCommercial.CompareTo(other.IsCommercial);
        if (c != 0) return c;
        c = (LoadIndex ?? -1).CompareTo(other.LoadIndex ?? -1);
        if (c != 0) return c;
        c = (DualLoadIndex ?? -1).CompareTo(other.DualLoadIndex ?? -1);
        if (c != 0) return c;
        return (SpeedRating ?? '\0').CompareTo(other.SpeedRating ?? '\0');
    }
    public static bool operator <(TireDimension left, TireDimension right) => left.CompareTo(right) < 0;
    public static bool operator >(TireDimension left, TireDimension right) => left.CompareTo(right) > 0;
    public static bool operator <=(TireDimension left, TireDimension right) => left.CompareTo(right) <= 0;
    public static bool operator >=(TireDimension left, TireDimension right) => left.CompareTo(right) >= 0;

    public bool Equals(TireDimension? other) =>
        other is not null && WidthMm == other.WidthMm && AspectRatio == other.AspectRatio &&
        Construction == other.Construction && RimDiameterInches == other.RimDiameterInches;
    public override bool Equals(object? obj) => obj is TireDimension other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(WidthMm, AspectRatio, Construction, RimDiameterInches);
}
