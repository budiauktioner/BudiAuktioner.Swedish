using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// An absolute temperature stored internally in kelvin. Supports Celsius, Fahrenheit, and kelvin
/// scales with correct offset conversions (not multiplicative factors alone).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-base-units">BIPM SI base units</see> — kelvin definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Conversion_of_units">Wikipedia — Conversion of units</see> — temperature</description></item>
/// </list>
/// </remarks>
public sealed class Temperature : IComparable<Temperature>, IEquatable<Temperature>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Temperature", "Temperatur", "🌡️", ["https://www.bipm.org/en/measurement-units/si-base-units", "https://en.wikipedia.org/wiki/Conversion_of_units"]);

    private const decimal CelsiusOffset = 273.15m;

    private readonly decimal _kelvin;
    private readonly TemperatureUnit _originalUnit;

    private Temperature(decimal kelvin, TemperatureUnit originalUnit)
    {
        _kelvin = kelvin;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in degrees Celsius, e.g. <c>0</c> at the ice point.</summary>
    public decimal Celsius => _kelvin - CelsiusOffset;

    /// <summary>The value in degrees Fahrenheit, e.g. <c>32</c> at <c>0 °C</c>.</summary>
    public decimal Fahrenheit => (_kelvin - CelsiusOffset) * 9m / 5m + 32m;

    /// <summary>The value in kelvin.</summary>
    public decimal Kelvin => _kelvin;

    /// <summary>The scale the value was originally parsed or created from.</summary>
    public TemperatureUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the absolute temperature expressed in <paramref name="unit"/>.</summary>
    public decimal In(TemperatureUnit unit)
    {
        if (ReferenceEquals(unit, TemperatureUnit.Celsius)) return Celsius;
        if (ReferenceEquals(unit, TemperatureUnit.Fahrenheit)) return Fahrenheit;
        if (ReferenceEquals(unit, TemperatureUnit.Kelvin)) return Kelvin;
        throw new ArgumentException("Unknown temperature unit.", nameof(unit));
    }

    public static Temperature FromCelsius(decimal c) => new(c + CelsiusOffset, TemperatureUnit.Celsius);

    public static Temperature FromFahrenheit(decimal f) =>
        new((f - 32m) * 5m / 9m + CelsiusOffset, TemperatureUnit.Fahrenheit);

    public static Temperature FromKelvin(decimal k) => new(k, TemperatureUnit.Kelvin);

    /// <summary>Creates a <see cref="Temperature"/> from a value in the given <paramref name="unit"/>.</summary>
    public static Temperature Create(decimal value, TemperatureUnit unit)
    {
        if (ReferenceEquals(unit, TemperatureUnit.Celsius)) return FromCelsius(value);
        if (ReferenceEquals(unit, TemperatureUnit.Fahrenheit)) return FromFahrenheit(value);
        if (ReferenceEquals(unit, TemperatureUnit.Kelvin)) return FromKelvin(value);
        throw new ArgumentException("Unknown temperature unit.", nameof(unit));
    }

    public static bool TryParse(string? input, out Temperature? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!TemperatureUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = Create(value, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Temperature Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid temperature.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>20 °C</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, TemperatureUnit? unit = null, int? decimals = null)
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
    /// Returns the value in kelvin as an invariant string, e.g. <c>293.15 K</c>.
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
    /// Returns the value in kelvin with invariant formatting, e.g. <c>293.15 K</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_kelvin);
        return $"{formatted} {TemperatureUnit.Kelvin.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>20 °C</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>68 °F</c>.
    /// </summary>
    public string ToString(TemperatureUnit unit, int? decimals = null)
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

    public static Temperature operator +(Temperature t, TemperatureDelta d) =>
        new(t._kelvin + d.Kelvin, t._originalUnit);

    public static Temperature operator +(TemperatureDelta d, Temperature t) => t + d;

    public static Temperature operator -(Temperature t, TemperatureDelta d) =>
        new(t._kelvin - d.Kelvin, t._originalUnit);

    public static TemperatureDelta operator -(Temperature a, Temperature b) =>
        TemperatureDelta.FromKelvin(a._kelvin - b._kelvin);

    public static bool operator ==(Temperature? a, Temperature? b) => a?._kelvin == b?._kelvin;
    public static bool operator !=(Temperature? a, Temperature? b) => !(a == b);
    public static bool operator <(Temperature a, Temperature b) => a._kelvin < b._kelvin;
    public static bool operator >(Temperature a, Temperature b) => a._kelvin > b._kelvin;
    public static bool operator <=(Temperature a, Temperature b) => a._kelvin <= b._kelvin;
    public static bool operator >=(Temperature a, Temperature b) => a._kelvin >= b._kelvin;

    public int CompareTo(Temperature? other) => other is null ? 1 : _kelvin.CompareTo(other._kelvin);

    public bool Equals(Temperature? other) => other is not null && _kelvin == other._kelvin;

    public override bool Equals(object? obj) => obj is Temperature other && Equals(other);

    public override int GetHashCode() => _kelvin.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:°[CF]|[CFK])\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like temperature values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Temperature>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Temperature>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var temperature)) continue;
            results.Add(new TextCandidate<Temperature>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Temperature), TextCandidateCategory.Measurement,
                temperature!.ToNormalizedString(), temperature.ToString(),
                temperature.ToMaskedString(),
                TextMatchConfidence.Medium,
                temperature));
        }
        return results;
    }
}
