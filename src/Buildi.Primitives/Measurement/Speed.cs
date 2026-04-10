using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A speed value stored internally in meters per second. Supports parsing from multiple unit formats
/// (e.g. <c>100 km/h</c>, <c>60 mph</c>, <c>10 kn</c>) and conversion between metric and imperial units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-base-units">BIPM SI base units</see> — speed as length over time (m/s)</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Speed : IComparable<Speed>, IEquatable<Speed>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Speed", "Hastighet", "🏎️", ["https://www.bipm.org/en/measurement-units/si-base-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _metersPerSecond;
    private readonly SpeedUnit _originalUnit;

    private Speed(decimal metersPerSecond, SpeedUnit originalUnit)
    {
        _metersPerSecond = metersPerSecond;
        _originalUnit = originalUnit;
    }

    private static decimal ToMetersPerSecond(decimal value, SpeedUnit unit) =>
        unit == SpeedUnit.KilometersPerHour ? value * 1000m / 3600m : value * unit.ToBaseUnitFactor;

    private decimal ValueIn(SpeedUnit unit) =>
        unit == SpeedUnit.KilometersPerHour ? _metersPerSecond * 3600m / 1000m : _metersPerSecond / unit.ToBaseUnitFactor;

    /// <summary>The value in meters per second, e.g. <c>10</c>.</summary>
    public decimal MetersPerSecond => _metersPerSecond;

    /// <summary>The value in kilometers per hour, e.g. <c>36</c> for 10 m/s.</summary>
    public decimal KilometersPerHour => ValueIn(SpeedUnit.KilometersPerHour);

    /// <summary>The value in miles per hour.</summary>
    public decimal MilesPerHour => _metersPerSecond / SpeedUnit.MilesPerHour.ToBaseUnitFactor;

    /// <summary>The value in feet per second.</summary>
    public decimal FeetPerSecond => _metersPerSecond / SpeedUnit.FeetPerSecond.ToBaseUnitFactor;

    /// <summary>The value in knots (nautical miles per hour).</summary>
    public decimal Knots => _metersPerSecond / SpeedUnit.Knot.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public SpeedUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(SpeedUnit unit) => ValueIn(unit);

    public static Speed FromMetersPerSecond(decimal mps) => new(mps, SpeedUnit.MetersPerSecond);
    public static Speed FromKilometersPerHour(decimal kmh) => new(ToMetersPerSecond(kmh, SpeedUnit.KilometersPerHour), SpeedUnit.KilometersPerHour);
    public static Speed FromMilesPerHour(decimal mph) => new(mph * SpeedUnit.MilesPerHour.ToBaseUnitFactor, SpeedUnit.MilesPerHour);
    public static Speed FromFeetPerSecond(decimal fps) => new(fps * SpeedUnit.FeetPerSecond.ToBaseUnitFactor, SpeedUnit.FeetPerSecond);
    public static Speed FromKnots(decimal kn) => new(kn * SpeedUnit.Knot.ToBaseUnitFactor, SpeedUnit.Knot);

    /// <summary>Creates a <see cref="Speed"/> from a value and unit.</summary>
    public static Speed Create(decimal value, SpeedUnit unit) => new(ToMetersPerSecond(value, unit), unit);

    public static bool TryParse(string? input, out Speed? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!SpeedUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Speed(ToMetersPerSecond(value, unit), unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Speed Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid speed.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>100 km/h</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, SpeedUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.ToString(unit ?? r.OriginalUnit, decimals);
            return r.ToString();
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns the value in SI base form (m/s) as an invariant string, e.g. <c>10 m/s</c>.
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
    /// Returns the value in meters per second with invariant formatting, e.g. <c>10 m/s</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_metersPerSecond);
        return $"{formatted} {SpeedUnit.MetersPerSecond.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>100 km/h</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = ValueIn(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>36 km/h</c> for 10 m/s.
    /// </summary>
    public string ToString(SpeedUnit unit, int? decimals = null)
    {
        var valueInUnit = ValueIn(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

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

    public static Speed operator +(Speed a, Speed b) => new(a._metersPerSecond + b._metersPerSecond, a._originalUnit);
    public static Speed operator -(Speed a, Speed b) => new(a._metersPerSecond - b._metersPerSecond, a._originalUnit);
    public static Speed operator *(Speed a, decimal factor) => new(a._metersPerSecond * factor, a._originalUnit);
    public static Speed operator *(decimal factor, Speed a) => new(a._metersPerSecond * factor, a._originalUnit);
    public static Speed operator /(Speed a, decimal divisor) => new(a._metersPerSecond / divisor, a._originalUnit);
    public static Speed operator -(Speed a) => new(-a._metersPerSecond, a._originalUnit);

    public static bool operator ==(Speed? a, Speed? b) => a?._metersPerSecond == b?._metersPerSecond;
    public static bool operator !=(Speed? a, Speed? b) => !(a == b);
    public static bool operator <(Speed a, Speed b) => a._metersPerSecond < b._metersPerSecond;
    public static bool operator >(Speed a, Speed b) => a._metersPerSecond > b._metersPerSecond;
    public static bool operator <=(Speed a, Speed b) => a._metersPerSecond <= b._metersPerSecond;
    public static bool operator >=(Speed a, Speed b) => a._metersPerSecond >= b._metersPerSecond;

    public int CompareTo(Speed? other) => other is null ? 1 : _metersPerSecond.CompareTo(other._metersPerSecond);
    public bool Equals(Speed? other) => other is not null && _metersPerSecond == other._metersPerSecond;
    public override bool Equals(object? obj) => obj is Speed other && Equals(other);
    public override int GetHashCode() => _metersPerSecond.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:miles\s+per\s+hour|km/h|ft/s|m/s|kph|kmh|km/t|mph|knots?|knop|kt|kn)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like speed values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Speed>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Speed>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var speed)) continue;
            results.Add(new TextCandidate<Speed>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Speed), TextCandidateCategory.Measurement,
                speed!.ToNormalizedString(), speed.ToString(),
                speed.ToMaskedString(),
                TextMatchConfidence.Medium,
                speed));
        }
        return results;
    }
}
