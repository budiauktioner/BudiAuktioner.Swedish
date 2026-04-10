using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A frequency value stored internally in hertz. Supports parsing from multiple unit formats
/// (e.g. <c>2.4 GHz</c>, <c>60 Hz</c>, <c>3000 rpm</c>) and conversion between SI and rotational speed.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — hertz definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Frequency : IComparable<Frequency>, IEquatable<Frequency>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Frequency", "Frekvens", "📡", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _hertz;
    private readonly FrequencyUnit _originalUnit;

    private Frequency(decimal hertz, FrequencyUnit originalUnit)
    {
        _hertz = hertz;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in hertz, e.g. <c>60</c>.</summary>
    public decimal Hertz => _hertz;
    /// <summary>The value in kilohertz.</summary>
    public decimal Kilohertz => _hertz / FrequencyUnit.Kilohertz.ToBaseUnitFactor;
    /// <summary>The value in megahertz.</summary>
    public decimal Megahertz => _hertz / FrequencyUnit.Megahertz.ToBaseUnitFactor;
    /// <summary>The value in gigahertz.</summary>
    public decimal Gigahertz => _hertz / FrequencyUnit.Gigahertz.ToBaseUnitFactor;
    /// <summary>The value in terahertz.</summary>
    public decimal Terahertz => _hertz / FrequencyUnit.Terahertz.ToBaseUnitFactor;
    /// <summary>The value in revolutions per minute (RPM).</summary>
    public decimal Rpm => HertzToValueInUnit(_hertz, FrequencyUnit.RevolutionsPerMinute);

    /// <summary>The unit the value was originally parsed from.</summary>
    public FrequencyUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(FrequencyUnit unit) => HertzToValueInUnit(_hertz, unit);

    public static Frequency FromHertz(decimal hz) => new(hz, FrequencyUnit.Hertz);
    public static Frequency FromKilohertz(decimal khz) => new(khz * FrequencyUnit.Kilohertz.ToBaseUnitFactor, FrequencyUnit.Kilohertz);
    public static Frequency FromMegahertz(decimal mhz) => new(mhz * FrequencyUnit.Megahertz.ToBaseUnitFactor, FrequencyUnit.Megahertz);
    public static Frequency FromGigahertz(decimal ghz) => new(ghz * FrequencyUnit.Gigahertz.ToBaseUnitFactor, FrequencyUnit.Gigahertz);
    public static Frequency FromTerahertz(decimal thz) => new(thz * FrequencyUnit.Terahertz.ToBaseUnitFactor, FrequencyUnit.Terahertz);
    public static Frequency FromRpm(decimal rpm) => new(ValueInUnitToHertz(rpm, FrequencyUnit.RevolutionsPerMinute), FrequencyUnit.RevolutionsPerMinute);

    /// <summary>Creates a <see cref="Frequency"/> from a value and unit.</summary>
    public static Frequency Create(decimal value, FrequencyUnit unit) => new(ValueInUnitToHertz(value, unit), unit);

    public static bool TryParse(string? input, out Frequency? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!FrequencyUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Frequency(ValueInUnitToHertz(value, unit), unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Frequency Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid frequency.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>50 Hz</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, FrequencyUnit? unit = null, int? decimals = null)
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
    /// Returns the value in SI base unit (hertz) as an invariant string, e.g. <c>2400000000 Hz</c>.
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
    /// Returns the value in hertz with invariant formatting, e.g. <c>60 Hz</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_hertz);
        return $"{formatted} {FrequencyUnit.Hertz.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>2.4 GHz</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>2400 MHz</c>.
    /// </summary>
    public string ToString(FrequencyUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable SI unit for this value, e.g. GHz for 3,500,000,000 Hz.
    /// </summary>
    public FrequencyUnit NaturalUnit => FrequencyUnit.GetNatural(_hertz);

    /// <summary>
    /// Returns the value formatted in the most human-readable SI unit, e.g. <c>3.5 GHz</c> instead of <c>3500000000 Hz</c>.
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

    private static decimal ValueInUnitToHertz(decimal value, FrequencyUnit unit) =>
        unit == FrequencyUnit.RevolutionsPerMinute ? value / 60m : value * unit.ToBaseUnitFactor;

    private static decimal HertzToValueInUnit(decimal hertz, FrequencyUnit unit) =>
        unit == FrequencyUnit.RevolutionsPerMinute ? hertz * 60m : hertz / unit.ToBaseUnitFactor;

    // --- Arithmetic operators ---

    public static Frequency operator +(Frequency a, Frequency b) => new(a._hertz + b._hertz, a._originalUnit);
    public static Frequency operator -(Frequency a, Frequency b) => new(a._hertz - b._hertz, a._originalUnit);
    public static Frequency operator *(Frequency a, decimal factor) => new(a._hertz * factor, a._originalUnit);
    public static Frequency operator *(decimal factor, Frequency a) => new(a._hertz * factor, a._originalUnit);
    public static Frequency operator /(Frequency a, decimal divisor) => new(a._hertz / divisor, a._originalUnit);
    public static Frequency operator -(Frequency a) => new(-a._hertz, a._originalUnit);

    public static bool operator ==(Frequency? a, Frequency? b) => a?._hertz == b?._hertz;
    public static bool operator !=(Frequency? a, Frequency? b) => !(a == b);
    public static bool operator <(Frequency a, Frequency b) => a._hertz < b._hertz;
    public static bool operator >(Frequency a, Frequency b) => a._hertz > b._hertz;
    public static bool operator <=(Frequency a, Frequency b) => a._hertz <= b._hertz;
    public static bool operator >=(Frequency a, Frequency b) => a._hertz >= b._hertz;

    public int CompareTo(Frequency? other) => other is null ? 1 : _hertz.CompareTo(other._hertz);
    public bool Equals(Frequency? other) => other is not null && _hertz == other._hertz;
    public override bool Equals(object? obj) => obj is Frequency other && Equals(other);
    public override int GetHashCode() => _hertz.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:THz|GHz|MHz|kHz|Hz|rpm|rev/min|varv/min|r/min)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like frequency values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Frequency>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Frequency>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var frequency)) continue;
            results.Add(new TextCandidate<Frequency>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Frequency), TextCandidateCategory.Measurement,
                frequency!.ToNormalizedString(), frequency.ToString(),
                frequency.ToMaskedString(),
                TextMatchConfidence.Medium,
                frequency));
        }
        return results;
    }
}
