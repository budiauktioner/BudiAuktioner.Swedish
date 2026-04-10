using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A pressure value stored internally in pascals. Supports parsing from multiple unit formats
/// (e.g. <c>1013 hPa</c>, <c>1 bar</c>, <c>14.7 PSI</c>) and conversion between common units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — pascal</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see> — pressure</description></item>
/// </list>
/// </remarks>
public sealed class Pressure : IComparable<Pressure>, IEquatable<Pressure>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Pressure", "Tryck", "🎯", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _pascals;
    private readonly PressureUnit _originalUnit;

    private Pressure(decimal pascals, PressureUnit originalUnit)
    {
        _pascals = pascals;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in pascals, e.g. <c>100000</c> for 1 bar.</summary>
    public decimal Pascals => _pascals;
    /// <summary>The value in hectopascals, e.g. <c>1013</c> for standard air pressure.</summary>
    public decimal Hectopascals => _pascals / PressureUnit.Hectopascal.ToBaseUnitFactor;
    /// <summary>The value in kilopascals.</summary>
    public decimal Kilopascals => _pascals / PressureUnit.Kilopascal.ToBaseUnitFactor;
    /// <summary>The value in megapascals, e.g. <c>0.1</c> for 1 bar.</summary>
    public decimal Megapascals => _pascals / PressureUnit.Megapascal.ToBaseUnitFactor;
    /// <summary>The value in gigapascals.</summary>
    public decimal Gigapascals => _pascals / PressureUnit.Gigapascal.ToBaseUnitFactor;
    /// <summary>The value in bar.</summary>
    public decimal Bars => _pascals / PressureUnit.Bar.ToBaseUnitFactor;
    /// <summary>The value in millibars.</summary>
    public decimal Millibars => _pascals / PressureUnit.Millibar.ToBaseUnitFactor;
    /// <summary>The value in pounds per square inch (PSI).</summary>
    public decimal Psi => _pascals / PressureUnit.Psi.ToBaseUnitFactor;
    /// <summary>The value in standard atmospheres.</summary>
    public decimal Atmospheres => _pascals / PressureUnit.Atmosphere.ToBaseUnitFactor;
    /// <summary>The value in millimeters of mercury (mmHg).</summary>
    public decimal MillimetersOfMercury => _pascals / PressureUnit.MillimeterOfMercury.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public PressureUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(PressureUnit unit) => _pascals / unit.ToBaseUnitFactor;

    public static Pressure FromPascals(decimal pa) => new(pa, PressureUnit.Pascal);
    public static Pressure FromHectopascals(decimal hpa) => new(hpa * PressureUnit.Hectopascal.ToBaseUnitFactor, PressureUnit.Hectopascal);
    public static Pressure FromKilopascals(decimal kpa) => new(kpa * PressureUnit.Kilopascal.ToBaseUnitFactor, PressureUnit.Kilopascal);
    public static Pressure FromMegapascals(decimal mpa) => new(mpa * PressureUnit.Megapascal.ToBaseUnitFactor, PressureUnit.Megapascal);
    public static Pressure FromGigapascals(decimal gpa) => new(gpa * PressureUnit.Gigapascal.ToBaseUnitFactor, PressureUnit.Gigapascal);
    public static Pressure FromBars(decimal bar) => new(bar * PressureUnit.Bar.ToBaseUnitFactor, PressureUnit.Bar);
    public static Pressure FromMillibars(decimal mbar) => new(mbar * PressureUnit.Millibar.ToBaseUnitFactor, PressureUnit.Millibar);
    public static Pressure FromPsi(decimal psi) => new(psi * PressureUnit.Psi.ToBaseUnitFactor, PressureUnit.Psi);
    public static Pressure FromAtmospheres(decimal atm) => new(atm * PressureUnit.Atmosphere.ToBaseUnitFactor, PressureUnit.Atmosphere);
    public static Pressure FromMillimetersOfMercury(decimal mmHg) => new(mmHg * PressureUnit.MillimeterOfMercury.ToBaseUnitFactor, PressureUnit.MillimeterOfMercury);

    /// <summary>Creates a <see cref="Pressure"/> from a value and unit.</summary>
    public static Pressure Create(decimal value, PressureUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Pressure? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!PressureUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Pressure(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Pressure Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid pressure.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>1013 hPa</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, PressureUnit? unit = null, int? decimals = null)
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
    /// Returns the value in SI base-derived unit (pascals) as an invariant string, e.g. <c>101300 Pa</c>.
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
    /// Returns the value in pascals with invariant formatting, e.g. <c>101300 Pa</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_pascals);
        return $"{formatted} {PressureUnit.Pascal.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>1013 hPa</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>1.013 bar</c>.
    /// </summary>
    public string ToString(PressureUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable metric unit for this value, e.g. bar for 200,000 Pa.
    /// </summary>
    public PressureUnit NaturalUnit => PressureUnit.GetNatural(_pascals);

    /// <summary>
    /// Returns the value formatted in the most human-readable metric unit, e.g. <c>2 bar</c> instead of <c>200000 Pa</c>.
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

    public static Pressure operator +(Pressure a, Pressure b) => new(a._pascals + b._pascals, a._originalUnit);
    public static Pressure operator -(Pressure a, Pressure b) => new(a._pascals - b._pascals, a._originalUnit);
    public static Pressure operator *(Pressure a, decimal factor) => new(a._pascals * factor, a._originalUnit);
    public static Pressure operator *(decimal factor, Pressure a) => new(a._pascals * factor, a._originalUnit);
    public static Pressure operator /(Pressure a, decimal divisor) => new(a._pascals / divisor, a._originalUnit);
    public static Pressure operator -(Pressure a) => new(-a._pascals, a._originalUnit);

    public static bool operator ==(Pressure? a, Pressure? b) => a?._pascals == b?._pascals;
    public static bool operator !=(Pressure? a, Pressure? b) => !(a == b);
    public static bool operator <(Pressure a, Pressure b) => a._pascals < b._pascals;
    public static bool operator >(Pressure a, Pressure b) => a._pascals > b._pascals;
    public static bool operator <=(Pressure a, Pressure b) => a._pascals <= b._pascals;
    public static bool operator >=(Pressure a, Pressure b) => a._pascals >= b._pascals;

    public int CompareTo(Pressure? other) => other is null ? 1 : _pascals.CompareTo(other._pascals);
    public bool Equals(Pressure? other) => other is not null && _pascals == other._pascals;
    public override bool Equals(object? obj) => obj is Pressure other && Equals(other);
    public override int GetHashCode() => _pascals.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:GPa|MPa|kPa|hPa|mbar|mmHg|mm\s+Hg|lb/in²|atm|torr|bar|Pa|PSI)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like pressure values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Pressure>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Pressure>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var pressure)) continue;
            results.Add(new TextCandidate<Pressure>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Pressure), TextCandidateCategory.Measurement,
                pressure!.ToNormalizedString(), pressure.ToString(),
                pressure.ToMaskedString(),
                TextMatchConfidence.Medium,
                pressure));
        }
        return results;
    }
}
