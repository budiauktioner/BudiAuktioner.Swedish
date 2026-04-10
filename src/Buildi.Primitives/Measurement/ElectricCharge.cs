using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// An electric charge value stored internally in ampere-hours. Supports parsing from common units
/// (e.g. <c>5000 mAh</c>, <c>2.5 Ah</c>, <c>3600 C</c>) and conversion between milliampere-hours,
/// ampere-hours, and coulombs.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — coulomb definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Ampere-hour">Wikipedia — Ampere hour</see> — relation to coulomb</description></item>
/// </list>
/// </remarks>
public sealed class ElectricCharge : IComparable<ElectricCharge>, IEquatable<ElectricCharge>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Electric Charge", "Elektrisk laddning", "🔋", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Ampere-hour"]);

    private readonly decimal _ampereHours;
    private readonly ElectricChargeUnit _originalUnit;

    private ElectricCharge(decimal ampereHours, ElectricChargeUnit originalUnit)
    {
        _ampereHours = ampereHours;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in milliampere-hours, e.g. <c>5000</c> for 5 Ah.</summary>
    public decimal MilliampereHours => _ampereHours / ElectricChargeUnit.MilliampereHour.ToBaseUnitFactor;

    /// <summary>The value in ampere-hours, e.g. <c>5</c>.</summary>
    public decimal AmpereHours => _ampereHours;

    /// <summary>The value in coulombs, e.g. <c>18000</c> for 5 Ah.</summary>
    public decimal Coulombs => _ampereHours * 3600m;

    /// <summary>The unit the value was originally parsed from.</summary>
    public ElectricChargeUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(ElectricChargeUnit unit) => unit.FromAmpereHours(_ampereHours);

    public static ElectricCharge FromMilliampereHours(decimal mah) =>
        new(ElectricChargeUnit.MilliampereHour.ToAmpereHours(mah), ElectricChargeUnit.MilliampereHour);

    public static ElectricCharge FromAmpereHours(decimal ah) => new(ah, ElectricChargeUnit.AmpereHour);

    public static ElectricCharge FromCoulombs(decimal c) =>
        new(ElectricChargeUnit.Coulomb.ToAmpereHours(c), ElectricChargeUnit.Coulomb);

    /// <summary>Creates an <see cref="ElectricCharge"/> from a value and unit.</summary>
    public static ElectricCharge Create(decimal value, ElectricChargeUnit unit) =>
        new(unit.ToAmpereHours(value), unit);

    public static bool TryParse(string? input, out ElectricCharge? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!ElectricChargeUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        result = new ElectricCharge(unit.ToAmpereHours(value), unit);
        return true;
    }

    public static ElectricCharge Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid electric charge.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>5000 mAh</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, ElectricChargeUnit? unit = null, int? decimals = null)
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
    /// Returns the value in ampere-hours as an invariant string, e.g. <c>5 Ah</c>.
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
    /// Returns the value in ampere-hours with invariant formatting, e.g. <c>5 Ah</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_ampereHours);
        return $"{formatted} {ElectricChargeUnit.AmpereHour.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>5000 mAh</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = _originalUnit.FromAmpereHours(_ampereHours);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>18000 C</c> for 5 Ah.
    /// </summary>
    public string ToString(ElectricChargeUnit unit, int? decimals = null)
    {
        var valueInUnit = unit.FromAmpereHours(_ampereHours);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable unit for this value, e.g. Ah for 1.5 ampere-hours.
    /// </summary>
    public ElectricChargeUnit NaturalUnit => ElectricChargeUnit.GetNatural(_ampereHours);

    /// <summary>
    /// Returns the value formatted in the most human-readable unit, e.g. <c>5000 mAh</c> for small charges.
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

    public static ElectricCharge operator +(ElectricCharge a, ElectricCharge b) =>
        new(a._ampereHours + b._ampereHours, a._originalUnit);

    public static ElectricCharge operator -(ElectricCharge a, ElectricCharge b) =>
        new(a._ampereHours - b._ampereHours, a._originalUnit);

    public static ElectricCharge operator *(ElectricCharge a, decimal factor) =>
        new(a._ampereHours * factor, a._originalUnit);

    public static ElectricCharge operator *(decimal factor, ElectricCharge a) =>
        new(a._ampereHours * factor, a._originalUnit);

    public static ElectricCharge operator /(ElectricCharge a, decimal divisor) =>
        new(a._ampereHours / divisor, a._originalUnit);

    public static ElectricCharge operator -(ElectricCharge a) => new(-a._ampereHours, a._originalUnit);

    public static bool operator ==(ElectricCharge? a, ElectricCharge? b) => a?._ampereHours == b?._ampereHours;
    public static bool operator !=(ElectricCharge? a, ElectricCharge? b) => !(a == b);
    public static bool operator <(ElectricCharge a, ElectricCharge b) => a._ampereHours < b._ampereHours;
    public static bool operator >(ElectricCharge a, ElectricCharge b) => a._ampereHours > b._ampereHours;
    public static bool operator <=(ElectricCharge a, ElectricCharge b) => a._ampereHours <= b._ampereHours;
    public static bool operator >=(ElectricCharge a, ElectricCharge b) => a._ampereHours >= b._ampereHours;

    public int CompareTo(ElectricCharge? other) => other is null ? 1 : _ampereHours.CompareTo(other._ampereHours);
    public bool Equals(ElectricCharge? other) => other is not null && _ampereHours == other._ampereHours;
    public override bool Equals(object? obj) => obj is ElectricCharge other && Equals(other);
    public override int GetHashCode() => _ampereHours.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:mAh|Ah|C)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like electric charge values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<ElectricCharge>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<ElectricCharge>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var charge)) continue;
            results.Add(new TextCandidate<ElectricCharge>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(ElectricCharge), TextCandidateCategory.Measurement,
                charge!.ToNormalizedString(), charge.ToString(),
                charge.ToMaskedString(),
                TextMatchConfidence.Medium,
                charge));
        }
        return results;
    }
}
