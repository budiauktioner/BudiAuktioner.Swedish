using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A mass (weight) value stored internally in kilograms. Supports parsing from multiple unit formats
/// (e.g. <c>10 kg</c>, <c>500 g</c>, <c>5 lb</c>) and conversion between metric and imperial units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-base-units">BIPM SI base units</see> — kilogram definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Weight : IComparable<Weight>, IEquatable<Weight>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Weight", "Vikt", "⚖️", ["https://www.bipm.org/en/measurement-units/si-base-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _kilograms;
    private readonly WeightUnit _originalUnit;

    private Weight(decimal kilograms, WeightUnit originalUnit)
    {
        _kilograms = kilograms;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in micrograms (µg), e.g. <c>1000000000</c> for 1 kg.</summary>
    public decimal Micrograms => _kilograms / WeightUnit.Microgram.ToBaseUnitFactor;
    /// <summary>The value in milligrams, e.g. <c>1000000</c> for 1 kg.</summary>
    public decimal Milligrams => _kilograms / WeightUnit.Milligram.ToBaseUnitFactor;
    /// <summary>The value in grams, e.g. <c>1000</c> for 1 kg.</summary>
    public decimal Grams => _kilograms / WeightUnit.Gram.ToBaseUnitFactor;
    /// <summary>The value in hectograms (hekto), e.g. <c>10</c> for 1 kg.</summary>
    public decimal Hectograms => _kilograms / WeightUnit.Hectogram.ToBaseUnitFactor;
    /// <summary>The value in kilograms, e.g. <c>1</c>.</summary>
    public decimal Kilograms => _kilograms;
    /// <summary>The value in metric tons, e.g. <c>0.001</c> for 1 kg.</summary>
    public decimal MetricTons => _kilograms / WeightUnit.MetricTon.ToBaseUnitFactor;
    /// <summary>The value in pounds.</summary>
    public decimal Pounds => _kilograms / WeightUnit.Pound.ToBaseUnitFactor;
    /// <summary>The value in ounces.</summary>
    public decimal Ounces => _kilograms / WeightUnit.Ounce.ToBaseUnitFactor;
    /// <summary>The value in stones.</summary>
    public decimal Stones => _kilograms / WeightUnit.Stone.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public WeightUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(WeightUnit unit) => _kilograms / unit.ToBaseUnitFactor;

    public static Weight FromMicrograms(decimal ug) => new(ug * WeightUnit.Microgram.ToBaseUnitFactor, WeightUnit.Microgram);
    public static Weight FromMilligrams(decimal mg) => new(mg * WeightUnit.Milligram.ToBaseUnitFactor, WeightUnit.Milligram);
    public static Weight FromGrams(decimal g) => new(g * WeightUnit.Gram.ToBaseUnitFactor, WeightUnit.Gram);
    public static Weight FromHectograms(decimal hg) => new(hg * WeightUnit.Hectogram.ToBaseUnitFactor, WeightUnit.Hectogram);
    public static Weight FromKilograms(decimal kg) => new(kg, WeightUnit.Kilogram);
    public static Weight FromMetricTons(decimal t) => new(t * WeightUnit.MetricTon.ToBaseUnitFactor, WeightUnit.MetricTon);
    public static Weight FromPounds(decimal lb) => new(lb * WeightUnit.Pound.ToBaseUnitFactor, WeightUnit.Pound);
    public static Weight FromOunces(decimal oz) => new(oz * WeightUnit.Ounce.ToBaseUnitFactor, WeightUnit.Ounce);
    public static Weight FromStones(decimal st) => new(st * WeightUnit.Stone.ToBaseUnitFactor, WeightUnit.Stone);

    /// <summary>Creates a <see cref="Weight"/> from a value and unit.</summary>
    public static Weight Create(decimal value, WeightUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Weight? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!WeightUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Weight(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Weight Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid weight.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>10 kg</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, WeightUnit? unit = null, int? decimals = null)
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
    /// Returns the value in SI base unit (kilograms) as an invariant string, e.g. <c>10 kg</c>.
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
    /// Returns the value in kilograms with invariant formatting, e.g. <c>10 kg</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_kilograms);
        return $"{formatted} {WeightUnit.Kilogram.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>10 kg</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>1000 g</c>.
    /// </summary>
    public string ToString(WeightUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable metric unit for this value, e.g. kg for 1.5 kilograms.
    /// </summary>
    public WeightUnit NaturalUnit => WeightUnit.GetNatural(_kilograms);

    /// <summary>
    /// Returns the value formatted in the most human-readable metric unit, e.g. <c>1.5 kg</c> instead of <c>1500 g</c>.
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

    public static Weight operator +(Weight a, Weight b) => new(a._kilograms + b._kilograms, a._originalUnit);
    public static Weight operator -(Weight a, Weight b) => new(a._kilograms - b._kilograms, a._originalUnit);
    public static Weight operator *(Weight a, decimal factor) => new(a._kilograms * factor, a._originalUnit);
    public static Weight operator *(decimal factor, Weight a) => new(a._kilograms * factor, a._originalUnit);
    public static Weight operator /(Weight a, decimal divisor) => new(a._kilograms / divisor, a._originalUnit);
    public static Weight operator -(Weight a) => new(-a._kilograms, a._originalUnit);

    public static bool operator ==(Weight? a, Weight? b) => a?._kilograms == b?._kilograms;
    public static bool operator !=(Weight? a, Weight? b) => !(a == b);
    public static bool operator <(Weight a, Weight b) => a._kilograms < b._kilograms;
    public static bool operator >(Weight a, Weight b) => a._kilograms > b._kilograms;
    public static bool operator <=(Weight a, Weight b) => a._kilograms <= b._kilograms;
    public static bool operator >=(Weight a, Weight b) => a._kilograms >= b._kilograms;

    public int CompareTo(Weight? other) => other is null ? 1 : _kilograms.CompareTo(other._kilograms);
    public bool Equals(Weight? other) => other is not null && _kilograms == other._kilograms;
    public override bool Equals(object? obj) => obj is Weight other && Equals(other);
    public override int GetHashCode() => _kilograms.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:metric\s+ton|microgram|milligram|hectogram|hektogram|kilogram|gram|tonne|ton|ounces|ounce|pounds|pound|stones|stone|hekto|lbs|lb|µg|ug|mg|hg|kg|g|oz|st|t|pund|uns)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like weight values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Weight>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Weight>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var weight)) continue;
            results.Add(new TextCandidate<Weight>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Weight), TextCandidateCategory.Measurement,
                weight!.ToNormalizedString(), weight.ToString(),
                weight.ToMaskedString(),
                TextMatchConfidence.Medium,
                weight));
        }
        return results;
    }
}
