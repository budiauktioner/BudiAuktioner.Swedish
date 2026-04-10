using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// An electric current value stored internally in amperes. Supports parsing from common SI prefixes
/// (e.g. <c>2.5 A</c>, <c>500 mA</c>, <c>10 kA</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-base-units">BIPM SI base units</see> — ampere definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Ampere">Wikipedia — Ampere</see></description></item>
/// </list>
/// </remarks>
public sealed class ElectricCurrent : IComparable<ElectricCurrent>, IEquatable<ElectricCurrent>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Electric Current", "Elektrisk ström", "⚡", ["https://www.bipm.org/en/measurement-units/si-base-units", "https://en.wikipedia.org/wiki/Ampere"]);

    private readonly decimal _amperes;
    private readonly ElectricCurrentUnit _originalUnit;

    private ElectricCurrent(decimal amperes, ElectricCurrentUnit originalUnit)
    {
        _amperes = amperes;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in microamperes (µA), e.g. <c>1000000</c> for 1 A.</summary>
    public decimal Microamperes => _amperes / ElectricCurrentUnit.Microampere.ToBaseUnitFactor;
    /// <summary>The value in milliamperes, e.g. <c>1000</c> for 1 A.</summary>
    public decimal Milliamperes => _amperes / ElectricCurrentUnit.Milliampere.ToBaseUnitFactor;
    /// <summary>The value in amperes, e.g. <c>2.5</c>.</summary>
    public decimal Amperes => _amperes;
    /// <summary>The value in kiloamperes, e.g. <c>0.01</c> for 10 A.</summary>
    public decimal Kiloamperes => _amperes / ElectricCurrentUnit.Kiloampere.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public ElectricCurrentUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(ElectricCurrentUnit unit) => _amperes / unit.ToBaseUnitFactor;

    public static ElectricCurrent FromMicroamperes(decimal ua) => new(ua * ElectricCurrentUnit.Microampere.ToBaseUnitFactor, ElectricCurrentUnit.Microampere);
    public static ElectricCurrent FromMilliamperes(decimal ma) => new(ma * ElectricCurrentUnit.Milliampere.ToBaseUnitFactor, ElectricCurrentUnit.Milliampere);
    public static ElectricCurrent FromAmperes(decimal a) => new(a, ElectricCurrentUnit.Ampere);
    public static ElectricCurrent FromKiloamperes(decimal ka) => new(ka * ElectricCurrentUnit.Kiloampere.ToBaseUnitFactor, ElectricCurrentUnit.Kiloampere);

    /// <summary>Creates an <see cref="ElectricCurrent"/> from a value and unit.</summary>
    public static ElectricCurrent Create(decimal value, ElectricCurrentUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out ElectricCurrent? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!ElectricCurrentUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new ElectricCurrent(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static ElectricCurrent Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid electric current.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>500 mA</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, ElectricCurrentUnit? unit = null, int? decimals = null)
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
    /// Returns the value in amperes as an invariant string, e.g. <c>0.5 A</c>.
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
    /// Returns the value in amperes with invariant formatting, e.g. <c>0.5 A</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_amperes);
        return $"{formatted} {ElectricCurrentUnit.Ampere.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>500 mA</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>500 mA</c>.
    /// </summary>
    public string ToString(ElectricCurrentUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable SI unit for this value, e.g. kA for 1500 amperes.
    /// </summary>
    public ElectricCurrentUnit NaturalUnit => ElectricCurrentUnit.GetNatural(_amperes);

    /// <summary>
    /// Returns the value formatted in the most human-readable SI unit, e.g. <c>1.5 kA</c> instead of <c>1500 A</c>.
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

    public static ElectricCurrent operator +(ElectricCurrent a, ElectricCurrent b) => new(a._amperes + b._amperes, a._originalUnit);
    public static ElectricCurrent operator -(ElectricCurrent a, ElectricCurrent b) => new(a._amperes - b._amperes, a._originalUnit);
    public static ElectricCurrent operator *(ElectricCurrent a, decimal factor) => new(a._amperes * factor, a._originalUnit);
    public static ElectricCurrent operator *(decimal factor, ElectricCurrent a) => new(a._amperes * factor, a._originalUnit);
    public static ElectricCurrent operator /(ElectricCurrent a, decimal divisor) => new(a._amperes / divisor, a._originalUnit);
    public static ElectricCurrent operator -(ElectricCurrent a) => new(-a._amperes, a._originalUnit);

    public static bool operator ==(ElectricCurrent? a, ElectricCurrent? b) => a?._amperes == b?._amperes;
    public static bool operator !=(ElectricCurrent? a, ElectricCurrent? b) => !(a == b);
    public static bool operator <(ElectricCurrent a, ElectricCurrent b) => a._amperes < b._amperes;
    public static bool operator >(ElectricCurrent a, ElectricCurrent b) => a._amperes > b._amperes;
    public static bool operator <=(ElectricCurrent a, ElectricCurrent b) => a._amperes <= b._amperes;
    public static bool operator >=(ElectricCurrent a, ElectricCurrent b) => a._amperes >= b._amperes;

    public int CompareTo(ElectricCurrent? other) => other is null ? 1 : _amperes.CompareTo(other._amperes);
    public bool Equals(ElectricCurrent? other) => other is not null && _amperes == other._amperes;
    public override bool Equals(object? obj) => obj is ElectricCurrent other && Equals(other);
    public override int GetHashCode() => _amperes.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:µA|uA|mA|kA|A)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like electric current values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<ElectricCurrent>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<ElectricCurrent>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var current)) continue;
            results.Add(new TextCandidate<ElectricCurrent>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(ElectricCurrent), TextCandidateCategory.Measurement,
                current!.ToNormalizedString(), current.ToString(),
                current.ToMaskedString(),
                TextMatchConfidence.Medium,
                current));
        }
        return results;
    }
}
