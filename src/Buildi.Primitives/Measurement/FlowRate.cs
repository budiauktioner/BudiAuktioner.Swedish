using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A volumetric flow rate value stored internally in liters per minute. Supports parsing from common units
/// (e.g. <c>10 L/min</c>, <c>2.5 m³/h</c>, <c>5 gal/min</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Volumetric_flow_rate">Wikipedia — Volumetric flow rate</see></description></item>
/// </list>
/// </remarks>
public sealed class FlowRate : IComparable<FlowRate>, IEquatable<FlowRate>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Flow Rate", "Flöde", "🌊", ["https://en.wikipedia.org/wiki/Volumetric_flow_rate"]);

    private readonly decimal _litersPerMinute;
    private readonly FlowRateUnit _originalUnit;

    private FlowRate(decimal litersPerMinute, FlowRateUnit originalUnit)
    {
        _litersPerMinute = litersPerMinute;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in liters per second, e.g. <c>0.5</c> for 30 L/min.</summary>
    public decimal LitersPerSecond => _litersPerMinute / FlowRateUnit.LitersPerSecond.ToBaseUnitFactor;
    /// <summary>The value in liters per minute, e.g. <c>30</c>.</summary>
    public decimal LitersPerMinute => _litersPerMinute;
    /// <summary>The value in liters per hour, e.g. <c>1800</c> for 30 L/min.</summary>
    public decimal LitersPerHour => FlowRateUnit.LitersPerHour.FromLitersPerMinute(_litersPerMinute);
    /// <summary>The value in cubic meters per hour, e.g. <c>1.8</c> for 30 L/min.</summary>
    public decimal CubicMetersPerHour => FlowRateUnit.CubicMetersPerHour.FromLitersPerMinute(_litersPerMinute);
    /// <summary>The value in cubic meters per minute, e.g. <c>0.03</c> for 30 L/min.</summary>
    public decimal CubicMetersPerMinute => FlowRateUnit.CubicMetersPerMinute.FromLitersPerMinute(_litersPerMinute);
    /// <summary>The value in US gallons per minute, e.g. <c>7.93</c> for 30 L/min.</summary>
    public decimal GallonsPerMinute => FlowRateUnit.GallonsPerMinute.FromLitersPerMinute(_litersPerMinute);

    /// <summary>The unit the value was originally parsed from.</summary>
    public FlowRateUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(FlowRateUnit unit) => unit.FromLitersPerMinute(_litersPerMinute);

    public static FlowRate FromLitersPerSecond(decimal value) => new(value * FlowRateUnit.LitersPerSecond.ToBaseUnitFactor, FlowRateUnit.LitersPerSecond);
    public static FlowRate FromLitersPerMinute(decimal value) => new(value, FlowRateUnit.LitersPerMinute);
    public static FlowRate FromLitersPerHour(decimal value) => new(FlowRateUnit.LitersPerHour.ToLitersPerMinute(value), FlowRateUnit.LitersPerHour);
    public static FlowRate FromCubicMetersPerHour(decimal value) => new(FlowRateUnit.CubicMetersPerHour.ToLitersPerMinute(value), FlowRateUnit.CubicMetersPerHour);
    public static FlowRate FromCubicMetersPerMinute(decimal value) => new(value * FlowRateUnit.CubicMetersPerMinute.ToBaseUnitFactor, FlowRateUnit.CubicMetersPerMinute);
    public static FlowRate FromGallonsPerMinute(decimal value) => new(value * FlowRateUnit.GallonsPerMinute.ToBaseUnitFactor, FlowRateUnit.GallonsPerMinute);

    /// <summary>Creates a <see cref="FlowRate"/> from a value and unit.</summary>
    public static FlowRate Create(decimal value, FlowRateUnit unit) => new(unit.ToLitersPerMinute(value), unit);

    public static bool TryParse(string? input, out FlowRate? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!FlowRateUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new FlowRate(unit.ToLitersPerMinute(value), unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static FlowRate Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid flow rate.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>2.5 m³/h</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, FlowRateUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.ToString(unit ?? r.OriginalUnit, decimals);
            return r.ToString();
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns the value in liters per minute as an invariant string, e.g. <c>30 L/min</c>.
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

    /// <summary>
    /// Returns the value in liters per minute with invariant formatting, e.g. <c>30 L/min</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_litersPerMinute);
        return $"{formatted} {FlowRateUnit.LitersPerMinute.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>2.5 m³/h</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = _originalUnit.FromLitersPerMinute(_litersPerMinute);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>150 L/h</c>.
    /// </summary>
    public string ToString(FlowRateUnit unit, int? decimals = null)
    {
        var valueInUnit = unit.FromLitersPerMinute(_litersPerMinute);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable unit for this value, e.g. m³/h for large flows.
    /// </summary>
    public FlowRateUnit NaturalUnit => FlowRateUnit.GetNatural(_litersPerMinute);

    /// <summary>
    /// Returns the value formatted in the most human-readable unit, e.g. <c>2.5 m³/h</c> instead of <c>41.6667 L/min</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => ToString(NaturalUnit, decimals);

    private static string FormatDecimal(decimal value, int? decimals = null)
    {
        if (decimals is not null)
            value = Math.Round(value, decimals.Value, MidpointRounding.AwayFromZero);
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    // --- Arithmetic operators ---

    public static FlowRate operator +(FlowRate a, FlowRate b) => new(a._litersPerMinute + b._litersPerMinute, a._originalUnit);
    public static FlowRate operator -(FlowRate a, FlowRate b) => new(a._litersPerMinute - b._litersPerMinute, a._originalUnit);
    public static FlowRate operator *(FlowRate a, decimal factor) => new(a._litersPerMinute * factor, a._originalUnit);
    public static FlowRate operator *(decimal factor, FlowRate a) => new(a._litersPerMinute * factor, a._originalUnit);
    public static FlowRate operator /(FlowRate a, decimal divisor) => new(a._litersPerMinute / divisor, a._originalUnit);
    public static FlowRate operator -(FlowRate a) => new(-a._litersPerMinute, a._originalUnit);

    public static bool operator ==(FlowRate? a, FlowRate? b) => a?._litersPerMinute == b?._litersPerMinute;
    public static bool operator !=(FlowRate? a, FlowRate? b) => !(a == b);
    public static bool operator <(FlowRate a, FlowRate b) => a._litersPerMinute < b._litersPerMinute;
    public static bool operator >(FlowRate a, FlowRate b) => a._litersPerMinute > b._litersPerMinute;
    public static bool operator <=(FlowRate a, FlowRate b) => a._litersPerMinute <= b._litersPerMinute;
    public static bool operator >=(FlowRate a, FlowRate b) => a._litersPerMinute >= b._litersPerMinute;

    public int CompareTo(FlowRate? other) => other is null ? 1 : _litersPerMinute.CompareTo(other._litersPerMinute);
    public bool Equals(FlowRate? other) => other is not null && _litersPerMinute == other._litersPerMinute;
    public override bool Equals(object? obj) => obj is FlowRate other && Equals(other);
    public override int GetHashCode() => _litersPerMinute.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:m[³3]/(?:h|min|tim)|l/(?:s|min|h|timme)|gal/min|gpm)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like flow rate values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<FlowRate>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<FlowRate>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var flowRate)) continue;
            results.Add(new TextCandidate<FlowRate>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(FlowRate), TextCandidateCategory.Measurement,
                flowRate!.ToNormalizedString(), flowRate.ToString(),
                flowRate.ToMaskedString(),
                TextMatchConfidence.Medium,
                flowRate));
        }
        return results;
    }
}
