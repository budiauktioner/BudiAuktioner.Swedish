using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// An area value stored internally in square meters. Supports parsing from multiple unit formats
/// (e.g. <c>2 ha</c>, <c>100 m²</c>, <c>5 sq ft</c>) and conversion between metric and imperial units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-base-units">BIPM SI base units</see> — meter; area derived as m²</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Area : IComparable<Area>, IEquatable<Area>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Area", "Yta", "📐", ["https://www.bipm.org/en/measurement-units/si-base-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _squareMeters;
    private readonly AreaUnit _originalUnit;

    private Area(decimal squareMeters, AreaUnit originalUnit)
    {
        _squareMeters = squareMeters;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in square millimeters.</summary>
    public decimal SquareMillimeters => _squareMeters / AreaUnit.SquareMillimeter.ToBaseUnitFactor;
    /// <summary>The value in square centimeters.</summary>
    public decimal SquareCentimeters => _squareMeters / AreaUnit.SquareCentimeter.ToBaseUnitFactor;
    /// <summary>The value in square meters.</summary>
    public decimal SquareMeters => _squareMeters;
    /// <summary>The value in square kilometers.</summary>
    public decimal SquareKilometers => _squareMeters / AreaUnit.SquareKilometer.ToBaseUnitFactor;
    /// <summary>The value in hectares.</summary>
    public decimal Hectares => _squareMeters / AreaUnit.Hectare.ToBaseUnitFactor;
    /// <summary>The value in acres.</summary>
    public decimal Acres => _squareMeters / AreaUnit.Acre.ToBaseUnitFactor;
    /// <summary>The value in square feet.</summary>
    public decimal SquareFeet => _squareMeters / AreaUnit.SquareFoot.ToBaseUnitFactor;
    /// <summary>The value in square inches.</summary>
    public decimal SquareInches => _squareMeters / AreaUnit.SquareInch.ToBaseUnitFactor;
    /// <summary>The value in square yards.</summary>
    public decimal SquareYards => _squareMeters / AreaUnit.SquareYard.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public AreaUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(AreaUnit unit) => _squareMeters / unit.ToBaseUnitFactor;

    public static Area FromSquareMillimeters(decimal mm2) => new(mm2 * AreaUnit.SquareMillimeter.ToBaseUnitFactor, AreaUnit.SquareMillimeter);
    public static Area FromSquareCentimeters(decimal cm2) => new(cm2 * AreaUnit.SquareCentimeter.ToBaseUnitFactor, AreaUnit.SquareCentimeter);
    public static Area FromSquareMeters(decimal m2) => new(m2, AreaUnit.SquareMeter);
    public static Area FromSquareKilometers(decimal km2) => new(km2 * AreaUnit.SquareKilometer.ToBaseUnitFactor, AreaUnit.SquareKilometer);
    public static Area FromHectares(decimal ha) => new(ha * AreaUnit.Hectare.ToBaseUnitFactor, AreaUnit.Hectare);
    public static Area FromAcres(decimal acre) => new(acre * AreaUnit.Acre.ToBaseUnitFactor, AreaUnit.Acre);
    public static Area FromSquareFeet(decimal sqFt) => new(sqFt * AreaUnit.SquareFoot.ToBaseUnitFactor, AreaUnit.SquareFoot);
    public static Area FromSquareInches(decimal sqIn) => new(sqIn * AreaUnit.SquareInch.ToBaseUnitFactor, AreaUnit.SquareInch);
    public static Area FromSquareYards(decimal sqYd) => new(sqYd * AreaUnit.SquareYard.ToBaseUnitFactor, AreaUnit.SquareYard);

    /// <summary>Creates an <see cref="Area"/> from a value and unit.</summary>
    public static Area Create(decimal value, AreaUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Area? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!AreaUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Area(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Area Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid area.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>2 ha</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, AreaUnit? unit = null, int? decimals = null)
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
    /// Returns the value in SI base unit (square meters) as an invariant string, e.g. <c>20000 m²</c>.
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
    /// Returns the value in square meters with invariant formatting, e.g. <c>20000 m²</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_squareMeters);
        return $"{formatted} {AreaUnit.SquareMeter.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>2 ha</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>0.02 km²</c>.
    /// </summary>
    public string ToString(AreaUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable metric unit for this value, e.g. ha for 15,000 m².
    /// </summary>
    public AreaUnit NaturalUnit => AreaUnit.GetNatural(_squareMeters);

    /// <summary>
    /// Returns the value formatted in the most human-readable metric unit, e.g. <c>1.5 ha</c> instead of <c>15000 m²</c>.
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

    public static Area operator +(Area a, Area b) => new(a._squareMeters + b._squareMeters, a._originalUnit);
    public static Area operator -(Area a, Area b) => new(a._squareMeters - b._squareMeters, a._originalUnit);
    public static Area operator *(Area a, decimal factor) => new(a._squareMeters * factor, a._originalUnit);
    public static Area operator *(decimal factor, Area a) => new(a._squareMeters * factor, a._originalUnit);
    public static Area operator /(Area a, decimal divisor) => new(a._squareMeters / divisor, a._originalUnit);
    public static Area operator -(Area a) => new(-a._squareMeters, a._originalUnit);

    public static bool operator ==(Area? a, Area? b) => a?._squareMeters == b?._squareMeters;
    public static bool operator !=(Area? a, Area? b) => !(a == b);
    public static bool operator <(Area a, Area b) => a._squareMeters < b._squareMeters;
    public static bool operator >(Area a, Area b) => a._squareMeters > b._squareMeters;
    public static bool operator <=(Area a, Area b) => a._squareMeters <= b._squareMeters;
    public static bool operator >=(Area a, Area b) => a._squareMeters >= b._squareMeters;

    public int CompareTo(Area? other) => other is null ? 1 : _squareMeters.CompareTo(other._squareMeters);
    public bool Equals(Area? other) => other is not null && _squareMeters == other._squareMeters;
    public override bool Equals(object? obj) => obj is Area other && Equals(other);
    public override int GetHashCode() => _squareMeters.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:(?:mm|cm|km|m)(?:²|2)|ha\b|hectare\b|hectares\b|hektar\b|acre\b|acres\b|sq\s*ft\b|sq\s*in\b|sq\s*yd\b|kvadrat(?:millimeter|centimeter|meter|kilometer|fot|tum|yard)\b)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like area values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Area>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Area>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var area)) continue;
            results.Add(new TextCandidate<Area>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Area), TextCandidateCategory.Measurement,
                area!.ToNormalizedString(), area.ToString(),
                area.ToMaskedString(),
                TextMatchConfidence.Medium,
                area));
        }
        return results;
    }
}
