using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A length (distance) value stored internally in meters. Supports parsing from multiple unit formats
/// (e.g. <c>10 km</c>, <c>5.5 cm</c>, <c>3 ft</c>) and conversion between metric and imperial units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-base-units">BIPM SI base units</see> — meter definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Length : IComparable<Length>, IEquatable<Length>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Length", "Längd", "📏", ["https://www.bipm.org/en/measurement-units/si-base-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _meters;
    private readonly LengthUnit _originalUnit;

    private Length(decimal meters, LengthUnit originalUnit)
    {
        _meters = meters;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in nanometers, e.g. <c>10000000000</c> for 10 m.</summary>
    public decimal Nanometers => _meters / LengthUnit.Nanometer.ToBaseUnitFactor;
    /// <summary>The value in micrometers (µm), e.g. <c>10000000</c> for 10 m.</summary>
    public decimal Micrometers => _meters / LengthUnit.Micrometer.ToBaseUnitFactor;
    /// <summary>The value in millimeters, e.g. <c>10000</c> for 10 m.</summary>
    public decimal Millimeters => _meters / LengthUnit.Millimeter.ToBaseUnitFactor;
    /// <summary>The value in centimeters, e.g. <c>1000</c> for 10 m.</summary>
    public decimal Centimeters => _meters / LengthUnit.Centimeter.ToBaseUnitFactor;
    /// <summary>The value in decimeters, e.g. <c>100</c> for 10 m.</summary>
    public decimal Decimeters => _meters / LengthUnit.Decimeter.ToBaseUnitFactor;
    /// <summary>The value in meters, e.g. <c>10</c>.</summary>
    public decimal Meters => _meters;
    /// <summary>The value in kilometers, e.g. <c>0.01</c> for 10 m.</summary>
    public decimal Kilometers => _meters / LengthUnit.Kilometer.ToBaseUnitFactor;
    /// <summary>The value in inches, e.g. <c>393.7...</c> for 10 m.</summary>
    public decimal Inches => _meters / LengthUnit.Inch.ToBaseUnitFactor;
    /// <summary>The value in feet, e.g. <c>32.8...</c> for 10 m.</summary>
    public decimal Feet => _meters / LengthUnit.Foot.ToBaseUnitFactor;
    /// <summary>The value in yards, e.g. <c>10.9...</c> for 10 m.</summary>
    public decimal Yards => _meters / LengthUnit.Yard.ToBaseUnitFactor;
    /// <summary>The value in miles, e.g. <c>0.006...</c> for 10 m.</summary>
    public decimal Miles => _meters / LengthUnit.Mile.ToBaseUnitFactor;
    /// <summary>The value in nautical miles.</summary>
    public decimal NauticalMiles => _meters / LengthUnit.NauticalMile.ToBaseUnitFactor;
    /// <summary>The value in Swedish miles (1 mil = 10 km).</summary>
    public decimal SwedishMiles => _meters / LengthUnit.SwedishMile.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public LengthUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(LengthUnit unit) => _meters / unit.ToBaseUnitFactor;

    public static Length FromNanometers(decimal nm) => new(nm * LengthUnit.Nanometer.ToBaseUnitFactor, LengthUnit.Nanometer);
    public static Length FromMicrometers(decimal um) => new(um * LengthUnit.Micrometer.ToBaseUnitFactor, LengthUnit.Micrometer);
    public static Length FromMillimeters(decimal mm) => new(mm * LengthUnit.Millimeter.ToBaseUnitFactor, LengthUnit.Millimeter);
    public static Length FromCentimeters(decimal cm) => new(cm * LengthUnit.Centimeter.ToBaseUnitFactor, LengthUnit.Centimeter);
    public static Length FromDecimeters(decimal dm) => new(dm * LengthUnit.Decimeter.ToBaseUnitFactor, LengthUnit.Decimeter);
    public static Length FromMeters(decimal m) => new(m, LengthUnit.Meter);
    public static Length FromKilometers(decimal km) => new(km * LengthUnit.Kilometer.ToBaseUnitFactor, LengthUnit.Kilometer);
    public static Length FromInches(decimal inches) => new(inches * LengthUnit.Inch.ToBaseUnitFactor, LengthUnit.Inch);
    public static Length FromFeet(decimal ft) => new(ft * LengthUnit.Foot.ToBaseUnitFactor, LengthUnit.Foot);
    public static Length FromYards(decimal yd) => new(yd * LengthUnit.Yard.ToBaseUnitFactor, LengthUnit.Yard);
    public static Length FromMiles(decimal mi) => new(mi * LengthUnit.Mile.ToBaseUnitFactor, LengthUnit.Mile);
    public static Length FromNauticalMiles(decimal nmi) => new(nmi * LengthUnit.NauticalMile.ToBaseUnitFactor, LengthUnit.NauticalMile);
    public static Length FromSwedishMiles(decimal mil) => new(mil * LengthUnit.SwedishMile.ToBaseUnitFactor, LengthUnit.SwedishMile);

    /// <summary>Creates a <see cref="Length"/> from a value and unit.</summary>
    public static Length Create(decimal value, LengthUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Length? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!LengthUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Length(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Length Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid length.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>10 km</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, LengthUnit? unit = null, int? decimals = null)
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
    /// Returns the value in SI base unit (meters) as an invariant string, e.g. <c>10000 m</c>.
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
    /// Returns the value in meters with invariant formatting, e.g. <c>10000 m</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_meters);
        return $"{formatted} {LengthUnit.Meter.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>10 km</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>0.01 km</c>.
    /// </summary>
    public string ToString(LengthUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable metric unit for this value, e.g. km for 1500 meters.
    /// </summary>
    public LengthUnit NaturalUnit => LengthUnit.GetNatural(_meters);

    /// <summary>
    /// Returns the value formatted in the most human-readable metric unit, e.g. <c>1.5 km</c> instead of <c>1500 m</c>.
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

    public static Length operator +(Length a, Length b) => new(a._meters + b._meters, a._originalUnit);
    public static Length operator -(Length a, Length b) => new(a._meters - b._meters, a._originalUnit);
    public static Length operator *(Length a, decimal factor) => new(a._meters * factor, a._originalUnit);
    public static Length operator *(decimal factor, Length a) => new(a._meters * factor, a._originalUnit);
    public static Length operator /(Length a, decimal divisor) => new(a._meters / divisor, a._originalUnit);
    public static Length operator -(Length a) => new(-a._meters, a._originalUnit);

    public static bool operator ==(Length? a, Length? b) => a?._meters == b?._meters;
    public static bool operator !=(Length? a, Length? b) => !(a == b);
    public static bool operator <(Length a, Length b) => a._meters < b._meters;
    public static bool operator >(Length a, Length b) => a._meters > b._meters;
    public static bool operator <=(Length a, Length b) => a._meters <= b._meters;
    public static bool operator >=(Length a, Length b) => a._meters >= b._meters;

    public int CompareTo(Length? other) => other is null ? 1 : _meters.CompareTo(other._meters);
    public bool Equals(Length? other) => other is not null && _meters == other._meters;
    public override bool Equals(object? obj) => obj is Length other && Equals(other);
    public override int GetHashCode() => _meters.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:µm|nm|mm|cm|dm|km|mi|ft|yd|nmi|mil|in|m)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like length values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Length>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Length>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var length)) continue;
            results.Add(new TextCandidate<Length>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Length), TextCandidateCategory.Measurement,
                length!.ToNormalizedString(), length.ToString(),
                length.ToMaskedString(),
                TextMatchConfidence.Medium,
                length));
        }
        return results;
    }
}
