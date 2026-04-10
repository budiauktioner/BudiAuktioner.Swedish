using System.Globalization;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Product;

/// <summary>
/// Battery capacity (<c>batterikapacitet</c>) in either charge (mAh/Ah) or energy (Wh/kWh) units.
/// Bare numbers are interpreted as mAh.
/// </summary>
public sealed class BatteryCapacity : IEquatable<BatteryCapacity>, IComparable<BatteryCapacity>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Battery Capacity", "Batterikapacitet", "🔋", []);

    /// <summary>The charge value, if parsed from charge units (mAh, Ah). Null when parsed from energy units.</summary>
    public ElectricCharge? Charge { get; }

    /// <summary>The energy value, if parsed from energy units (Wh, kWh). Null when parsed from charge units.</summary>
    public Energy? EnergyValue { get; }

    /// <summary>Display form, e.g. <c>5000 mAh</c> or <c>50 Wh</c>.</summary>
    public string Value { get; }

    private BatteryCapacity(ElectricCharge charge, string displayValue)
    {
        Charge = charge;
        Value = displayValue;
    }

    private BatteryCapacity(Energy energy, string displayValue)
    {
        EnergyValue = energy;
        Value = displayValue;
    }

    /// <summary>Creates a <see cref="BatteryCapacity"/> from milliampere-hours, e.g. <c>FromMilliampereHours(5000)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mAh"/> is not positive.</exception>
    public static BatteryCapacity FromMilliampereHours(decimal mAh)
    {
        if (mAh <= 0) throw new ArgumentOutOfRangeException(nameof(mAh), "Battery capacity must be positive.");
        var charge = ElectricCharge.FromMilliampereHours(mAh);
        return new BatteryCapacity(charge, charge.ToString());
    }

    /// <summary>Creates a <see cref="BatteryCapacity"/> from milliampere-hours, e.g. <c>FromMilliampereHours(5000)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mAh"/> is not positive.</exception>
    public static BatteryCapacity FromMilliampereHours(int mAh) => FromMilliampereHours((decimal)mAh);

    /// <summary>Creates a <see cref="BatteryCapacity"/> from ampere-hours, e.g. <c>FromAmpereHours(5)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ah"/> is not positive.</exception>
    public static BatteryCapacity FromAmpereHours(decimal ah)
    {
        if (ah <= 0) throw new ArgumentOutOfRangeException(nameof(ah), "Battery capacity must be positive.");
        var charge = ElectricCharge.FromAmpereHours(ah);
        return new BatteryCapacity(charge, charge.ToString());
    }

    /// <summary>Creates a <see cref="BatteryCapacity"/> from ampere-hours, e.g. <c>FromAmpereHours(5)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ah"/> is not positive.</exception>
    public static BatteryCapacity FromAmpereHours(int ah) => FromAmpereHours((decimal)ah);

    /// <summary>Creates a <see cref="BatteryCapacity"/> from watt-hours, e.g. <c>FromWattHours(50)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="wh"/> is not positive.</exception>
    public static BatteryCapacity FromWattHours(decimal wh)
    {
        if (wh <= 0) throw new ArgumentOutOfRangeException(nameof(wh), "Battery capacity must be positive.");
        var energy = Energy.FromWattHours(wh);
        return new BatteryCapacity(energy, energy.ToString());
    }

    /// <summary>Creates a <see cref="BatteryCapacity"/> from watt-hours, e.g. <c>FromWattHours(50)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="wh"/> is not positive.</exception>
    public static BatteryCapacity FromWattHours(int wh) => FromWattHours((decimal)wh);

    /// <summary>Creates a <see cref="BatteryCapacity"/> from kilowatt-hours, e.g. <c>FromKilowattHours(75)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="kwh"/> is not positive.</exception>
    public static BatteryCapacity FromKilowattHours(decimal kwh)
    {
        if (kwh <= 0) throw new ArgumentOutOfRangeException(nameof(kwh), "Battery capacity must be positive.");
        var energy = Energy.FromKilowattHours(kwh);
        return new BatteryCapacity(energy, energy.ToString());
    }

    /// <summary>Creates a <see cref="BatteryCapacity"/> from kilowatt-hours, e.g. <c>FromKilowattHours(75)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="kwh"/> is not positive.</exception>
    public static BatteryCapacity FromKilowattHours(int kwh) => FromKilowattHours((decimal)kwh);

    public static bool TryParse(string? input, out BatteryCapacity? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        // Bare number → mAh
        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            var charge = ElectricCharge.Parse($"{FormatDecimal(bare)} mAh");
            result = new BatteryCapacity(charge, $"{FormatDecimal(bare)} mAh");
            return true;
        }

        // Try charge units first (mAh, Ah)
        if (ElectricCharge.TryParse(trimmed, out var parsedCharge) && parsedCharge is not null)
        {
            if (parsedCharge.AmpereHours <= 0) return false;
            result = new BatteryCapacity(parsedCharge, parsedCharge.ToString());
            return true;
        }

        // Try energy units (Wh, kWh)
        if (Energy.TryParse(trimmed, out var parsedEnergy) && parsedEnergy is not null)
        {
            if (parsedEnergy.Joules <= 0) return false;
            result = new BatteryCapacity(parsedEnergy, parsedEnergy.ToString());
            return true;
        }

        return false;
    }

    public static BatteryCapacity Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid battery capacity.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Returns display form, e.g. <c>5000 mAh</c> or <c>50 Wh</c>.</summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>Returns the value in base units.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    public string ToNormalizedString()
    {
        if (Charge is not null) return Charge.ToNormalizedString();
        return EnergyValue!.ToNormalizedString();
    }

    public override string ToString() => Value;

    /// <summary>
    /// Returns the value formatted in the most human-readable unit, e.g. <c>5000 mAh</c> or <c>50 Wh</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) =>
        Charge is not null ? Charge.ToNaturalString(decimals) : EnergyValue!.ToNaturalString(decimals);

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static BatteryCapacity operator +(BatteryCapacity a, BatteryCapacity b)
    {
        if (a.Charge is not null && b.Charge is not null)
        {
            var sum = a.Charge + b.Charge;
            return new BatteryCapacity(sum, sum.ToString());
        }
        if (a.EnergyValue is not null && b.EnergyValue is not null)
        {
            var sum = a.EnergyValue + b.EnergyValue;
            return new BatteryCapacity(sum, sum.ToString());
        }
        throw new InvalidOperationException("Cannot combine charge-based and energy-based battery capacities.");
    }

    public static BatteryCapacity operator -(BatteryCapacity a, BatteryCapacity b)
    {
        if (a.Charge is not null && b.Charge is not null)
        {
            var diff = a.Charge - b.Charge;
            return new BatteryCapacity(diff, diff.ToString());
        }
        if (a.EnergyValue is not null && b.EnergyValue is not null)
        {
            var diff = a.EnergyValue - b.EnergyValue;
            return new BatteryCapacity(diff, diff.ToString());
        }
        throw new InvalidOperationException("Cannot combine charge-based and energy-based battery capacities.");
    }

    public static BatteryCapacity operator *(BatteryCapacity a, decimal factor)
    {
        if (a.Charge is not null) { var r = a.Charge * factor; return new BatteryCapacity(r, r.ToString()); }
        var e = a.EnergyValue! * factor; return new BatteryCapacity(e, e.ToString());
    }

    public static BatteryCapacity operator *(decimal factor, BatteryCapacity a) => a * factor;

    public static BatteryCapacity operator /(BatteryCapacity a, decimal divisor)
    {
        if (a.Charge is not null) { var r = a.Charge / divisor; return new BatteryCapacity(r, r.ToString()); }
        var e = a.EnergyValue! / divisor; return new BatteryCapacity(e, e.ToString());
    }

    public static BatteryCapacity operator -(BatteryCapacity a)
    {
        if (a.Charge is not null) { var r = -a.Charge; return new BatteryCapacity(r, r.ToString()); }
        var e = -a.EnergyValue!; return new BatteryCapacity(e, e.ToString());
    }

    public bool Equals(BatteryCapacity? other)
    {
        if (other is null) return false;
        if (Charge is not null && other.Charge is not null)
            return Charge.AmpereHours == other.Charge.AmpereHours;
        if (EnergyValue is not null && other.EnergyValue is not null)
            return EnergyValue.Joules == other.EnergyValue.Joules;
        return false;
    }

    public override bool Equals(object? obj) => obj is BatteryCapacity other && Equals(other);
    public override int GetHashCode() => Charge?.AmpereHours.GetHashCode() ?? EnergyValue?.Joules.GetHashCode() ?? 0;
    public static bool operator ==(BatteryCapacity? a, BatteryCapacity? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(BatteryCapacity? a, BatteryCapacity? b) => !(a == b);
    public int CompareTo(BatteryCapacity? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(BatteryCapacity left, BatteryCapacity right) => left.CompareTo(right) < 0;
    public static bool operator >(BatteryCapacity left, BatteryCapacity right) => left.CompareTo(right) > 0;
    public static bool operator <=(BatteryCapacity left, BatteryCapacity right) => left.CompareTo(right) <= 0;
    public static bool operator >=(BatteryCapacity left, BatteryCapacity right) => left.CompareTo(right) >= 0;
}
