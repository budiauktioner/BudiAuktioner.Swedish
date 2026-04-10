using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Vehicle fuel consumption (<c>bränsleförbrukning</c>), stored internally as liters per 100 km.
/// Supports conventional fuel units (l/100km, km/l, mpg) and electric consumption (kWh/100km).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Fuel_economy_in_automobiles">Wikipedia — Fuel economy in automobiles</see></description></item>
/// </list>
/// </remarks>
public sealed class FuelConsumption : IEquatable<FuelConsumption>, IComparable<FuelConsumption>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Fuel Consumption", "Bränsleförbrukning", "⛽", ["https://en.wikipedia.org/wiki/Fuel_economy_in_automobiles"]);

    private const decimal MpgUsConversion = 235.214583m;
    private const decimal MpgImpConversion = 282.481m;
    private const int ConversionPrecision = 6;

    private static readonly Regex Pattern = new(
        @"^\s*(?<num>[0-9][0-9 .,]*[0-9]|[0-9])\s*(?<unit>l/100\s*km|liter/100\s*km|L/100\s*km|km/l|km/liter|km/L|mpg(?:\s*\(imp\))?|miles\s*per\s*gallon|kWh/100\s*km|kWh/km|Wh/km)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly decimal _litersPer100Km;
    private readonly decimal _kwhPer100Km;
    private readonly bool _isElectric;

    /// <summary>Display form preserving original unit, e.g. <c>8.3 l/100km</c> or <c>15 kWh/100km</c>.</summary>
    public string Value { get; }

    /// <summary>Consumption in liters per 100 km. Zero for electric vehicles.</summary>
    public decimal LitersPer100Km => _litersPer100Km;

    /// <summary>Fuel economy in km per liter, e.g. <c>12.048</c>. Zero for electric vehicles.</summary>
    public decimal KilometersPerLiter => _litersPer100Km > 0 ? Math.Round(100m / _litersPer100Km, ConversionPrecision) : 0;

    /// <summary>Fuel economy in US miles per gallon, e.g. <c>28.34</c>. Zero for electric vehicles.</summary>
    public decimal MilesPerGallonUs => _litersPer100Km > 0 ? Math.Round(MpgUsConversion / _litersPer100Km, ConversionPrecision) : 0;

    /// <summary>Fuel economy in imperial miles per gallon. Zero for electric vehicles.</summary>
    public decimal MilesPerGallonImp => _litersPer100Km > 0 ? Math.Round(MpgImpConversion / _litersPer100Km, ConversionPrecision) : 0;

    /// <summary>Electric consumption in kWh per 100 km, e.g. <c>15</c>. Zero for combustion vehicles.</summary>
    public decimal KwhPer100Km => _kwhPer100Km;

    /// <summary><see langword="true"/> when this value represents electric vehicle consumption (kWh-based).</summary>
    public bool IsElectric => _isElectric;

    private FuelConsumption(decimal litersPer100Km, decimal kwhPer100Km, bool isElectric, string value)
    {
        _litersPer100Km = litersPer100Km;
        _kwhPer100Km = kwhPer100Km;
        _isElectric = isElectric;
        Value = value;
    }

    /// <summary>Creates a <see cref="FuelConsumption"/> from liters per 100 km, e.g. <c>FromLitersPer100Km(8.3m)</c>.</summary>
    public static FuelConsumption FromLitersPer100Km(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new FuelConsumption(value, 0, false, $"{FormatDecimal(value)} l/100km");
    }

    /// <summary>Creates a <see cref="FuelConsumption"/> from km per liter, e.g. <c>FromKilometersPerLiter(12m)</c>.</summary>
    public static FuelConsumption FromKilometersPerLiter(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new FuelConsumption(Math.Round(100m / value, ConversionPrecision), 0, false, $"{FormatDecimal(value)} km/l");
    }

    /// <summary>Creates a <see cref="FuelConsumption"/> from US miles per gallon, e.g. <c>FromMpgUs(28m)</c>.</summary>
    public static FuelConsumption FromMpgUs(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new FuelConsumption(Math.Round(MpgUsConversion / value, ConversionPrecision), 0, false, $"{FormatDecimal(value)} mpg");
    }

    /// <summary>Creates a <see cref="FuelConsumption"/> from kWh per 100 km (EV), e.g. <c>FromKwhPer100Km(15m)</c>.</summary>
    public static FuelConsumption FromKwhPer100Km(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new FuelConsumption(0, value, true, $"{FormatDecimal(value)} kWh/100km");
    }

    public static bool TryParse(string? input, out FuelConsumption? result)
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
            result = new FuelConsumption(bare, 0, false, $"{FormatDecimal(bare)} l/100km");
            return true;
        }

        return false;
    }

    private static bool TryCreateFromUnit(decimal num, string unitRaw, out FuelConsumption? result)
    {
        result = null;
        var u = unitRaw.ToLowerInvariant().Replace(" ", "");

        decimal litersPer100Km = 0;
        decimal kwhPer100Km = 0;
        bool isElectric = false;
        string displayUnit;

        switch (u)
        {
            case "l/100km" or "liter/100km":
                litersPer100Km = num;
                displayUnit = "l/100km";
                break;
            case "km/l" or "km/liter":
                litersPer100Km = Math.Round(100m / num, ConversionPrecision);
                displayUnit = "km/l";
                break;
            case "mpg" or "milespergallon":
                litersPer100Km = Math.Round(MpgUsConversion / num, ConversionPrecision);
                displayUnit = "mpg";
                break;
            case "mpg(imp)":
                litersPer100Km = Math.Round(MpgImpConversion / num, ConversionPrecision);
                displayUnit = "mpg (imp)";
                break;
            case "kwh/100km":
                kwhPer100Km = num;
                isElectric = true;
                displayUnit = "kWh/100km";
                break;
            case "kwh/km":
                kwhPer100Km = Math.Round(num * 100m, ConversionPrecision);
                isElectric = true;
                displayUnit = "kWh/km";
                break;
            case "wh/km":
                kwhPer100Km = Math.Round(num / 10m, ConversionPrecision);
                isElectric = true;
                displayUnit = "Wh/km";
                break;
            default:
                return false;
        }

        result = new FuelConsumption(litersPer100Km, kwhPer100Km, isElectric, $"{FormatDecimal(num)} {displayUnit}");
        return true;
    }

    public static FuelConsumption Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid fuel consumption.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form in the original unit, e.g. <c>8.3 l/100km</c> or <c>12 km/l</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) && r is not null
            ? r.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns normalized form in l/100km (or kWh/100km for EVs), e.g. <c>8.3 l/100km</c>.
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

    /// <summary>Returns normalized form, e.g. <c>8.3 l/100km</c> or <c>15 kWh/100km</c>.</summary>
    public string ToNormalizedString() =>
        _isElectric
            ? $"{FormatDecimal(_kwhPer100Km)} kWh/100km"
            : $"{FormatDecimal(_litersPer100Km)} l/100km";

    /// <summary>Returns display form preserving original unit, e.g. <c>8.3 l/100km</c>.</summary>
    public override string ToString() => Value;

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)\d+[0-9 .,]*\s*(?:l/100\s*km|liter/100\s*km|km/l|km/liter|mpg|kWh/100\s*km|kWh/km|Wh/km)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like fuel consumption values
    /// (e.g. <c>8.3 l/100km</c>, <c>15 kWh/100km</c>). Compound unit patterns are distinctive.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<FuelConsumption>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<FuelConsumption>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var fc)) continue;
            results.Add(new TextCandidate<FuelConsumption>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(FuelConsumption), TextCandidateCategory.Vehicle,
                fc!.ToNormalizedString(), fc.ToString(),
                fc.ToMaskedString(),
                TextMatchConfidence.Medium,
                fc));
        }
        return results;
    }

    public static bool operator ==(FuelConsumption? a, FuelConsumption? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(FuelConsumption? a, FuelConsumption? b) => !(a == b);

    public int CompareTo(FuelConsumption? other)
    {
        if (other is null) return 1;
        var c = _litersPer100Km.CompareTo(other._litersPer100Km);
        if (c != 0) return c;
        return _kwhPer100Km.CompareTo(other._kwhPer100Km);
    }

    public static bool operator <(FuelConsumption left, FuelConsumption right) => left.CompareTo(right) < 0;
    public static bool operator >(FuelConsumption left, FuelConsumption right) => left.CompareTo(right) > 0;
    public static bool operator <=(FuelConsumption left, FuelConsumption right) => left.CompareTo(right) <= 0;
    public static bool operator >=(FuelConsumption left, FuelConsumption right) => left.CompareTo(right) >= 0;

    public bool Equals(FuelConsumption? other) =>
        other is not null && _litersPer100Km == other._litersPer100Km && _kwhPer100Km == other._kwhPer100Km;

    public override bool Equals(object? obj) => obj is FuelConsumption other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_litersPer100Km, _kwhPer100Km);
}
