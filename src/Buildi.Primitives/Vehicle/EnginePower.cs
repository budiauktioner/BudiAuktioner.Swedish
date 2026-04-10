using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Engine power output (<c>motoreffekt</c>).
/// Bare numbers are interpreted as horsepower (hp/hk). Delegates to <see cref="Power"/> for unit parsing.
/// Also accepts <c>hk</c> (Swedish abbreviation for <c>hästkraft</c>).
/// </summary>
public sealed class EnginePower : IEquatable<EnginePower>, IComparable<EnginePower>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Engine Power", "Motoreffekt", "🐎", []);

    private static readonly Regex HkPattern = new(
        @"^\s*(?<num>[0-9][0-9 .,]*[0-9]|[0-9])\s*hk\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ApproximatePrefix = new(
        @"^(?:ca\.?\s+|circa\s+|~\s*|approx\.?\s+|ungefär\s+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>The underlying power value.</summary>
    public Power PowerValue { get; }

    /// <summary>Display form, e.g. <c>150 hp</c>.</summary>
    public string Value { get; }

    /// <summary>Power in horsepower.</summary>
    public decimal Horsepower => PowerValue.Horsepower;

    /// <summary>Power in kilowatts.</summary>
    public decimal Kilowatts => PowerValue.Kilowatts;

    /// <summary>Power in watts.</summary>
    public decimal Watts => PowerValue.Watts;

    private EnginePower(Power power)
    {
        PowerValue = power;
        Value = power.ToString();
    }

    private EnginePower(Power power, string displayValue)
    {
        PowerValue = power;
        Value = displayValue;
    }

    /// <summary>Creates an <see cref="EnginePower"/> from a numeric value and power unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static EnginePower Create(decimal value, PowerUnit unit)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Engine power must be positive.");
        var power = Power.Create(value, unit);
        return new EnginePower(power);
    }

    /// <summary>Creates an <see cref="EnginePower"/> from a numeric value and power unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static EnginePower Create(int value, PowerUnit unit) => Create((decimal)value, unit);

    /// <summary>Creates an <see cref="EnginePower"/> from horsepower, e.g. <c>FromHorsepower(150)</c>.</summary>
    public static EnginePower FromHorsepower(decimal hp) => Create(hp, PowerUnit.Horsepower);

    /// <summary>Creates an <see cref="EnginePower"/> from horsepower, e.g. <c>FromHorsepower(150)</c>.</summary>
    public static EnginePower FromHorsepower(int hp) => Create(hp, PowerUnit.Horsepower);

    /// <summary>Creates an <see cref="EnginePower"/> from kilowatts, e.g. <c>FromKilowatts(110)</c>.</summary>
    public static EnginePower FromKilowatts(decimal kw) => Create(kw, PowerUnit.Kilowatt);

    /// <summary>Creates an <see cref="EnginePower"/> from kilowatts, e.g. <c>FromKilowatts(110)</c>.</summary>
    public static EnginePower FromKilowatts(int kw) => Create(kw, PowerUnit.Kilowatt);

    /// <summary>Creates an <see cref="EnginePower"/> from watts, e.g. <c>FromWatts(110000)</c>.</summary>
    public static EnginePower FromWatts(decimal w) => Create(w, PowerUnit.Watt);

    /// <summary>Creates an <see cref="EnginePower"/> from watts, e.g. <c>FromWatts(110000)</c>.</summary>
    public static EnginePower FromWatts(int w) => Create(w, PowerUnit.Watt);

    public static bool TryParse(string? input, out EnginePower? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        trimmed = StripApproximatePrefix(trimmed);

        // Bare number → hp
        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            var power = Power.FromHorsepower(bare);
            result = new EnginePower(power, $"{FormatDecimal(bare)} {PowerUnit.Horsepower.Symbol}");
            return true;
        }

        // Handle "hk" (Swedish for horsepower)
        var hkMatch = HkPattern.Match(trimmed);
        if (hkMatch.Success)
        {
            if (!MeasurementUnitParser.TryParseNumberOnly(hkMatch.Groups["num"].Value, out var hkVal))
                return false;
            if (hkVal <= 0) return false;
            var power = Power.FromHorsepower(hkVal);
            result = new EnginePower(power, $"{FormatDecimal(hkVal)} {PowerUnit.Horsepower.Symbol}");
            return true;
        }

        if (!Power.TryParse(trimmed, out var parsed) || parsed is null) return false;
        if (parsed.Watts <= 0) return false;

        result = new EnginePower(parsed);
        return true;
    }

    public static EnginePower Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid engine power.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns display form, e.g. <c>150 hp</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, PowerUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.PowerValue.ToString(unit ?? r.PowerValue.OriginalUnit, decimals);
            return r.Value;
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.PowerValue.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    public string ToNormalizedString() => PowerValue.ToNormalizedString();
    public override string ToString() => Value;

    /// <summary>
    /// Returns the value formatted in the most human-readable SI unit, e.g. <c>1.5 kW</c> instead of <c>1500 W</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => PowerValue.ToNaturalString(decimals);

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)\d+[0-9 .,]*\s*hk\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for engine power values using the Swedish <c>hk</c> (hästkraft) suffix.
    /// Standard power units (kW, W, hp) are handled by <see cref="Power.FindCandidatesInText"/>.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<EnginePower>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<EnginePower>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var ep)) continue;
            results.Add(new TextCandidate<EnginePower>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(EnginePower), TextCandidateCategory.Vehicle,
                ep!.ToNormalizedString(), ep.ToString(),
                ep.ToMaskedString(),
                TextMatchConfidence.Low,
                ep));
        }
        return results;
    }

    private static string StripApproximatePrefix(string input)
    {
        var match = ApproximatePrefix.Match(input);
        return match.Success ? input[match.Length..] : input;
    }

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static EnginePower operator +(EnginePower a, EnginePower b) => new(a.PowerValue + b.PowerValue);
    public static EnginePower operator -(EnginePower a, EnginePower b) => new(a.PowerValue - b.PowerValue);
    public static EnginePower operator *(EnginePower a, decimal factor) => new(a.PowerValue * factor);
    public static EnginePower operator *(decimal factor, EnginePower a) => new(a.PowerValue * factor);
    public static EnginePower operator /(EnginePower a, decimal divisor) => new(a.PowerValue / divisor);
    public static EnginePower operator -(EnginePower a) => new(-a.PowerValue);

    public int CompareTo(EnginePower? other) => other is null ? 1 : PowerValue.Watts.CompareTo(other.PowerValue.Watts);
    public bool Equals(EnginePower? other) => other is not null && PowerValue.Watts == other.PowerValue.Watts;
    public override bool Equals(object? obj) => obj is EnginePower other && Equals(other);
    public override int GetHashCode() => PowerValue.Watts.GetHashCode();
    public static bool operator ==(EnginePower? a, EnginePower? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EnginePower? a, EnginePower? b) => !(a == b);
    public static bool operator <(EnginePower a, EnginePower b) => a.CompareTo(b) < 0;
    public static bool operator >(EnginePower a, EnginePower b) => a.CompareTo(b) > 0;
    public static bool operator <=(EnginePower a, EnginePower b) => a.CompareTo(b) <= 0;
    public static bool operator >=(EnginePower a, EnginePower b) => a.CompareTo(b) >= 0;
}
