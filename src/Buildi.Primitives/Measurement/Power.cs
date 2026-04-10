using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// An electrical or mechanical power value stored internally in watts. Supports parsing from multiple unit formats
/// (e.g. <c>2.5 kW</c>, <c>100 mW</c>, <c>150 HP</c>) and conversion between SI units and horsepower.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — watt definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Horsepower">Wikipedia — Horsepower</see> — mechanical horsepower conversion</description></item>
/// </list>
/// </remarks>
public sealed class Power : IComparable<Power>, IEquatable<Power>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Power", "Effekt", "💡", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Horsepower"]);

    private readonly decimal _watts;
    private readonly PowerUnit _originalUnit;

    private Power(decimal watts, PowerUnit originalUnit)
    {
        _watts = watts;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in microwatts (µW), e.g. <c>1000000</c> for 1 W.</summary>
    public decimal Microwatts => _watts / PowerUnit.Microwatt.ToBaseUnitFactor;
    /// <summary>The value in milliwatts, e.g. <c>1000</c> for 1 W.</summary>
    public decimal Milliwatts => _watts / PowerUnit.Milliwatt.ToBaseUnitFactor;
    /// <summary>The value in watts, e.g. <c>1000</c>.</summary>
    public decimal Watts => _watts;
    /// <summary>The value in kilowatts, e.g. <c>1</c> for 1000 W.</summary>
    public decimal Kilowatts => _watts / PowerUnit.Kilowatt.ToBaseUnitFactor;
    /// <summary>The value in megawatts.</summary>
    public decimal Megawatts => _watts / PowerUnit.Megawatt.ToBaseUnitFactor;
    /// <summary>The value in gigawatts.</summary>
    public decimal Gigawatts => _watts / PowerUnit.Gigawatt.ToBaseUnitFactor;
    /// <summary>The value in terawatts.</summary>
    public decimal Terawatts => _watts / PowerUnit.Terawatt.ToBaseUnitFactor;
    /// <summary>The value in mechanical horsepower (approx. 745.7 W per HP).</summary>
    public decimal Horsepower => _watts / PowerUnit.Horsepower.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public PowerUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(PowerUnit unit) => _watts / unit.ToBaseUnitFactor;

    public static Power FromMicrowatts(decimal uw) => new(uw * PowerUnit.Microwatt.ToBaseUnitFactor, PowerUnit.Microwatt);
    public static Power FromMilliwatts(decimal mw) => new(mw * PowerUnit.Milliwatt.ToBaseUnitFactor, PowerUnit.Milliwatt);
    public static Power FromWatts(decimal w) => new(w, PowerUnit.Watt);
    public static Power FromKilowatts(decimal kw) => new(kw * PowerUnit.Kilowatt.ToBaseUnitFactor, PowerUnit.Kilowatt);
    public static Power FromMegawatts(decimal megawatts) => new(megawatts * PowerUnit.Megawatt.ToBaseUnitFactor, PowerUnit.Megawatt);
    public static Power FromGigawatts(decimal gw) => new(gw * PowerUnit.Gigawatt.ToBaseUnitFactor, PowerUnit.Gigawatt);
    public static Power FromTerawatts(decimal tw) => new(tw * PowerUnit.Terawatt.ToBaseUnitFactor, PowerUnit.Terawatt);
    public static Power FromHorsepower(decimal hp) => new(hp * PowerUnit.Horsepower.ToBaseUnitFactor, PowerUnit.Horsepower);

    /// <summary>Creates a <see cref="Power"/> from a value and unit.</summary>
    public static Power Create(decimal value, PowerUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Power? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!PowerUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Power(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Power Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid power.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>2.5 kW</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, PowerUnit? unit = null, int? decimals = null)
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
    /// Returns the value in SI base unit (watts) as an invariant string, e.g. <c>2500 W</c>.
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
    /// Returns the value in watts with invariant formatting, e.g. <c>2500 W</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_watts);
        return $"{formatted} {PowerUnit.Watt.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>2.5 kW</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>1000 W</c>.
    /// </summary>
    public string ToString(PowerUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable SI unit for this value, e.g. kW for 1500 watts.
    /// </summary>
    public PowerUnit NaturalUnit => PowerUnit.GetNatural(_watts);

    /// <summary>
    /// Returns the value formatted in the most human-readable SI unit, e.g. <c>1.5 kW</c> instead of <c>1500 W</c>.
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

    public static Power operator +(Power a, Power b) => new(a._watts + b._watts, a._originalUnit);
    public static Power operator -(Power a, Power b) => new(a._watts - b._watts, a._originalUnit);
    public static Power operator *(Power a, decimal factor) => new(a._watts * factor, a._originalUnit);
    public static Power operator *(decimal factor, Power a) => new(a._watts * factor, a._originalUnit);
    public static Power operator /(Power a, decimal divisor) => new(a._watts / divisor, a._originalUnit);
    public static Power operator -(Power a) => new(-a._watts, a._originalUnit);

    public static bool operator ==(Power? a, Power? b) => a?._watts == b?._watts;
    public static bool operator !=(Power? a, Power? b) => !(a == b);
    public static bool operator <(Power a, Power b) => a._watts < b._watts;
    public static bool operator >(Power a, Power b) => a._watts > b._watts;
    public static bool operator <=(Power a, Power b) => a._watts <= b._watts;
    public static bool operator >=(Power a, Power b) => a._watts >= b._watts;

    public int CompareTo(Power? other) => other is null ? 1 : _watts.CompareTo(other._watts);
    public bool Equals(Power? other) => other is not null && _watts == other._watts;
    public override bool Equals(object? obj) => obj is Power other && Equals(other);
    public override int GetHashCode() => _watts.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:µW|uW|mW|MW|TW|(?i:kW|GW|W|hp|horsepower|hästkraft|hästkrafter|microwatt|mikrowatt|milliwatt|megawatt|gigawatt|terawatt|kilowatt|watt))\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for substrings that look like power values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Power>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Power>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var power)) continue;
            results.Add(new TextCandidate<Power>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Power), TextCandidateCategory.Measurement,
                power!.ToNormalizedString(), power.ToString(),
                power.ToMaskedString(),
                TextMatchConfidence.Medium,
                power));
        }
        return results;
    }
}
