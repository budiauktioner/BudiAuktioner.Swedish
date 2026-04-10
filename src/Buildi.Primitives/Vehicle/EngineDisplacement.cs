using System.Globalization;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Engine displacement (<c>motorvolym</c> / <c>slagvolym</c>).
/// Bare numbers are interpreted as cubic centimeters (cc/mL).
/// </summary>
public sealed class EngineDisplacement : IEquatable<EngineDisplacement>, IComparable<EngineDisplacement>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Engine Displacement", "Motorvolym", "🔩", []);

    /// <summary>The underlying volume.</summary>
    public Volume Displacement { get; }

    /// <summary>Display form, e.g. <c>1998 mL</c>.</summary>
    public string Value { get; }

    /// <summary>Displacement in cubic centimeters (cc).</summary>
    public decimal CubicCentimeters => Displacement.Milliliters;

    /// <summary>Displacement in liters.</summary>
    public decimal Liters => Displacement.Liters;

    private EngineDisplacement(Volume vol)
    {
        Displacement = vol;
        Value = vol.ToString();
    }

    private EngineDisplacement(Volume vol, string displayValue)
    {
        Displacement = vol;
        Value = displayValue;
    }

    /// <summary>Creates an <see cref="EngineDisplacement"/> from a numeric value and volume unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static EngineDisplacement Create(decimal value, VolumeUnit unit)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Engine displacement must be positive.");
        var vol = Volume.Create(value, unit);
        return new EngineDisplacement(vol);
    }

    /// <summary>Creates an <see cref="EngineDisplacement"/> from a numeric value and volume unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static EngineDisplacement Create(int value, VolumeUnit unit) => Create((decimal)value, unit);

    /// <summary>Creates an <see cref="EngineDisplacement"/> from cubic centimeters (cc), e.g. <c>FromCubicCentimeters(1998)</c>.</summary>
    public static EngineDisplacement FromCubicCentimeters(decimal cc) => Create(cc, VolumeUnit.Milliliter);

    /// <summary>Creates an <see cref="EngineDisplacement"/> from cubic centimeters (cc), e.g. <c>FromCubicCentimeters(1998)</c>.</summary>
    public static EngineDisplacement FromCubicCentimeters(int cc) => Create(cc, VolumeUnit.Milliliter);

    /// <summary>Creates an <see cref="EngineDisplacement"/> from liters, e.g. <c>FromLiters(2.0m)</c>.</summary>
    public static EngineDisplacement FromLiters(decimal l) => Create(l, VolumeUnit.Liter);

    /// <summary>Creates an <see cref="EngineDisplacement"/> from liters, e.g. <c>FromLiters(2)</c>.</summary>
    public static EngineDisplacement FromLiters(int l) => Create(l, VolumeUnit.Liter);

    public static bool TryParse(string? input, out EngineDisplacement? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        // Bare number → cc (= mL)
        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            var vol = Volume.FromMilliliters(bare);
            result = new EngineDisplacement(vol, $"{FormatDecimal(bare)} mL");
            return true;
        }

        // Also support "cc" as an alias — check if input ends with "cc"
        if (trimmed.EndsWith("cc", StringComparison.OrdinalIgnoreCase))
        {
            var numPart = trimmed[..^2].Trim();
            if (MeasurementUnitParser.TryParseNumberOnly(numPart, out var ccVal) && ccVal > 0)
            {
                var vol = Volume.FromMilliliters(ccVal);
                result = new EngineDisplacement(vol, $"{FormatDecimal(ccVal)} mL");
                return true;
            }
        }

        if (!Volume.TryParse(trimmed, out var parsed) || parsed is null) return false;
        if (parsed.Liters <= 0) return false;

        result = new EngineDisplacement(parsed);
        return true;
    }

    public static EngineDisplacement Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid engine displacement.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns display form, e.g. <c>1998 mL</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, VolumeUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.Displacement.ToString(unit ?? r.Displacement.OriginalUnit, decimals);
            return r.Value;
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Displacement.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    public string ToNormalizedString() => Displacement.ToNormalizedString();
    public override string ToString() => Value;

    /// <summary>
    /// Returns the value formatted in the most human-readable metric unit, e.g. <c>2 L</c> instead of <c>2000 mL</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => Displacement.ToNaturalString(decimals);

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static EngineDisplacement operator +(EngineDisplacement a, EngineDisplacement b) => new(a.Displacement + b.Displacement);
    public static EngineDisplacement operator -(EngineDisplacement a, EngineDisplacement b) => new(a.Displacement - b.Displacement);
    public static EngineDisplacement operator *(EngineDisplacement a, decimal factor) => new(a.Displacement * factor);
    public static EngineDisplacement operator *(decimal factor, EngineDisplacement a) => new(a.Displacement * factor);
    public static EngineDisplacement operator /(EngineDisplacement a, decimal divisor) => new(a.Displacement / divisor);
    public static EngineDisplacement operator -(EngineDisplacement a) => new(-a.Displacement);

    public int CompareTo(EngineDisplacement? other) => other is null ? 1 : Displacement.Liters.CompareTo(other.Displacement.Liters);
    public bool Equals(EngineDisplacement? other) => other is not null && Displacement.Liters == other.Displacement.Liters;
    public override bool Equals(object? obj) => obj is EngineDisplacement other && Equals(other);
    public override int GetHashCode() => Displacement.Liters.GetHashCode();
    public static bool operator ==(EngineDisplacement? a, EngineDisplacement? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EngineDisplacement? a, EngineDisplacement? b) => !(a == b);
    public static bool operator <(EngineDisplacement a, EngineDisplacement b) => a.CompareTo(b) < 0;
    public static bool operator >(EngineDisplacement a, EngineDisplacement b) => a.CompareTo(b) > 0;
    public static bool operator <=(EngineDisplacement a, EngineDisplacement b) => a.CompareTo(b) <= 0;
    public static bool operator >=(EngineDisplacement a, EngineDisplacement b) => a.CompareTo(b) >= 0;
}
