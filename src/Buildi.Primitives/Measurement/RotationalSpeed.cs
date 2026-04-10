using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A rotational speed value stored internally in revolutions per minute (RPM). Supports parsing
/// from multiple unit formats (e.g. <c>5200 rpm</c>, <c>100 rps</c>, <c>523.6 rad/s</c>)
/// and conversion between RPM, RPS, and radians per second.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Revolutions_per_minute">Wikipedia — Revolutions per minute</see> — RPM definition</description></item>
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — radian per second</description></item>
/// </list>
/// </remarks>
public sealed class RotationalSpeed : IComparable<RotationalSpeed>, IEquatable<RotationalSpeed>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Rotational Speed", "Varvtal", "🔄", ["https://en.wikipedia.org/wiki/Revolutions_per_minute", "https://www.bipm.org/en/measurement-units/si-derived-units"]);

    private readonly decimal _rpm;
    private readonly RotationalSpeedUnit _originalUnit;

    private RotationalSpeed(decimal rpm, RotationalSpeedUnit originalUnit)
    {
        _rpm = rpm;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in revolutions per minute, e.g. <c>6000</c>.</summary>
    public decimal Rpm => _rpm;

    /// <summary>The value in revolutions per second, e.g. <c>100</c> for 6000 RPM.</summary>
    public decimal Rps => _rpm / RotationalSpeedUnit.Rps.ToBaseUnitFactor;

    /// <summary>The value in radians per second, e.g. <c>628.32</c> for 6000 RPM.</summary>
    public decimal RadiansPerSecond => _rpm / RotationalSpeedUnit.RadiansPerSecond.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public RotationalSpeedUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(RotationalSpeedUnit unit) => _rpm / unit.ToBaseUnitFactor;

    /// <summary>Creates a <see cref="RotationalSpeed"/> from a value and unit.</summary>
    public static RotationalSpeed Create(decimal value, RotationalSpeedUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    /// <summary>Creates a <see cref="RotationalSpeed"/> from RPM, e.g. <c>FromRpm(6000)</c>.</summary>
    public static RotationalSpeed FromRpm(decimal rpm) => new(rpm, RotationalSpeedUnit.Rpm);

    /// <summary>Creates a <see cref="RotationalSpeed"/> from revolutions per second, e.g. <c>FromRps(100)</c> → 6000 RPM.</summary>
    public static RotationalSpeed FromRps(decimal rps) => new(rps * RotationalSpeedUnit.Rps.ToBaseUnitFactor, RotationalSpeedUnit.Rps);

    /// <summary>Creates a <see cref="RotationalSpeed"/> from radians per second, e.g. <c>FromRadiansPerSecond(628.32)</c>.</summary>
    public static RotationalSpeed FromRadiansPerSecond(decimal radPerSec) => new(radPerSec * RotationalSpeedUnit.RadiansPerSecond.ToBaseUnitFactor, RotationalSpeedUnit.RadiansPerSecond);

    public static bool TryParse(string? input, out RotationalSpeed? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            result = new RotationalSpeed(bare, RotationalSpeedUnit.Rpm);
            return true;
        }

        if (!MeasurementUnitParser.TrySplit(trimmed, out var value, out var unitSuffix))
            return false;

        if (!RotationalSpeedUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new RotationalSpeed(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static RotationalSpeed Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid rotational speed.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>5200 rpm</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, RotationalSpeedUnit? unit = null, int? decimals = null)
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
    /// Returns the value in the base unit (RPM) as an invariant string, e.g. <c>5200 rpm</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the value in RPM with invariant formatting, e.g. <c>5200 rpm</c>.</summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_rpm);
        return $"{formatted} {RotationalSpeedUnit.Rpm.Symbol}";
    }

    /// <summary>Returns the value in its original unit with invariant formatting, e.g. <c>5200 rpm</c>.</summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>100 rps</c>.</summary>
    public string ToString(RotationalSpeedUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
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

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:rpm|r/min|rev/min|varv/min|rps|r/s|rev/s|varv/s|rad/s(?:ec)?)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like rotational speed values.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<RotationalSpeed>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<RotationalSpeed>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var rs)) continue;
            results.Add(new TextCandidate<RotationalSpeed>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(RotationalSpeed), TextCandidateCategory.Measurement,
                rs!.ToNormalizedString(), rs.ToString(),
                rs.ToMaskedString(),
                TextMatchConfidence.Low,
                rs));
        }
        return results;
    }

    // --- Arithmetic operators ---

    public static RotationalSpeed operator +(RotationalSpeed a, RotationalSpeed b) => new(a._rpm + b._rpm, a._originalUnit);
    public static RotationalSpeed operator -(RotationalSpeed a, RotationalSpeed b) => new(a._rpm - b._rpm, a._originalUnit);
    public static RotationalSpeed operator *(RotationalSpeed a, decimal factor) => new(a._rpm * factor, a._originalUnit);
    public static RotationalSpeed operator *(decimal factor, RotationalSpeed a) => new(a._rpm * factor, a._originalUnit);
    public static RotationalSpeed operator /(RotationalSpeed a, decimal divisor) => new(a._rpm / divisor, a._originalUnit);
    public static RotationalSpeed operator -(RotationalSpeed a) => new(-a._rpm, a._originalUnit);

    public static bool operator ==(RotationalSpeed? a, RotationalSpeed? b) => a?._rpm == b?._rpm;
    public static bool operator !=(RotationalSpeed? a, RotationalSpeed? b) => !(a == b);
    public static bool operator <(RotationalSpeed a, RotationalSpeed b) => a._rpm < b._rpm;
    public static bool operator >(RotationalSpeed a, RotationalSpeed b) => a._rpm > b._rpm;
    public static bool operator <=(RotationalSpeed a, RotationalSpeed b) => a._rpm <= b._rpm;
    public static bool operator >=(RotationalSpeed a, RotationalSpeed b) => a._rpm >= b._rpm;

    public int CompareTo(RotationalSpeed? other) => other is null ? 1 : _rpm.CompareTo(other._rpm);
    public bool Equals(RotationalSpeed? other) => other is not null && _rpm == other._rpm;
    public override bool Equals(object? obj) => obj is RotationalSpeed other && Equals(other);
    public override int GetHashCode() => _rpm.GetHashCode();
}
