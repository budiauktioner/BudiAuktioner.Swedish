using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Vehicle electric energy consumption (<c>elförbrukning</c>) over a distance, stored internally
/// as kilowatt-hours per 100&#160;kilometres (kWh/100&#160;km). Supports input in <c>kWh/100km</c>,
/// <c>kWh/km</c>, <c>Wh/km</c>, and US-style <c>mi/kWh</c> / <c>kWh/mi</c>.
/// </summary>
/// <remarks>
/// <para>Sibling of <see cref="FuelConsumption"/>. Use this type for battery electric vehicles
/// (BEV) and the electric portion of plug-in hybrid consumption figures, where expressing the
/// value as litres of fuel per distance does not apply.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Fuel_economy_in_electric_vehicles">Wikipedia — Fuel economy in electric vehicles</see></description></item>
/// <item><description><see href="https://www.energimyndigheten.se/">Energimyndigheten</see> — energy declarations</description></item>
/// </list>
/// </remarks>
public sealed class EnergyConsumption : IEquatable<EnergyConsumption>, IComparable<EnergyConsumption>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Energy Consumption",
        "Elförbrukning",
        "🔌",
        ["https://en.wikipedia.org/wiki/Fuel_economy_in_electric_vehicles", "https://www.energimyndigheten.se/"]);

    private const decimal KilometresPerMile = 1.609344m;
    private const int ConversionPrecision = 6;

    private static readonly Regex Pattern = new(
        @"^\s*(?<num>[0-9][0-9 .,]*[0-9]|[0-9])\s*(?<unit>kWh/100\s*km|kWh/km|Wh/km|mi/kWh|miles/kWh|kWh/mi|kWh/mile)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly decimal _kwhPer100Km;
    private readonly string _displayUnit;

    /// <summary>Display form preserving the original unit, e.g. <c>15 kWh/100km</c>, <c>150 Wh/km</c>, <c>4 mi/kWh</c>.</summary>
    public string Value { get; }

    /// <summary>Consumption in kilowatt-hours per 100 km, e.g. <c>15</c> for a typical EV.</summary>
    public decimal KwhPer100Km => _kwhPer100Km;

    /// <summary>Consumption in kilowatt-hours per km, e.g. <c>0.15</c> for 15 kWh/100km.</summary>
    public decimal KwhPerKm => Math.Round(_kwhPer100Km / 100m, ConversionPrecision);

    /// <summary>Consumption in watt-hours per km, e.g. <c>150</c> for 15 kWh/100km.</summary>
    public decimal WhPerKm => Math.Round(_kwhPer100Km * 10m, ConversionPrecision);

    /// <summary>Energy economy in US miles per kilowatt-hour, e.g. <c>4.13</c> for 15 kWh/100km.</summary>
    public decimal MilesPerKwh => _kwhPer100Km > 0
        ? Math.Round(100m / (_kwhPer100Km * KilometresPerMile), ConversionPrecision)
        : 0;

    /// <summary>Consumption in kilowatt-hours per mile, e.g. <c>0.241</c> for 15 kWh/100km.</summary>
    public decimal KwhPerMile => Math.Round(_kwhPer100Km * KilometresPerMile / 100m, ConversionPrecision);

    private EnergyConsumption(decimal kwhPer100Km, string displayUnit, string value)
    {
        _kwhPer100Km = kwhPer100Km;
        _displayUnit = displayUnit;
        Value = value;
    }

    /// <summary>Creates an <see cref="EnergyConsumption"/> from kWh per 100 km, e.g. <c>FromKwhPer100Km(15m)</c>.</summary>
    public static EnergyConsumption FromKwhPer100Km(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new EnergyConsumption(value, "kWh/100km", $"{FormatDecimal(value)} kWh/100km");
    }

    /// <summary>Creates an <see cref="EnergyConsumption"/> from Wh per km, e.g. <c>FromWhPerKm(150m)</c>.</summary>
    public static EnergyConsumption FromWhPerKm(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new EnergyConsumption(Math.Round(value / 10m, ConversionPrecision), "Wh/km", $"{FormatDecimal(value)} Wh/km");
    }

    /// <summary>Creates an <see cref="EnergyConsumption"/> from kWh per km, e.g. <c>FromKwhPerKm(0.15m)</c>.</summary>
    public static EnergyConsumption FromKwhPerKm(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new EnergyConsumption(Math.Round(value * 100m, ConversionPrecision), "kWh/km", $"{FormatDecimal(value)} kWh/km");
    }

    /// <summary>Creates an <see cref="EnergyConsumption"/> from miles per kWh, e.g. <c>FromMilesPerKwh(4m)</c>.</summary>
    public static EnergyConsumption FromMilesPerKwh(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        var kwhPer100Km = Math.Round(100m / (value * KilometresPerMile), ConversionPrecision);
        return new EnergyConsumption(kwhPer100Km, "mi/kWh", $"{FormatDecimal(value)} mi/kWh");
    }

    /// <summary>Creates an <see cref="EnergyConsumption"/> from kWh per mile, e.g. <c>FromKwhPerMile(0.25m)</c>.</summary>
    public static EnergyConsumption FromKwhPerMile(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        var kwhPer100Km = Math.Round(value * 100m / KilometresPerMile, ConversionPrecision);
        return new EnergyConsumption(kwhPer100Km, "kWh/mi", $"{FormatDecimal(value)} kWh/mi");
    }

    public static bool TryParse(string? input, out EnergyConsumption? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        var match = Pattern.Match(trimmed);
        if (match.Success)
        {
            if (!MeasurementUnitParser.TryParseNumberOnly(match.Groups["num"].Value, out var num)) return false;
            if (num <= 0) return false;
            return TryCreateFromUnit(num, match.Groups["unit"].Value, out result);
        }

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            result = new EnergyConsumption(bare, "kWh/100km", $"{FormatDecimal(bare)} kWh/100km");
            return true;
        }

        return false;
    }

    private static bool TryCreateFromUnit(decimal num, string unitRaw, out EnergyConsumption? result)
    {
        result = null;
        var u = unitRaw.ToLowerInvariant().Replace(" ", "");

        decimal kwhPer100Km;
        string displayUnit;

        switch (u)
        {
            case "kwh/100km":
                kwhPer100Km = num;
                displayUnit = "kWh/100km";
                break;
            case "kwh/km":
                kwhPer100Km = Math.Round(num * 100m, ConversionPrecision);
                displayUnit = "kWh/km";
                break;
            case "wh/km":
                kwhPer100Km = Math.Round(num / 10m, ConversionPrecision);
                displayUnit = "Wh/km";
                break;
            case "mi/kwh" or "miles/kwh":
                kwhPer100Km = Math.Round(100m / (num * KilometresPerMile), ConversionPrecision);
                displayUnit = "mi/kWh";
                break;
            case "kwh/mi" or "kwh/mile":
                kwhPer100Km = Math.Round(num * 100m / KilometresPerMile, ConversionPrecision);
                displayUnit = "kWh/mi";
                break;
            default:
                return false;
        }

        result = new EnergyConsumption(kwhPer100Km, displayUnit, $"{FormatDecimal(num)} {displayUnit}");
        return true;
    }

    public static EnergyConsumption Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid energy consumption.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form in the original unit, e.g. <c>15 kWh/100km</c> or <c>4 mi/kWh</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) && r is not null
            ? r.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns normalized form in kWh/100&#160;km, e.g. <c>15 kWh/100km</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already identical to its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input) == input;

    /// <summary>Returns normalized form in kWh/100&#160;km, e.g. <c>15 kWh/100km</c>.</summary>
    public string ToNormalizedString() => $"{FormatDecimal(_kwhPer100Km)} kWh/100km";

    /// <summary>Returns display form preserving original unit, e.g. <c>150 Wh/km</c>.</summary>
    public override string ToString() => Value;

    /// <summary>The original unit symbol, e.g. <c>kWh/100km</c>, <c>Wh/km</c>, <c>mi/kWh</c>.</summary>
    public string OriginalUnit => _displayUnit;

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)\d+[0-9 .,]*\s*(?:kWh/100\s*km|kWh/km|Wh/km|mi/kWh|miles/kWh|kWh/mi|kWh/mile)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like energy consumption values
    /// (e.g. <c>15 kWh/100km</c>, <c>150 Wh/km</c>, <c>4 mi/kWh</c>). Compound unit patterns are distinctive.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<EnergyConsumption>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<EnergyConsumption>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var ec)) continue;
            results.Add(new TextCandidate<EnergyConsumption>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(EnergyConsumption), TextCandidateCategory.Vehicle,
                ec!.ToNormalizedString(), ec.ToString(),
                ec.ToMaskedString(),
                TextMatchConfidence.Medium,
                ec));
        }
        return results;
    }

    public static bool operator ==(EnergyConsumption? a, EnergyConsumption? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EnergyConsumption? a, EnergyConsumption? b) => !(a == b);

    public int CompareTo(EnergyConsumption? other) =>
        other is null ? 1 : _kwhPer100Km.CompareTo(other._kwhPer100Km);

    public static bool operator <(EnergyConsumption left, EnergyConsumption right) => left.CompareTo(right) < 0;
    public static bool operator >(EnergyConsumption left, EnergyConsumption right) => left.CompareTo(right) > 0;
    public static bool operator <=(EnergyConsumption left, EnergyConsumption right) => left.CompareTo(right) <= 0;
    public static bool operator >=(EnergyConsumption left, EnergyConsumption right) => left.CompareTo(right) >= 0;

    public bool Equals(EnergyConsumption? other) =>
        other is not null && _kwhPer100Km == other._kwhPer100Km;

    public override bool Equals(object? obj) => obj is EnergyConsumption other && Equals(other);
    public override int GetHashCode() => _kwhPer100Km.GetHashCode();
}
