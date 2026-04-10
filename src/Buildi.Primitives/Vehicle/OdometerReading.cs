using System.Globalization;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Vehicle odometer reading (<c>mätarställning</c>).
/// Bare numbers are interpreted as kilometers. Also accepts Swedish miles (<c>mil</c>, 1 mil = 10 km)
/// and English miles (<c>mi</c>).
/// </summary>
public sealed class OdometerReading : IEquatable<OdometerReading>, IComparable<OdometerReading>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Odometer Reading", "Mätarställning", "🔢", []);

    /// <summary>The underlying length value.</summary>
    public Length Distance { get; }

    /// <summary>Display form preserving the original unit, e.g. <c>15000 km</c>.</summary>
    public string Value { get; }

    /// <summary>Reading in kilometers.</summary>
    public decimal Kilometers => Distance.Kilometers;

    /// <summary>Reading in English miles.</summary>
    public decimal Miles => Distance.Miles;

    /// <summary>Reading in Swedish miles (1 mil = 10 km).</summary>
    public decimal SwedishMiles => Distance.SwedishMiles;

    private OdometerReading(Length distance)
    {
        Distance = distance;
        Value = distance.ToString();
    }

    private OdometerReading(Length distance, string displayValue)
    {
        Distance = distance;
        Value = displayValue;
    }

    /// <summary>Creates an <see cref="OdometerReading"/> from a numeric value and length unit. Value must be non-negative.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static OdometerReading Create(decimal value, LengthUnit unit)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Odometer reading cannot be negative.");
        var distance = Length.Create(value, unit);
        return new OdometerReading(distance);
    }

    /// <summary>Creates an <see cref="OdometerReading"/> from a numeric value and length unit. Value must be non-negative.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static OdometerReading Create(int value, LengthUnit unit) => Create((decimal)value, unit);

    /// <summary>Creates an <see cref="OdometerReading"/> from kilometers, e.g. <c>FromKilometers(15000)</c>.</summary>
    public static OdometerReading FromKilometers(decimal km) => Create(km, LengthUnit.Kilometer);

    /// <summary>Creates an <see cref="OdometerReading"/> from kilometers, e.g. <c>FromKilometers(15000)</c>.</summary>
    public static OdometerReading FromKilometers(int km) => Create(km, LengthUnit.Kilometer);

    /// <summary>Creates an <see cref="OdometerReading"/> from Swedish miles (1 mil = 10 km), e.g. <c>FromSwedishMiles(1500)</c>.</summary>
    public static OdometerReading FromSwedishMiles(decimal mil) => Create(mil, LengthUnit.SwedishMile);

    /// <summary>Creates an <see cref="OdometerReading"/> from Swedish miles (1 mil = 10 km), e.g. <c>FromSwedishMiles(1500)</c>.</summary>
    public static OdometerReading FromSwedishMiles(int mil) => Create(mil, LengthUnit.SwedishMile);

    /// <summary>Creates an <see cref="OdometerReading"/> from English miles, e.g. <c>FromMiles(9321)</c>.</summary>
    public static OdometerReading FromMiles(decimal mi) => Create(mi, LengthUnit.Mile);

    /// <summary>Creates an <see cref="OdometerReading"/> from English miles, e.g. <c>FromMiles(9321)</c>.</summary>
    public static OdometerReading FromMiles(int mi) => Create(mi, LengthUnit.Mile);

    /// <summary>Creates an <see cref="OdometerReading"/> from meters, e.g. <c>FromMeters(15000000)</c>.</summary>
    public static OdometerReading FromMeters(decimal m) => Create(m, LengthUnit.Meter);

    /// <summary>Creates an <see cref="OdometerReading"/> from meters, e.g. <c>FromMeters(15000000)</c>.</summary>
    public static OdometerReading FromMeters(int m) => Create(m, LengthUnit.Meter);

    public static bool TryParse(string? input, out OdometerReading? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare < 0) return false;
            var dist = Length.FromKilometers(bare);
            result = new OdometerReading(dist, $"{FormatDecimal(bare)} km");
            return true;
        }

        if (!Length.TryParse(trimmed, out var parsed) || parsed is null) return false;
        if (parsed.Meters < 0) return false;

        result = new OdometerReading(parsed);
        return true;
    }

    public static OdometerReading Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid odometer reading.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form, e.g. <c>15000 km</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, LengthUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.Distance.ToString(unit ?? r.Distance.OriginalUnit, decimals);
            return r.Value;
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>Returns the value in kilometers, e.g. <c>15000 km</c>.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            var formatted = FormatDecimal(r.Kilometers);
            return $"{formatted} km";
        }
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the reading in kilometers, e.g. <c>15000 km</c>.</summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(Kilometers);
        return $"{formatted} km";
    }

    public override string ToString() => Value;

    /// <summary>
    /// Returns the value formatted in the most human-readable metric unit, e.g. <c>15 km</c> instead of <c>15000 m</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => Distance.ToNaturalString(decimals);

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static OdometerReading operator +(OdometerReading a, OdometerReading b) => new(a.Distance + b.Distance);
    public static OdometerReading operator -(OdometerReading a, OdometerReading b) => new(a.Distance - b.Distance);
    public static OdometerReading operator *(OdometerReading a, decimal factor) => new(a.Distance * factor);
    public static OdometerReading operator *(decimal factor, OdometerReading a) => new(a.Distance * factor);
    public static OdometerReading operator /(OdometerReading a, decimal divisor) => new(a.Distance / divisor);
    public static OdometerReading operator -(OdometerReading a) => new(-a.Distance);

    public int CompareTo(OdometerReading? other) => other is null ? 1 : Distance.Meters.CompareTo(other.Distance.Meters);
    public bool Equals(OdometerReading? other) => other is not null && Distance.Meters == other.Distance.Meters;
    public override bool Equals(object? obj) => obj is OdometerReading other && Equals(other);
    public override int GetHashCode() => Distance.Meters.GetHashCode();
    public static bool operator ==(OdometerReading? a, OdometerReading? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(OdometerReading? a, OdometerReading? b) => !(a == b);
    public static bool operator <(OdometerReading a, OdometerReading b) => a.CompareTo(b) < 0;
    public static bool operator >(OdometerReading a, OdometerReading b) => a.CompareTo(b) > 0;
    public static bool operator <=(OdometerReading a, OdometerReading b) => a.CompareTo(b) <= 0;
    public static bool operator >=(OdometerReading a, OdometerReading b) => a.CompareTo(b) >= 0;
}
