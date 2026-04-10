using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Wheel rim/wheel size notation (<c>fälgdimension</c>), e.g. <c>18x7J</c>.
/// Specifies rim diameter in inches, width in inches, and optional flange type.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Wheel_sizing">Wikipedia — Wheel sizing</see></description></item>
/// <item><description><see href="https://www.etrto.org/">ETRTO — European Tyre and Rim Technical Organisation</see></description></item>
/// </list>
/// </remarks>
public sealed class WheelRimDimension : IEquatable<WheelRimDimension>, IComparable<WheelRimDimension>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Wheel Rim Dimension", "Fälgdimension", "⭕", ["https://en.wikipedia.org/wiki/Wheel_sizing", "https://www.etrto.org/"]);

    private const decimal MinDiameter = 10m;
    private const decimal MaxDiameter = 26m;
    private const decimal MinWidth = 3m;
    private const decimal MaxWidth = 16m;

    private static readonly Regex NormalPattern = new(
        @"^\s*(\d{2}(?:[.,]\d{1,2})?)\s*[xX×]\s*(\d{1,2}(?:[.,]\d{1,2})?)\s*(JJ|JK|J|B|K|P|D)?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex ReversedPattern = new(
        @"^\s*(\d{1,2}(?:[.,]\d{1,2})?)\s*(JJ|JK|J|B|K|P|D)?\s*[xX×]\s*(\d{2}(?:[.,]\d{1,2})?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <summary>Normalized compact form, e.g. <c>18x7J</c>.</summary>
    public string Value { get; }

    /// <summary>Rim diameter in inches, e.g. <c>18</c>.</summary>
    public decimal DiameterInches { get; }

    /// <summary>Rim width in inches, e.g. <c>7</c>.</summary>
    public decimal WidthInches { get; }

    /// <summary>Flange type code, e.g. <c>J</c>, <c>JJ</c>, or empty when not specified.</summary>
    public string FlangeType { get; }

    private WheelRimDimension(decimal diameter, decimal width, string flangeType, string value)
    {
        DiameterInches = diameter;
        WidthInches = width;
        FlangeType = flangeType;
        Value = value;
    }

    public static bool TryParse(string? input, out WheelRimDimension? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        decimal diameter, width;
        string flange;

        var match = NormalPattern.Match(trimmed);
        if (match.Success)
        {
            if (!TryParseDecimal(match.Groups[1].Value, out diameter)) return false;
            if (!TryParseDecimal(match.Groups[2].Value, out width)) return false;
            flange = match.Groups[3].Success ? match.Groups[3].Value.ToUpperInvariant() : "";
        }
        else
        {
            match = ReversedPattern.Match(trimmed);
            if (!match.Success) return false;

            if (!TryParseDecimal(match.Groups[1].Value, out width)) return false;
            flange = match.Groups[2].Success ? match.Groups[2].Value.ToUpperInvariant() : "";
            if (!TryParseDecimal(match.Groups[3].Value, out diameter)) return false;
        }

        if (diameter < MinDiameter || diameter > MaxDiameter) return false;
        if (width < MinWidth || width > MaxWidth) return false;

        var value = BuildNormalized(diameter, width, flange);
        result = new WheelRimDimension(diameter, width, flange, value);
        return true;
    }

    public static WheelRimDimension Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid wheel rim dimension.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns spaced display form, e.g. <c>18 x 7 J</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var d) && d is not null
            ? d.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input)
                ? input!.Trim()
                : null;

    /// <summary>
    /// Returns normalized compact form, e.g. <c>18x7J</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input (empty strings become <see langword="null"/>).
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
        input is not null && Normalize(input) == input;

    /// <summary>Returns normalized compact form, e.g. <c>18x7J</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns spaced display form, e.g. <c>18 x 7 J</c>.</summary>
    public override string ToString() => BuildFormatted(DiameterInches, WidthInches, FlangeType);

    private static string BuildNormalized(decimal diameter, decimal width, string flange)
    {
        var d = FormatDecimal(diameter);
        var w = FormatDecimal(width);
        return flange.Length > 0 ? $"{d}x{w}{flange}" : $"{d}x{w}";
    }

    private static string BuildFormatted(decimal diameter, decimal width, string flange)
    {
        var d = FormatDecimal(diameter);
        var w = FormatDecimal(width);
        return flange.Length > 0 ? $"{d} x {w} {flange}" : $"{d} x {w}";
    }

    private static bool TryParseDecimal(string s, out decimal value)
    {
        value = 0;
        return decimal.TryParse(s.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
    }

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)\d{1,2}(?:[.,]\d{1,2})?\s*(?:JJ|JK|J|B|K|P|D)?\s*[xX×]\s*\d{1,2}(?:[.,]\d{1,2})?\s*(?:JJ|JK|J|B|K|P|D)?(?!\w)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like wheel rim dimensions (e.g. <c>18x7J</c>, <c>7.5Jx17</c>).
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<WheelRimDimension>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<WheelRimDimension>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var dim)) continue;
            results.Add(new TextCandidate<WheelRimDimension>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(WheelRimDimension), TextCandidateCategory.Vehicle,
                dim!.ToNormalizedString(), dim.ToString(),
                dim.ToMaskedString(),
                TextMatchConfidence.Medium,
                dim));
        }
        return results;
    }

    public static bool operator ==(WheelRimDimension? a, WheelRimDimension? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(WheelRimDimension? a, WheelRimDimension? b) => !(a == b);

    public int CompareTo(WheelRimDimension? other)
    {
        if (other is null) return 1;
        var c = DiameterInches.CompareTo(other.DiameterInches);
        if (c != 0) return c;
        c = WidthInches.CompareTo(other.WidthInches);
        if (c != 0) return c;
        return string.Compare(FlangeType, other.FlangeType, StringComparison.Ordinal);
    }

    public static bool operator <(WheelRimDimension left, WheelRimDimension right) => left.CompareTo(right) < 0;
    public static bool operator >(WheelRimDimension left, WheelRimDimension right) => left.CompareTo(right) > 0;
    public static bool operator <=(WheelRimDimension left, WheelRimDimension right) => left.CompareTo(right) <= 0;
    public static bool operator >=(WheelRimDimension left, WheelRimDimension right) => left.CompareTo(right) >= 0;

    public bool Equals(WheelRimDimension? other) =>
        other is not null && DiameterInches == other.DiameterInches &&
        WidthInches == other.WidthInches && FlangeType == other.FlangeType;

    public override bool Equals(object? obj) => obj is WheelRimDimension other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(DiameterInches, WidthInches, FlangeType);
}
