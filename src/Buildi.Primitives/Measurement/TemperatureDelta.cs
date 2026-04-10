using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A temperature difference (interval), stored in kelvin. For differences, one kelvin equals
/// one degree Celsius; Fahrenheit intervals scale by <c>9/5</c>.
/// </summary>
public sealed class TemperatureDelta : IComparable<TemperatureDelta>, IEquatable<TemperatureDelta>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Temperature Difference", "Temperaturskillnad", "🌡️", []);

    private readonly decimal _deltaKelvin;

    private TemperatureDelta(decimal deltaKelvin) => _deltaKelvin = deltaKelvin;

    /// <summary>The interval in kelvin (same numeric value as <see cref="Celsius"/>).</summary>
    public decimal Kelvin => _deltaKelvin;

    /// <summary>The interval in degrees Celsius (identical to <see cref="Kelvin"/> numerically).</summary>
    public decimal Celsius => _deltaKelvin;

    /// <summary>The interval in degrees Fahrenheit, e.g. a <c>5</c> K interval is <c>9</c> °F.</summary>
    public decimal Fahrenheit => _deltaKelvin * 9m / 5m;

    public static TemperatureDelta FromKelvin(decimal deltaKelvin) => new(deltaKelvin);

    public static TemperatureDelta FromCelsius(decimal deltaCelsius) => new(deltaCelsius);

    public static TemperatureDelta FromFahrenheit(decimal deltaFahrenheit) => new(deltaFahrenheit * 5m / 9m);

    public static TemperatureDelta operator +(TemperatureDelta a, TemperatureDelta b) =>
        new(a._deltaKelvin + b._deltaKelvin);

    public static TemperatureDelta operator -(TemperatureDelta a, TemperatureDelta b) =>
        new(a._deltaKelvin - b._deltaKelvin);

    public static TemperatureDelta operator *(TemperatureDelta a, decimal factor) =>
        new(a._deltaKelvin * factor);

    public static TemperatureDelta operator *(decimal factor, TemperatureDelta a) =>
        new(a._deltaKelvin * factor);

    public static TemperatureDelta operator /(TemperatureDelta a, decimal divisor) =>
        new(a._deltaKelvin / divisor);

    public static TemperatureDelta operator -(TemperatureDelta a) => new(-a._deltaKelvin);

    public static bool operator ==(TemperatureDelta? a, TemperatureDelta? b) => a?._deltaKelvin == b?._deltaKelvin;
    public static bool operator !=(TemperatureDelta? a, TemperatureDelta? b) => !(a == b);
    public static bool operator <(TemperatureDelta a, TemperatureDelta b) => a._deltaKelvin < b._deltaKelvin;
    public static bool operator >(TemperatureDelta a, TemperatureDelta b) => a._deltaKelvin > b._deltaKelvin;
    public static bool operator <=(TemperatureDelta a, TemperatureDelta b) => a._deltaKelvin <= b._deltaKelvin;
    public static bool operator >=(TemperatureDelta a, TemperatureDelta b) => a._deltaKelvin >= b._deltaKelvin;

    public int CompareTo(TemperatureDelta? other) =>
        other is null ? 1 : _deltaKelvin.CompareTo(other._deltaKelvin);

    public bool Equals(TemperatureDelta? other) => other is not null && _deltaKelvin == other._deltaKelvin;

    public override bool Equals(object? obj) => obj is TemperatureDelta other && Equals(other);

    public override int GetHashCode() => _deltaKelvin.GetHashCode();
}
