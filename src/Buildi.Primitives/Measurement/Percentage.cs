using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A percentage stored internally as a fraction in the range 0–1 (where <c>1</c> = 100%).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/publications/si-brochure">BIPM SI Brochure</see> — dimensionless quantities and percent</description></item>
/// </list>
/// </remarks>
public sealed class Percentage : IComparable<Percentage>, IEquatable<Percentage>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Percentage", "Procent", "📊", ["https://www.bipm.org/en/publications/si-brochure"]);

    private readonly decimal _fraction;

    private Percentage(decimal fraction)
    {
        if (fraction < 0m || fraction > 1m)
            throw new ArgumentOutOfRangeException(nameof(fraction), "Fraction must be in the range 0 to 1 inclusive.");
        _fraction = fraction;
    }

    /// <summary>The value as a fraction from 0 to 1, e.g. <c>0.85</c> for 85%.</summary>
    public decimal Value => _fraction;

    /// <summary>The value on a 0–100 scale, e.g. <c>85</c> for 85%.</summary>
    public decimal Percent => _fraction * 100m;

    /// <summary>Creates a percentage from a 0–1 fraction, e.g. <c>0.85</c> for 85%.</summary>
    public static Percentage FromDecimal(decimal value) => new(value);

    /// <summary>Creates a percentage from a 0–100 value, e.g. <c>85</c> for 85%.</summary>
    public static Percentage FromPercent(decimal percent)
    {
        if (percent < 0m || percent > 100m)
            throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be in the range 0 to 100 inclusive.");
        return new(percent / 100m);
    }

    /// <summary>Creates a percentage from a 0–100 value; same as <see cref="FromPercent"/>.</summary>
    public static Percentage Create(decimal percent) => FromPercent(percent);

    private static readonly Regex PercentWordSuffix = new(
        @"^(?<sign>[+-])?\s*(?<number>[0-9][0-9 .,]*[0-9]|[0-9])\s+(?<suffix>procent|percent)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool TryParse(string? input, out Percentage? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > 100) return false;

        decimal raw;
        var isPercentScale = false;

        if (trimmed.EndsWith('%'))
        {
            var numPart = trimmed[..^1].TrimEnd();
            if (!MeasurementUnitParser.TryParseNumberOnly(numPart, out raw)) return false;
            isPercentScale = true;
        }
        else
        {
            var m = PercentWordSuffix.Match(trimmed);
            if (m.Success)
            {
                var sign = m.Groups["sign"].Value;
                var number = m.Groups["number"].Value;
                var combined = string.IsNullOrEmpty(sign) ? number : sign + number;
                if (!MeasurementUnitParser.TryParseNumberOnly(combined, out raw)) return false;
                isPercentScale = true;
            }
            else
            {
                if (!MeasurementUnitParser.TryParseNumberOnly(trimmed, out raw)) return false;
            }
        }

        decimal fraction;
        if (isPercentScale)
        {
            if (raw < 0m || raw > 100m) return false;
            fraction = raw / 100m;
        }
        else
        {
            if (raw < 0m) return false;
            if (raw <= 1m)
                fraction = raw;
            else if (raw <= 100m)
                fraction = raw / 100m;
            else
                return false;
        }

        result = new Percentage(fraction);
        return true;
    }

    public static Percentage Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid percentage.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display string with a percent sign, e.g. <c>85%</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
            return decimals is not null ? FormatPercent(r.Value, decimals) : r.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns the fraction as an invariant decimal string, e.g. <c>0.85</c>.
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

    /// <summary>Returns the fraction with invariant formatting, e.g. <c>0.85</c>.</summary>
    public string ToNormalizedString() => FormatDecimal(_fraction);

    /// <summary>Returns the value with a percent sign, e.g. <c>85%</c>.</summary>
    public override string ToString() => FormatPercent(_fraction);

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    private static string FormatPercent(decimal fraction, int? decimals = null)
    {
        var p = fraction * 100m;
        if (decimals is not null)
            p = Math.Round(p, decimals.Value, MidpointRounding.AwayFromZero);
        var s = p.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return $"{s}%";
    }

    private static Percentage CheckedFraction(decimal fraction)
    {
        if (fraction < 0m || fraction > 1m)
            throw new OverflowException("The operation produced a percentage outside the range 0% to 100%.");
        return new Percentage(fraction);
    }

    public static Percentage operator +(Percentage a, Percentage b) => CheckedFraction(a._fraction + b._fraction);
    public static Percentage operator -(Percentage a, Percentage b) => CheckedFraction(a._fraction - b._fraction);
    public static Percentage operator *(Percentage a, decimal factor) => CheckedFraction(a._fraction * factor);
    public static Percentage operator *(decimal factor, Percentage a) => CheckedFraction(a._fraction * factor);
    public static Percentage operator /(Percentage a, decimal divisor) => CheckedFraction(a._fraction / divisor);

    public static bool operator ==(Percentage? a, Percentage? b) => a?._fraction == b?._fraction;
    public static bool operator !=(Percentage? a, Percentage? b) => !(a == b);
    public static bool operator <(Percentage a, Percentage b) => a._fraction < b._fraction;
    public static bool operator >(Percentage a, Percentage b) => a._fraction > b._fraction;
    public static bool operator <=(Percentage a, Percentage b) => a._fraction <= b._fraction;
    public static bool operator >=(Percentage a, Percentage b) => a._fraction >= b._fraction;

    public int CompareTo(Percentage? other) => other is null ? 1 : _fraction.CompareTo(other._fraction);
    public bool Equals(Percentage? other) => other is not null && _fraction == other._fraction;
    public override bool Equals(object? obj) => obj is Percentage other && Equals(other);
    public override int GetHashCode() => _fraction.GetHashCode();

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*%",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for substrings that look like <c>number%</c> values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Percentage>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Percentage>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var p)) continue;
            results.Add(new TextCandidate<Percentage>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Percentage), TextCandidateCategory.Measurement,
                p!.ToNormalizedString(), p.ToString(),
                p.ToMaskedString(),
                TextMatchConfidence.Low,
                p));
        }
        return results;
    }
}
