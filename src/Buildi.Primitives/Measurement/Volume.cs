using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A volume value stored internally in liters. Supports parsing from multiple unit formats
/// (e.g. <c>2 L</c>, <c>500 mL</c>, <c>1 gal</c>) and conversion between metric and US customary units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — liter</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Volume : IComparable<Volume>, IEquatable<Volume>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Volume", "Volym", "🧪", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _liters;
    private readonly VolumeUnit _originalUnit;

    private Volume(decimal liters, VolumeUnit originalUnit)
    {
        _liters = liters;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in microliters (µL), e.g. <c>2000000</c> for 2 L.</summary>
    public decimal Microliters => _liters / VolumeUnit.Microliter.ToBaseUnitFactor;
    /// <summary>The value in milliliters, e.g. <c>2000</c> for 2 L.</summary>
    public decimal Milliliters => _liters / VolumeUnit.Milliliter.ToBaseUnitFactor;
    /// <summary>The value in centiliters.</summary>
    public decimal Centiliters => _liters / VolumeUnit.Centiliter.ToBaseUnitFactor;
    /// <summary>The value in deciliters.</summary>
    public decimal Deciliters => _liters / VolumeUnit.Deciliter.ToBaseUnitFactor;
    /// <summary>The value in liters, e.g. <c>2</c>.</summary>
    public decimal Liters => _liters;
    /// <summary>The value in hectoliters, e.g. <c>0.02</c> for 2 L.</summary>
    public decimal Hectoliters => _liters / VolumeUnit.Hectoliter.ToBaseUnitFactor;
    /// <summary>The value in cubic meters, e.g. <c>0.002</c> for 2 L.</summary>
    public decimal CubicMeters => _liters / VolumeUnit.CubicMeter.ToBaseUnitFactor;
    /// <summary>The value in US gallons.</summary>
    public decimal Gallons => _liters / VolumeUnit.Gallon.ToBaseUnitFactor;
    /// <summary>The value in US pints.</summary>
    public decimal Pints => _liters / VolumeUnit.Pint.ToBaseUnitFactor;
    /// <summary>The value in US fluid ounces.</summary>
    public decimal FluidOunces => _liters / VolumeUnit.FluidOunce.ToBaseUnitFactor;
    /// <summary>The value in US cups.</summary>
    public decimal Cups => _liters / VolumeUnit.Cup.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public VolumeUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(VolumeUnit unit) => _liters / unit.ToBaseUnitFactor;

    public static Volume FromMicroliters(decimal ul) => new(ul * VolumeUnit.Microliter.ToBaseUnitFactor, VolumeUnit.Microliter);
    public static Volume FromMilliliters(decimal ml) => new(ml * VolumeUnit.Milliliter.ToBaseUnitFactor, VolumeUnit.Milliliter);
    public static Volume FromCentiliters(decimal cl) => new(cl * VolumeUnit.Centiliter.ToBaseUnitFactor, VolumeUnit.Centiliter);
    public static Volume FromDeciliters(decimal dl) => new(dl * VolumeUnit.Deciliter.ToBaseUnitFactor, VolumeUnit.Deciliter);
    public static Volume FromLiters(decimal l) => new(l, VolumeUnit.Liter);
    public static Volume FromHectoliters(decimal hl) => new(hl * VolumeUnit.Hectoliter.ToBaseUnitFactor, VolumeUnit.Hectoliter);
    public static Volume FromCubicMeters(decimal m3) => new(m3 * VolumeUnit.CubicMeter.ToBaseUnitFactor, VolumeUnit.CubicMeter);
    public static Volume FromGallons(decimal gal) => new(gal * VolumeUnit.Gallon.ToBaseUnitFactor, VolumeUnit.Gallon);
    public static Volume FromPints(decimal pt) => new(pt * VolumeUnit.Pint.ToBaseUnitFactor, VolumeUnit.Pint);
    public static Volume FromFluidOunces(decimal flOz) => new(flOz * VolumeUnit.FluidOunce.ToBaseUnitFactor, VolumeUnit.FluidOunce);
    public static Volume FromCups(decimal cup) => new(cup * VolumeUnit.Cup.ToBaseUnitFactor, VolumeUnit.Cup);

    /// <summary>Creates a <see cref="Volume"/> from a value and unit.</summary>
    public static Volume Create(decimal value, VolumeUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Volume? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!VolumeUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Volume(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Volume Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid volume.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>2 L</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, VolumeUnit? unit = null, int? decimals = null)
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
    /// Returns the value in liters as an invariant string, e.g. <c>2 L</c>.
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
    /// Returns the value in liters with invariant formatting, e.g. <c>2 L</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_liters);
        return $"{formatted} {VolumeUnit.Liter.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>500 mL</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>2000 mL</c>.
    /// </summary>
    public string ToString(VolumeUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable metric unit for this value, e.g. L for 2.5 liters.
    /// </summary>
    public VolumeUnit NaturalUnit => VolumeUnit.GetNatural(_liters);

    /// <summary>
    /// Returns the value formatted in the most human-readable metric unit, e.g. <c>2.5 L</c> instead of <c>2500 mL</c>.
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

    public static Volume operator +(Volume a, Volume b) => new(a._liters + b._liters, a._originalUnit);
    public static Volume operator -(Volume a, Volume b) => new(a._liters - b._liters, a._originalUnit);
    public static Volume operator *(Volume a, decimal factor) => new(a._liters * factor, a._originalUnit);
    public static Volume operator *(decimal factor, Volume a) => new(a._liters * factor, a._originalUnit);
    public static Volume operator /(Volume a, decimal divisor) => new(a._liters / divisor, a._originalUnit);
    public static Volume operator -(Volume a) => new(-a._liters, a._originalUnit);

    public static bool operator ==(Volume? a, Volume? b) => a?._liters == b?._liters;
    public static bool operator !=(Volume? a, Volume? b) => !(a == b);
    public static bool operator <(Volume a, Volume b) => a._liters < b._liters;
    public static bool operator >(Volume a, Volume b) => a._liters > b._liters;
    public static bool operator <=(Volume a, Volume b) => a._liters <= b._liters;
    public static bool operator >=(Volume a, Volume b) => a._liters >= b._liters;

    public int CompareTo(Volume? other) => other is null ? 1 : _liters.CompareTo(other._liters);
    public bool Equals(Volume? other) => other is not null && _liters == other._liters;
    public override bool Equals(object? obj) => obj is Volume other && Equals(other);
    public override int GetHashCode() => _liters.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:kubikmeter|hektoliter|mikroliter|vätskeans|fluid\s+ounces?|fl\s*oz|gallons?|microliters?|milliliters?|centiliters?|deciliters?|hectoliters?|liters?|cubic\s+meters?|koppar|kopp|cups?|pints?|m³|m3|cbm|µL|µl|ul|ml|cl|dl|hL|hl|gal|pt|l)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like volume values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Volume>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Volume>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var volume)) continue;
            results.Add(new TextCandidate<Volume>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Volume), TextCandidateCategory.Measurement,
                volume!.ToNormalizedString(), volume.ToString(),
                volume.ToMaskedString(),
                TextMatchConfidence.Medium,
                volume));
        }
        return results;
    }
}
