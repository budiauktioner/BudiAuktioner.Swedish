using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Machine operating hours (<c>drifttimmar</c>).
/// Bare numbers are interpreted as hours. Also accepts explicit hour-unit suffixes
/// (<c>h</c>, <c>tim</c>, <c>timmar</c>).
/// </summary>
public sealed class OperatingHours : IEquatable<OperatingHours>, IComparable<OperatingHours>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Operating Hours", "Drifttimmar", "⏱️", []);

    /// <summary>Display form preserving the original input style, e.g. <c>1234 h</c>.</summary>
    public string Value { get; }

    /// <summary>Reading in hours.</summary>
    public decimal Hours { get; }

    private OperatingHours(decimal hours, string displayValue)
    {
        Hours = hours;
        Value = displayValue;
    }

    /// <summary>Creates an <see cref="OperatingHours"/> from a decimal hour value. Must be non-negative.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="hours"/> is negative.</exception>
    public static OperatingHours Create(decimal hours)
    {
        if (hours < 0) throw new ArgumentOutOfRangeException(nameof(hours), "Operating hours cannot be negative.");
        return new OperatingHours(hours, $"{FormatDecimal(hours)} h");
    }

    /// <summary>Creates an <see cref="OperatingHours"/> from an integer hour value.</summary>
    public static OperatingHours Create(int hours) => Create((decimal)hours);

    /// <summary>Creates an <see cref="OperatingHours"/> from hours, e.g. <c>FromHours(1234)</c>.</summary>
    public static OperatingHours FromHours(decimal hours) => Create(hours);

    /// <summary>Creates an <see cref="OperatingHours"/> from hours, e.g. <c>FromHours(1234)</c>.</summary>
    public static OperatingHours FromHours(int hours) => Create(hours);

    private static readonly string[] HourSuffixes = ["h", "tim", "timmar", "timme", "hours", "hour", "hrs", "hr"];

    public static bool TryParse(string? input, out OperatingHours? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (Measurement.MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare < 0) return false;
            result = new OperatingHours(bare, $"{FormatDecimal(bare)} h");
            return true;
        }

        if (!Measurement.MeasurementUnitParser.TrySplit(trimmed, out var value, out var unitSuffix))
            return false;

        if (value < 0) return false;

        var isHourUnit = false;
        foreach (var suffix in HourSuffixes)
        {
            if (string.Equals(unitSuffix, suffix, StringComparison.OrdinalIgnoreCase))
            {
                isHourUnit = true;
                break;
            }
        }
        if (!isHourUnit) return false;

        result = new OperatingHours(value, $"{FormatDecimal(value)} h");
        return true;
    }

    public static OperatingHours Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid operating hours.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form, e.g. <c>1234 h</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>Returns the value in hours, e.g. <c>1234 h</c>.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the reading in hours, e.g. <c>1234 h</c>.</summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(Hours);
        return $"{formatted} h";
    }

    public override string ToString() => Value;

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static OperatingHours operator +(OperatingHours a, OperatingHours b) =>
        new(a.Hours + b.Hours, $"{FormatDecimal(a.Hours + b.Hours)} h");
    public static OperatingHours operator -(OperatingHours a, OperatingHours b) =>
        new(a.Hours - b.Hours, $"{FormatDecimal(a.Hours - b.Hours)} h");
    public static OperatingHours operator *(OperatingHours a, decimal factor) =>
        new(a.Hours * factor, $"{FormatDecimal(a.Hours * factor)} h");
    public static OperatingHours operator *(decimal factor, OperatingHours a) =>
        new(a.Hours * factor, $"{FormatDecimal(a.Hours * factor)} h");
    public static OperatingHours operator /(OperatingHours a, decimal divisor) =>
        new(a.Hours / divisor, $"{FormatDecimal(a.Hours / divisor)} h");
    public static OperatingHours operator -(OperatingHours a) =>
        new(-a.Hours, $"{FormatDecimal(-a.Hours)} h");

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:\d+[0-9 .,]*\d|\d)\s*(?:timmar|timme|tim|hours|hour|hrs|hr|h)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like operating hour values and returns
    /// successfully parsed candidates. Only matches values with explicit hour suffixes to avoid
    /// false positives. This is heuristic-based and may still produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<OperatingHours>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<OperatingHours>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var hours)) continue;
            results.Add(new TextCandidate<OperatingHours>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(OperatingHours), TextCandidateCategory.Vehicle,
                hours!.ToNormalizedString(), hours.ToString(),
                hours.ToMaskedString(),
                TextMatchConfidence.Low,
                hours));
        }
        return results;
    }

    public int CompareTo(OperatingHours? other) => other is null ? 1 : Hours.CompareTo(other.Hours);
    public bool Equals(OperatingHours? other) => other is not null && Hours == other.Hours;
    public override bool Equals(object? obj) => obj is OperatingHours other && Equals(other);
    public override int GetHashCode() => Hours.GetHashCode();
    public static bool operator ==(OperatingHours? a, OperatingHours? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(OperatingHours? a, OperatingHours? b) => !(a == b);
    public static bool operator <(OperatingHours a, OperatingHours b) => a.Hours < b.Hours;
    public static bool operator >(OperatingHours a, OperatingHours b) => a.Hours > b.Hours;
    public static bool operator <=(OperatingHours a, OperatingHours b) => a.Hours <= b.Hours;
    public static bool operator >=(OperatingHours a, OperatingHours b) => a.Hours >= b.Hours;
}
