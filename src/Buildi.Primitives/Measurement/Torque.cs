using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A torque value stored internally in newton-meters. Supports parsing from multiple unit formats
/// (e.g. <c>250 Nm</c>, <c>100 ft-lb</c>, <c>10 kgf-m</c>) and conversion between SI and common units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — newton meter</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Torque : IComparable<Torque>, IEquatable<Torque>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Torque", "Vridmoment", "🔧", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _newtonMeters;
    private readonly TorqueUnit _originalUnit;

    private Torque(decimal newtonMeters, TorqueUnit originalUnit)
    {
        _newtonMeters = newtonMeters;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in millinewton-meters (mNm), e.g. <c>250000</c> for 250 Nm.</summary>
    public decimal MillinewtonMeters => _newtonMeters / TorqueUnit.MillinewtonMeter.ToBaseUnitFactor;
    /// <summary>The value in newton-meters, e.g. <c>250</c>.</summary>
    public decimal NewtonMeters => _newtonMeters;
    /// <summary>The value in kilonewton-meters (kNm), e.g. <c>0.25</c> for 250 Nm.</summary>
    public decimal KilonewtonMeters => _newtonMeters / TorqueUnit.KilonewtonMeter.ToBaseUnitFactor;

    /// <summary>The value in foot-pounds.</summary>
    public decimal FootPounds => _newtonMeters / TorqueUnit.FootPound.ToBaseUnitFactor;

    /// <summary>The value in kilogram-force meters.</summary>
    public decimal KilogramForceMeters => _newtonMeters / TorqueUnit.KilogramForceMeter.ToBaseUnitFactor;

    /// <summary>The value in inch-pounds.</summary>
    public decimal InchPounds => _newtonMeters / TorqueUnit.InchPound.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public TorqueUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(TorqueUnit unit) => _newtonMeters / unit.ToBaseUnitFactor;

    public static Torque FromMillinewtonMeters(decimal mnm) => new(mnm * TorqueUnit.MillinewtonMeter.ToBaseUnitFactor, TorqueUnit.MillinewtonMeter);
    public static Torque FromNewtonMeters(decimal nm) => new(nm, TorqueUnit.NewtonMeter);
    public static Torque FromKilonewtonMeters(decimal knm) => new(knm * TorqueUnit.KilonewtonMeter.ToBaseUnitFactor, TorqueUnit.KilonewtonMeter);
    public static Torque FromFootPounds(decimal ftLb) => new(ftLb * TorqueUnit.FootPound.ToBaseUnitFactor, TorqueUnit.FootPound);
    public static Torque FromKilogramForceMeters(decimal kgfM) =>
        new(kgfM * TorqueUnit.KilogramForceMeter.ToBaseUnitFactor, TorqueUnit.KilogramForceMeter);
    public static Torque FromInchPounds(decimal inLb) => new(inLb * TorqueUnit.InchPound.ToBaseUnitFactor, TorqueUnit.InchPound);

    /// <summary>Creates a <see cref="Torque"/> from a value and unit.</summary>
    public static Torque Create(decimal value, TorqueUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Torque? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!TorqueUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Torque(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Torque Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid torque.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>50 Nm</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, TorqueUnit? unit = null, int? decimals = null)
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
    /// Returns the value in newton-meters as an invariant string, e.g. <c>250 Nm</c>.
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
    /// Returns the value in newton-meters with invariant formatting, e.g. <c>250 Nm</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_newtonMeters);
        return $"{formatted} {TorqueUnit.NewtonMeter.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>100 ft-lb</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>100 Nm</c> when that is the chosen unit.
    /// </summary>
    public string ToString(TorqueUnit unit, int? decimals = null)
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

    // --- Arithmetic operators ---

    public static Torque operator +(Torque a, Torque b) => new(a._newtonMeters + b._newtonMeters, a._originalUnit);
    public static Torque operator -(Torque a, Torque b) => new(a._newtonMeters - b._newtonMeters, a._originalUnit);
    public static Torque operator *(Torque a, decimal factor) => new(a._newtonMeters * factor, a._originalUnit);
    public static Torque operator *(decimal factor, Torque a) => new(a._newtonMeters * factor, a._originalUnit);
    public static Torque operator /(Torque a, decimal divisor) => new(a._newtonMeters / divisor, a._originalUnit);
    public static Torque operator -(Torque a) => new(-a._newtonMeters, a._originalUnit);

    public static bool operator ==(Torque? a, Torque? b) => a?._newtonMeters == b?._newtonMeters;
    public static bool operator !=(Torque? a, Torque? b) => !(a == b);
    public static bool operator <(Torque a, Torque b) => a._newtonMeters < b._newtonMeters;
    public static bool operator >(Torque a, Torque b) => a._newtonMeters > b._newtonMeters;
    public static bool operator <=(Torque a, Torque b) => a._newtonMeters <= b._newtonMeters;
    public static bool operator >=(Torque a, Torque b) => a._newtonMeters >= b._newtonMeters;

    public int CompareTo(Torque? other) => other is null ? 1 : _newtonMeters.CompareTo(other._newtonMeters);
    public bool Equals(Torque? other) => other is not null && _newtonMeters == other._newtonMeters;
    public override bool Equals(object? obj) => obj is Torque other && Equals(other);
    public override int GetHashCode() => _newtonMeters.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:foot-pounds|foot-pound|inch-pounds|inch-pound|kilogramkraftmeter|kilonewton[\s-]?meters?|millinewton[\s-]?meters?|newtonmeter|fotpund|tumpund|kgf-m|kgf·m|kgfm|ft-lb|ft·lb|in-lb|in·lb|kN·m|kNm|mN·m|mNm|N·m|Nm)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like torque values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Torque>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Torque>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var torque)) continue;
            results.Add(new TextCandidate<Torque>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Torque), TextCandidateCategory.Measurement,
                torque!.ToNormalizedString(), torque.ToString(),
                torque.ToMaskedString(),
                TextMatchConfidence.Medium,
                torque));
        }
        return results;
    }
}
