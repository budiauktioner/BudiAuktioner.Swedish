using System.Globalization;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Product;

/// <summary>
/// Processor clock speed (<c>processorhastighet</c>).
/// Bare numbers are interpreted as GHz. Delegates to <see cref="Frequency"/> for unit parsing.
/// </summary>
public sealed class ProcessorSpeed : IEquatable<ProcessorSpeed>, IComparable<ProcessorSpeed>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Processor Speed", "Processorhastighet", "💻", []);

    /// <summary>The underlying frequency value.</summary>
    public Frequency Speed { get; }

    /// <summary>Display form, e.g. <c>3.5 GHz</c>.</summary>
    public string Value { get; }

    public decimal Hertz => Speed.Hertz;
    public decimal Megahertz => Speed.Megahertz;
    public decimal Gigahertz => Speed.Gigahertz;

    private ProcessorSpeed(Frequency speed)
    {
        Speed = speed;
        Value = speed.ToString();
    }

    private ProcessorSpeed(Frequency speed, string displayValue)
    {
        Speed = speed;
        Value = displayValue;
    }

    /// <summary>Creates a <see cref="ProcessorSpeed"/> from a numeric value and frequency unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static ProcessorSpeed Create(decimal value, FrequencyUnit unit)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Processor speed must be positive.");
        var freq = Frequency.Create(value, unit);
        return new ProcessorSpeed(freq);
    }

    /// <summary>Creates a <see cref="ProcessorSpeed"/> from a numeric value and frequency unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static ProcessorSpeed Create(int value, FrequencyUnit unit) => Create((decimal)value, unit);

    /// <summary>Creates a <see cref="ProcessorSpeed"/> from gigahertz, e.g. <c>FromGigahertz(3.5m)</c>.</summary>
    public static ProcessorSpeed FromGigahertz(decimal ghz) => Create(ghz, FrequencyUnit.Gigahertz);

    /// <summary>Creates a <see cref="ProcessorSpeed"/> from gigahertz, e.g. <c>FromGigahertz(3)</c>.</summary>
    public static ProcessorSpeed FromGigahertz(int ghz) => Create(ghz, FrequencyUnit.Gigahertz);

    /// <summary>Creates a <see cref="ProcessorSpeed"/> from megahertz, e.g. <c>FromMegahertz(3500)</c>.</summary>
    public static ProcessorSpeed FromMegahertz(decimal mhz) => Create(mhz, FrequencyUnit.Megahertz);

    /// <summary>Creates a <see cref="ProcessorSpeed"/> from megahertz, e.g. <c>FromMegahertz(3500)</c>.</summary>
    public static ProcessorSpeed FromMegahertz(int mhz) => Create(mhz, FrequencyUnit.Megahertz);

    public static bool TryParse(string? input, out ProcessorSpeed? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            var freq = Frequency.FromGigahertz(bare);
            result = new ProcessorSpeed(freq, $"{FormatDecimal(bare)} GHz");
            return true;
        }

        if (!Frequency.TryParse(trimmed, out var parsed) || parsed is null) return false;
        if (parsed.Hertz <= 0) return false;

        result = new ProcessorSpeed(parsed);
        return true;
    }

    public static ProcessorSpeed Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid processor speed.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns display form, e.g. <c>3.5 GHz</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, FrequencyUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.Speed.ToString(unit ?? r.Speed.OriginalUnit, decimals);
            return r.Value;
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Speed.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    public string ToNormalizedString() => Speed.ToNormalizedString();
    public override string ToString() => Value;

    /// <summary>
    /// Returns the value formatted in the most human-readable unit, e.g. <c>3.5 GHz</c> instead of <c>3500000000 Hz</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => Speed.ToNaturalString(decimals);

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public int CompareTo(ProcessorSpeed? other) => other is null ? 1 : Speed.Hertz.CompareTo(other.Speed.Hertz);
    public bool Equals(ProcessorSpeed? other) => other is not null && Speed.Hertz == other.Speed.Hertz;
    public override bool Equals(object? obj) => obj is ProcessorSpeed other && Equals(other);
    public override int GetHashCode() => Speed.Hertz.GetHashCode();
    public static bool operator ==(ProcessorSpeed? a, ProcessorSpeed? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ProcessorSpeed? a, ProcessorSpeed? b) => !(a == b);
    public static bool operator <(ProcessorSpeed a, ProcessorSpeed b) => a.CompareTo(b) < 0;
    public static bool operator >(ProcessorSpeed a, ProcessorSpeed b) => a.CompareTo(b) > 0;
    public static bool operator <=(ProcessorSpeed a, ProcessorSpeed b) => a.CompareTo(b) <= 0;
    public static bool operator >=(ProcessorSpeed a, ProcessorSpeed b) => a.CompareTo(b) >= 0;
}
