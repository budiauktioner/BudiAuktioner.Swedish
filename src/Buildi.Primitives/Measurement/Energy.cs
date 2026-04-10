using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// An energy value stored internally in joules. Supports parsing from multiple unit formats
/// (e.g. <c>3600 kJ</c>, <c>1 kWh</c>, <c>500 cal</c>) and conversion between SI, electrical, and common units.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — joule definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see></description></item>
/// </list>
/// </remarks>
public sealed class Energy : IComparable<Energy>, IEquatable<Energy>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Energy", "Energi", "⚡", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private readonly decimal _joules;
    private readonly EnergyUnit _originalUnit;

    private Energy(decimal joules, EnergyUnit originalUnit)
    {
        _joules = joules;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in joules, e.g. <c>3600000</c> for 1 kWh.</summary>
    public decimal Joules => _joules;
    /// <summary>The value in kilojoules.</summary>
    public decimal Kilojoules => _joules / EnergyUnit.Kilojoule.ToBaseUnitFactor;
    /// <summary>The value in megajoules.</summary>
    public decimal Megajoules => _joules / EnergyUnit.Megajoule.ToBaseUnitFactor;
    /// <summary>The value in gigajoules.</summary>
    public decimal Gigajoules => _joules / EnergyUnit.Gigajoule.ToBaseUnitFactor;
    /// <summary>The value in terajoules.</summary>
    public decimal Terajoules => _joules / EnergyUnit.Terajoule.ToBaseUnitFactor;
    /// <summary>The value in watt-hours.</summary>
    public decimal WattHours => _joules / EnergyUnit.WattHour.ToBaseUnitFactor;
    /// <summary>The value in kilowatt-hours.</summary>
    public decimal KilowattHours => _joules / EnergyUnit.KilowattHour.ToBaseUnitFactor;
    /// <summary>The value in megawatt-hours.</summary>
    public decimal MegawattHours => _joules / EnergyUnit.MegawattHour.ToBaseUnitFactor;
    /// <summary>The value in gigawatt-hours.</summary>
    public decimal GigawattHours => _joules / EnergyUnit.GigawattHour.ToBaseUnitFactor;
    /// <summary>The value in terawatt-hours.</summary>
    public decimal TerawattHours => _joules / EnergyUnit.TerawattHour.ToBaseUnitFactor;
    /// <summary>The value in calories (thermochemical).</summary>
    public decimal Calories => _joules / EnergyUnit.Calorie.ToBaseUnitFactor;
    /// <summary>The value in kilocalories.</summary>
    public decimal Kilocalories => _joules / EnergyUnit.Kilocalorie.ToBaseUnitFactor;
    /// <summary>The value in British thermal units (IT).</summary>
    public decimal Btus => _joules / EnergyUnit.Btu.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public EnergyUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(EnergyUnit unit) => _joules / unit.ToBaseUnitFactor;

    public static Energy FromJoules(decimal j) => new(j, EnergyUnit.Joule);
    public static Energy FromKilojoules(decimal kj) => new(kj * EnergyUnit.Kilojoule.ToBaseUnitFactor, EnergyUnit.Kilojoule);
    public static Energy FromMegajoules(decimal mj) => new(mj * EnergyUnit.Megajoule.ToBaseUnitFactor, EnergyUnit.Megajoule);
    public static Energy FromGigajoules(decimal gj) => new(gj * EnergyUnit.Gigajoule.ToBaseUnitFactor, EnergyUnit.Gigajoule);
    public static Energy FromTerajoules(decimal tj) => new(tj * EnergyUnit.Terajoule.ToBaseUnitFactor, EnergyUnit.Terajoule);
    public static Energy FromWattHours(decimal wh) => new(wh * EnergyUnit.WattHour.ToBaseUnitFactor, EnergyUnit.WattHour);
    public static Energy FromKilowattHours(decimal kwh) => new(kwh * EnergyUnit.KilowattHour.ToBaseUnitFactor, EnergyUnit.KilowattHour);
    public static Energy FromMegawattHours(decimal mwh) => new(mwh * EnergyUnit.MegawattHour.ToBaseUnitFactor, EnergyUnit.MegawattHour);
    public static Energy FromGigawattHours(decimal gwh) => new(gwh * EnergyUnit.GigawattHour.ToBaseUnitFactor, EnergyUnit.GigawattHour);
    public static Energy FromTerawattHours(decimal twh) => new(twh * EnergyUnit.TerawattHour.ToBaseUnitFactor, EnergyUnit.TerawattHour);
    public static Energy FromCalories(decimal cal) => new(cal * EnergyUnit.Calorie.ToBaseUnitFactor, EnergyUnit.Calorie);
    public static Energy FromKilocalories(decimal kcal) => new(kcal * EnergyUnit.Kilocalorie.ToBaseUnitFactor, EnergyUnit.Kilocalorie);
    public static Energy FromBtus(decimal btu) => new(btu * EnergyUnit.Btu.ToBaseUnitFactor, EnergyUnit.Btu);

    /// <summary>Creates an <see cref="Energy"/> from a value and unit.</summary>
    public static Energy Create(decimal value, EnergyUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Energy? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!EnergyUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Energy(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Energy Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid energy.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>500 kWh</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, EnergyUnit? unit = null, int? decimals = null)
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
    /// Returns the value in SI base unit (joules) as an invariant string, e.g. <c>3600000 J</c>.
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
    /// Returns the value in joules with invariant formatting, e.g. <c>3600000 J</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_joules);
        return $"{formatted} {EnergyUnit.Joule.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>1 kWh</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>1000 Wh</c>.
    /// </summary>
    public string ToString(EnergyUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable unit for this value on the watt-hour scale.
    /// </summary>
    public EnergyUnit NaturalUnit => EnergyUnit.GetNatural(_joules);

    /// <summary>
    /// Returns the value formatted in the most human-readable watt-hour unit, e.g. <c>1.5 kWh</c> instead of <c>5400000 J</c>.
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

    public static Energy operator +(Energy a, Energy b) => new(a._joules + b._joules, a._originalUnit);
    public static Energy operator -(Energy a, Energy b) => new(a._joules - b._joules, a._originalUnit);
    public static Energy operator *(Energy a, decimal factor) => new(a._joules * factor, a._originalUnit);
    public static Energy operator *(decimal factor, Energy a) => new(a._joules * factor, a._originalUnit);
    public static Energy operator /(Energy a, decimal divisor) => new(a._joules / divisor, a._originalUnit);
    public static Energy operator -(Energy a) => new(-a._joules, a._originalUnit);

    public static bool operator ==(Energy? a, Energy? b) => a?._joules == b?._joules;
    public static bool operator !=(Energy? a, Energy? b) => !(a == b);
    public static bool operator <(Energy a, Energy b) => a._joules < b._joules;
    public static bool operator >(Energy a, Energy b) => a._joules > b._joules;
    public static bool operator <=(Energy a, Energy b) => a._joules <= b._joules;
    public static bool operator >=(Energy a, Energy b) => a._joules >= b._joules;

    public int CompareTo(Energy? other) => other is null ? 1 : _joules.CompareTo(other._joules);
    public bool Equals(Energy? other) => other is not null && _joules == other._joules;
    public override bool Equals(object? obj) => obj is Energy other && Equals(other);
    public override int GetHashCode() => _joules.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:TWh|GWh|MWh|kWh|TJ|GJ|MJ|kJ|kcal|Wh|BTU|cal|J)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like energy values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Energy>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Energy>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var energy)) continue;
            results.Add(new TextCandidate<Energy>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Energy), TextCandidateCategory.Measurement,
                energy!.ToNormalizedString(), energy.ToString(),
                energy.ToMaskedString(),
                TextMatchConfidence.Medium,
                energy));
        }
        return results;
    }
}
