using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// An electric potential (voltage) value stored internally in volts. Supports parsing from common SI prefixes
/// (e.g. <c>3.3 V</c>, <c>500 mV</c>, <c>230 kV</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — volt definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Volt">Wikipedia — Volt</see></description></item>
/// </list>
/// </remarks>
public sealed class Voltage : IComparable<Voltage>, IEquatable<Voltage>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Voltage", "Spänning", "🔌", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Volt"]);

    private readonly decimal _volts;
    private readonly VoltageUnit _originalUnit;

    private Voltage(decimal volts, VoltageUnit originalUnit)
    {
        _volts = volts;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in microvolts (µV), e.g. <c>1000000</c> for 1 V.</summary>
    public decimal Microvolts => _volts / VoltageUnit.Microvolt.ToBaseUnitFactor;
    /// <summary>The value in millivolts, e.g. <c>1000</c> for 1 V.</summary>
    public decimal Millivolts => _volts / VoltageUnit.Millivolt.ToBaseUnitFactor;
    /// <summary>The value in volts, e.g. <c>3.3</c>.</summary>
    public decimal Volts => _volts;
    /// <summary>The value in kilovolts, e.g. <c>0.23</c> for 230 V.</summary>
    public decimal Kilovolts => _volts / VoltageUnit.Kilovolt.ToBaseUnitFactor;
    /// <summary>The value in megavolts.</summary>
    public decimal Megavolts => _volts / VoltageUnit.Megavolt.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public VoltageUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(VoltageUnit unit) => _volts / unit.ToBaseUnitFactor;

    public static Voltage FromMicrovolts(decimal uv) => new(uv * VoltageUnit.Microvolt.ToBaseUnitFactor, VoltageUnit.Microvolt);
    public static Voltage FromMillivolts(decimal mv) => new(mv * VoltageUnit.Millivolt.ToBaseUnitFactor, VoltageUnit.Millivolt);
    public static Voltage FromVolts(decimal v) => new(v, VoltageUnit.Volt);
    public static Voltage FromKilovolts(decimal kv) => new(kv * VoltageUnit.Kilovolt.ToBaseUnitFactor, VoltageUnit.Kilovolt);
    public static Voltage FromMegavolts(decimal mv) => new(mv * VoltageUnit.Megavolt.ToBaseUnitFactor, VoltageUnit.Megavolt);

    /// <summary>Creates a <see cref="Voltage"/> from a value and unit.</summary>
    public static Voltage Create(decimal value, VoltageUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Voltage? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!VoltageUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Voltage(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Voltage Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid voltage.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>230 kV</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, VoltageUnit? unit = null, int? decimals = null)
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
    /// Returns the value in volts as an invariant string, e.g. <c>230000 V</c>.
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
    /// Returns the value in volts with invariant formatting, e.g. <c>3.3 V</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_volts);
        return $"{formatted} {VoltageUnit.Volt.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>230 kV</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>3300 mV</c>.
    /// </summary>
    public string ToString(VoltageUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable SI unit for this value, e.g. kV for 1500 volts.
    /// </summary>
    public VoltageUnit NaturalUnit => VoltageUnit.GetNatural(_volts);

    /// <summary>
    /// Returns the value formatted in the most human-readable SI unit, e.g. <c>1.5 kV</c> instead of <c>1500 V</c>.
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

    public static Voltage operator +(Voltage a, Voltage b) => new(a._volts + b._volts, a._originalUnit);
    public static Voltage operator -(Voltage a, Voltage b) => new(a._volts - b._volts, a._originalUnit);
    public static Voltage operator *(Voltage a, decimal factor) => new(a._volts * factor, a._originalUnit);
    public static Voltage operator *(decimal factor, Voltage a) => new(a._volts * factor, a._originalUnit);
    public static Voltage operator /(Voltage a, decimal divisor) => new(a._volts / divisor, a._originalUnit);
    public static Voltage operator -(Voltage a) => new(-a._volts, a._originalUnit);

    public static bool operator ==(Voltage? a, Voltage? b) => a?._volts == b?._volts;
    public static bool operator !=(Voltage? a, Voltage? b) => !(a == b);
    public static bool operator <(Voltage a, Voltage b) => a._volts < b._volts;
    public static bool operator >(Voltage a, Voltage b) => a._volts > b._volts;
    public static bool operator <=(Voltage a, Voltage b) => a._volts <= b._volts;
    public static bool operator >=(Voltage a, Voltage b) => a._volts >= b._volts;

    public int CompareTo(Voltage? other) => other is null ? 1 : _volts.CompareTo(other._volts);
    public bool Equals(Voltage? other) => other is not null && _volts == other._volts;
    public override bool Equals(object? obj) => obj is Voltage other && Equals(other);
    public override int GetHashCode() => _volts.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:µV|uV|mV|kV|MV|V)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like voltage values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Voltage>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Voltage>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var voltage)) continue;
            results.Add(new TextCandidate<Voltage>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Voltage), TextCandidateCategory.Measurement,
                voltage!.ToNormalizedString(), voltage.ToString(),
                voltage.ToMaskedString(),
                TextMatchConfidence.Medium,
                voltage));
        }
        return results;
    }
}
