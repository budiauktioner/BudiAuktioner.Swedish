using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Extension methods for masking vehicle identifiers in display strings.
/// </summary>
public static class VehicleMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked registration number, e.g. <c>ABC 123</c> → <c>*** ***</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishVehicleRegistrationNumber regNumber)
    {
        var letters = new string(MaskChar, regNumber.Letters.Length);
        var suffix = new string(MaskChar, regNumber.Suffix.Length);
        return $"{letters} {suffix}";
    }

    /// <summary>
    /// Returns a masked VIN showing only the WMI (manufacturer) and masking the rest,
    /// e.g. <c>WBA3A5C55CF256789</c> → <c>WBA**************</c>.
    /// </summary>
    public static string ToMaskedString(this VehicleIdentificationNumber vin)
    {
        var wmi = vin.Wmi;
        return $"{wmi}{new string(MaskChar, vin.Value.Length - wmi.Length)}";
    }

    /// <summary>
    /// Returns a masked operating hours reading, e.g. <c>1234 h</c> → <c>**** h</c>.
    /// </summary>
    public static string ToMaskedString(this OperatingHours hours) =>
        $"{new string(MaskChar, hours.Hours.ToString(System.Globalization.CultureInfo.InvariantCulture).Length)} h";

    /// <summary>
    /// Returns a masked bolt pattern, e.g. <c>5 x 114.3</c> → <c>* x *****</c>.
    /// </summary>
    public static string ToMaskedString(this BoltPattern bp) =>
        $"{new string(MaskChar, bp.BoltCount.ToString().Length)} x {new string(MaskChar, bp.PitchCircleDiameter.ToString(System.Globalization.CultureInfo.InvariantCulture).Length)}";

    /// <summary>
    /// Returns a masked tire dimension with construction letter and commercial <c>C</c> visible,
    /// e.g. <c>205/55R16</c> → <c>***/**R**</c>, <c>385/65R22.5</c> → <c>***/**R****</c>.
    /// Load-related suffixes (load index, dual load, speed rating) are fully masked.
    /// </summary>
    public static string ToMaskedString(this TireDimension dimension)
    {
        var rimMaskLen = dimension.RimDiameterInches % 1 == 0 ? 2 : 4;
        var core = $"{new string(MaskChar, 3)}/{new string(MaskChar, 2)}{dimension.Construction}{new string(MaskChar, rimMaskLen)}";
        if (dimension.IsCommercial) core += "C";
        if (dimension.LoadIndex is null) return core;
        var loadPart = dimension.DualLoadIndex.HasValue
            ? $"{dimension.LoadIndex}/{dimension.DualLoadIndex}"
            : $"{dimension.LoadIndex}";
        if (dimension.SpeedRating.HasValue) loadPart += dimension.SpeedRating.Value;
        var suffix = $" {loadPart}";
        return $"{core}{new string(MaskChar, suffix.Length)}";
    }

    /// <summary>
    /// Returns a masked wheel rim dimension, e.g. <c>18 x 7 J</c> → <c>** x * J</c>.
    /// </summary>
    public static string ToMaskedString(this WheelRimDimension dim)
    {
        var d = new string(MaskChar, dim.DiameterInches.ToString(System.Globalization.CultureInfo.InvariantCulture).Length);
        var w = new string(MaskChar, dim.WidthInches.ToString(System.Globalization.CultureInfo.InvariantCulture).Length);
        return dim.FlangeType.Length > 0 ? $"{d} x {w} {dim.FlangeType}" : $"{d} x {w}";
    }

    /// <summary>
    /// Returns a masked EU type-approval number showing the e-mark country code,
    /// e.g. <c>e9*2007/46*6364*09</c> → <c>e9*****</c>.
    /// </summary>
    public static string ToMaskedString(this EuTypeApprovalNumber approval) =>
        $"e{approval.ApprovalCountryCode}{new string(MaskChar, approval.Value.Length - $"e{approval.ApprovalCountryCode}".Length)}";

    /// <summary>
    /// Returns a masked engine power, e.g. <c>150 hp</c> → <c>*** hp</c>.
    /// </summary>
    public static string ToMaskedString(this EnginePower ep) =>
        ep.PowerValue.ToMaskedString();

    /// <summary>
    /// Returns a masked fuel consumption, e.g. <c>8.3 l/100km</c> → <c>*** l/100km</c>.
    /// </summary>
    public static string ToMaskedString(this FuelConsumption fc)
    {
        var parts = fc.Value.Split(' ', 2);
        return parts.Length == 2 ? $"{new string(MaskChar, 3)} {parts[1]}" : new string(MaskChar, fc.Value.Length);
    }

    /// <summary>
    /// Returns a masked emission rate, e.g. <c>221 g/km</c> → <c>*** g/km</c>.
    /// </summary>
    public static string ToMaskedString(this EmissionRate er)
    {
        var parts = er.Value.Split(' ', 2);
        return parts.Length == 2 ? $"{new string(MaskChar, 3)} {parts[1]}" : new string(MaskChar, er.Value.Length);
    }
}
