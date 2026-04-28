namespace Buildi.Primitives.Measurement;

/// <summary>
/// Extension methods for masking measurement values in display strings.
/// </summary>
public static class MeasurementMaskingExtensions
{
    internal const string MaskedNumber = "***";

    /// <summary>Returns a masked length, e.g. <c>10 km</c> → <c>*** km</c>.</summary>
    public static string ToMaskedString(this Length length)
        => $"{MaskedNumber} {length.OriginalUnit.Symbol}";

    /// <summary>Returns a masked speed, e.g. <c>100 km/h</c> → <c>*** km/h</c>.</summary>
    public static string ToMaskedString(this Speed speed)
        => $"{MaskedNumber} {speed.OriginalUnit.Symbol}";

    /// <summary>Returns a masked power, e.g. <c>5 kW</c> → <c>*** kW</c>.</summary>
    public static string ToMaskedString(this Power power)
        => $"{MaskedNumber} {power.OriginalUnit.Symbol}";

    /// <summary>Returns a masked area, e.g. <c>2 ha</c> → <c>*** ha</c>.</summary>
    public static string ToMaskedString(this Area area)
        => $"{MaskedNumber} {area.OriginalUnit.Symbol}";

    /// <summary>Returns a masked volume, e.g. <c>2 L</c> → <c>*** L</c>.</summary>
    public static string ToMaskedString(this Volume volume)
        => $"{MaskedNumber} {volume.OriginalUnit.Symbol}";

    /// <summary>Returns a masked voltage, e.g. <c>230 kV</c> → <c>*** kV</c>.</summary>
    public static string ToMaskedString(this Voltage voltage)
        => $"{MaskedNumber} {voltage.OriginalUnit.Symbol}";

    /// <summary>Returns a masked flow rate, e.g. <c>10 L/min</c> → <c>*** L/min</c>.</summary>
    public static string ToMaskedString(this FlowRate flowRate)
        => $"{MaskedNumber} {flowRate.OriginalUnit.Symbol}";

    /// <summary>Returns a masked frequency, e.g. <c>2.4 GHz</c> → <c>*** GHz</c>.</summary>
    public static string ToMaskedString(this Frequency frequency)
        => $"{MaskedNumber} {frequency.OriginalUnit.Symbol}";

    /// <summary>Returns a masked energy, e.g. <c>5 kWh</c> → <c>*** kWh</c>.</summary>
    public static string ToMaskedString(this Energy energy)
        => $"{MaskedNumber} {energy.OriginalUnit.Symbol}";

    /// <summary>Returns a masked electric charge, e.g. <c>5000 mAh</c> → <c>*** mAh</c>.</summary>
    public static string ToMaskedString(this ElectricCharge charge)
        => $"{MaskedNumber} {charge.OriginalUnit.Symbol}";

    /// <summary>Returns a masked electric current, e.g. <c>500 mA</c> → <c>*** mA</c>.</summary>
    public static string ToMaskedString(this ElectricCurrent current)
        => $"{MaskedNumber} {current.OriginalUnit.Symbol}";

    /// <summary>Returns a masked weight, e.g. <c>10 kg</c> → <c>*** kg</c>.</summary>
    public static string ToMaskedString(this Weight weight)
        => $"{MaskedNumber} {weight.OriginalUnit.Symbol}";

    /// <summary>Returns a masked torque, e.g. <c>250 Nm</c> → <c>*** Nm</c>.</summary>
    public static string ToMaskedString(this Torque torque)
        => $"{MaskedNumber} {torque.OriginalUnit.Symbol}";

    /// <summary>Returns a masked pressure, e.g. <c>1013 hPa</c> → <c>*** hPa</c>.</summary>
    public static string ToMaskedString(this Pressure pressure)
        => $"{MaskedNumber} {pressure.OriginalUnit.Symbol}";

    /// <summary>Returns a masked temperature, e.g. <c>20 °C</c> → <c>*** °C</c>.</summary>
    public static string ToMaskedString(this Temperature temperature)
        => $"{MaskedNumber} {temperature.OriginalUnit.Symbol}";

    /// <summary>Returns a masked data size, e.g. <c>10 MB</c> → <c>*** MB</c>.</summary>
    public static string ToMaskedString(this DataSize dataSize)
        => $"{MaskedNumber} {dataSize.OriginalUnit.Symbol}";

    /// <summary>Returns a masked luminous flux, e.g. <c>800 lm</c> → <c>*** lm</c>.</summary>
    public static string ToMaskedString(this LuminousFlux flux)
        => $"{MaskedNumber} {flux.OriginalUnit.Symbol}";

    /// <summary>Returns a masked percentage, e.g. <c>85%</c> → <c>***%</c>.</summary>
    public static string ToMaskedString(this Percentage percentage)
        => $"{MaskedNumber}%";

    /// <summary>Returns a masked rotational speed, e.g. <c>5200 rpm</c> → <c>*** rpm</c>.</summary>
    public static string ToMaskedString(this RotationalSpeed rs)
        => $"{MaskedNumber} {rs.OriginalUnit.Symbol}";

    /// <summary>Returns a masked count, e.g. <c>5 st</c> → <c>*** st</c>.</summary>
    public static string ToMaskedString(this Count count)
        => $"{MaskedNumber} st";

    /// <summary>Returns a masked year, e.g. <c>2024</c> → <c>****</c>.</summary>
    public static string ToMaskedString(this Year year)
        => new('*', 4);

    /// <summary>Returns a masked year-month, e.g. <c>2026-07</c> → <c>****-**</c>.</summary>
    public static string ToMaskedString(this YearMonth yearMonth)
        => $"{new string('*', 4)}-{new string('*', 2)}";

    /// <summary>Returns a masked sound level, e.g. <c>69 dB(A)</c> → <c>*** dB(A)</c>.</summary>
    public static string ToMaskedString(this SoundLevel sl)
    {
        var suffix = sl.Weighting switch
        {
            SoundWeighting.A => " dB(A)",
            SoundWeighting.B => " dB(B)",
            SoundWeighting.C => " dB(C)",
            SoundWeighting.Z => " dB(Z)",
            _ => " dB"
        };
        return $"{MaskedNumber}{suffix}";
    }
}
