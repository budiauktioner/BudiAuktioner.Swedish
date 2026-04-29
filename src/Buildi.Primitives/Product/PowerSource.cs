using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Power source (<c>strömkälla</c> / <c>drivkälla</c>) of a device, machine, or appliance,
/// e.g. <c>Electric</c>, <c>Battery</c>, <c>Diesel</c>, <c>Petrol</c>, <c>Hybrid</c>, <c>Manual</c>.
/// </summary>
/// <remarks>
/// <para>Generic power-source classification used in product feeds for tools, appliances, garden
/// equipment, and outdoor gear. Complements <see cref="Buildi.Primitives.Vehicle.FuelType"/>,
/// which is the road-vehicle–specific drivmedel taxonomy with Transportstyrelsen codes.
/// Each entry exposes <see cref="IsElectric"/>, <see cref="IsCombustion"/>, and
/// <see cref="RequiresFuel"/> for filtering.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://schema.org/PowerSupplySpecification">Schema.org — PowerSupplySpecification</see></description></item>
/// </list>
/// </remarks>
public sealed class PowerSource : IEquatable<PowerSource>, IComparable<PowerSource>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Power Source",
        "Strömkälla",
        "🔌",
        ["https://schema.org/PowerSupplySpecification"]);

    private static readonly Lazy<Dictionary<string, PowerSource>> Lookup = new(BuildLookup);

    private readonly int _order;

    /// <summary>Canonical English value, e.g. <c>Electric</c>, <c>Battery</c>, <c>Petrol</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Mains electric</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>El</c>, <c>Batteri</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>
    /// Returns <see langword="true"/> when the power source is electrical
    /// (mains <c>Electric</c>, <c>Battery</c>, <c>Solar</c>, or <c>Hybrid</c>).
    /// </summary>
    public bool IsElectric { get; }

    /// <summary>
    /// Returns <see langword="true"/> for internal-combustion / hydrocarbon fuels
    /// (<c>Diesel</c>, <c>Petrol</c>, <c>Hybrid</c>).
    /// </summary>
    public bool IsCombustion { get; }

    /// <summary>
    /// Returns <see langword="true"/> when operation requires consumable fuel
    /// (<c>Diesel</c>, <c>Petrol</c>, <c>Hybrid</c>, <c>Hydrogen</c>).
    /// </summary>
    public bool RequiresFuel { get; }

    public static readonly PowerSource Electric  = new("Electric",  "Mains electric",   "El",          isElectric: true,  isCombustion: false, requiresFuel: false, 0);
    public static readonly PowerSource Battery   = new("Battery",   "Battery-powered",  "Batteri",     isElectric: true,  isCombustion: false, requiresFuel: false, 1);
    public static readonly PowerSource Solar     = new("Solar",     "Solar-powered",    "Solenergi",   isElectric: true,  isCombustion: false, requiresFuel: false, 2);
    public static readonly PowerSource Hybrid    = new("Hybrid",    "Hybrid",           "Hybrid",      isElectric: true,  isCombustion: true,  requiresFuel: true,  3);
    public static readonly PowerSource Petrol    = new("Petrol",    "Petrol",           "Bensin",      isElectric: false, isCombustion: true,  requiresFuel: true,  4);
    public static readonly PowerSource Diesel    = new("Diesel",    "Diesel",           "Diesel",      isElectric: false, isCombustion: true,  requiresFuel: true,  5);
    public static readonly PowerSource Hydrogen  = new("Hydrogen",  "Hydrogen",         "Vätgas",      isElectric: true,  isCombustion: false, requiresFuel: true,  6);
    public static readonly PowerSource Pneumatic = new("Pneumatic", "Pneumatic (air)",  "Pneumatisk",  isElectric: false, isCombustion: false, requiresFuel: false, 7);
    public static readonly PowerSource Hydraulic = new("Hydraulic", "Hydraulic",        "Hydraulisk",  isElectric: false, isCombustion: false, requiresFuel: false, 8);
    public static readonly PowerSource Manual    = new("Manual",    "Manual",           "Manuell",     isElectric: false, isCombustion: false, requiresFuel: false, 9);

    /// <summary>All predefined power sources.</summary>
    public static IReadOnlyList<PowerSource> All { get; } =
    [
        Electric, Battery, Solar, Hybrid, Petrol, Diesel, Hydrogen, Pneumatic, Hydraulic, Manual
    ];

    private PowerSource(string value, string englishName, string localizedName, bool isElectric, bool isCombustion, bool requiresFuel, int order)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        IsElectric = isElectric;
        IsCombustion = isCombustion;
        RequiresFuel = requiresFuel;
        _order = order;
    }

    /// <summary>
    /// Attempts to parse a power source from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out PowerSource? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var v))
        {
            result = v;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Parses a power source. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static PowerSource Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid power source.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized power source.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>El</c> (Swedish) or
    /// <c>Mains electric</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Electric</c>, <c>Petrol</c>, <c>Hybrid</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical value.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical English value, e.g. <c>Electric</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_/().]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, PowerSource> d, PowerSource value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, PowerSource> BuildLookup()
    {
        var d = new Dictionary<string, PowerSource>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in All)
        {
            AddKey(d, p, p.Value);
            AddKey(d, p, p.EnglishName);
            AddKey(d, p, p.LocalizedName);
        }

        AddKey(d, Electric, "Electricity");
        AddKey(d, Electric, "Elektrisk");
        AddKey(d, Electric, "Eldriven");
        AddKey(d, Electric, "Mains");
        AddKey(d, Electric, "Mains-powered");
        AddKey(d, Electric, "Corded");
        AddKey(d, Electric, "Corded electric");
        AddKey(d, Electric, "Plug-in");
        AddKey(d, Electric, "Nät");
        AddKey(d, Electric, "Nätansluten");
        AddKey(d, Electric, "Nätström");
        AddKey(d, Electric, "Sladdansluten");
        AddKey(d, Electric, "230V");
        AddKey(d, Electric, "400V");

        AddKey(d, Battery, "Battery powered");
        AddKey(d, Battery, "Batteridriven");
        AddKey(d, Battery, "Cordless");
        AddKey(d, Battery, "Sladdlös");
        AddKey(d, Battery, "Rechargeable");
        AddKey(d, Battery, "Uppladdningsbart batteri");
        AddKey(d, Battery, "Accu");

        AddKey(d, Solar, "Solar powered");
        AddKey(d, Solar, "Solar power");
        AddKey(d, Solar, "Solcell");
        AddKey(d, Solar, "Soldriven");
        AddKey(d, Solar, "Solpanel");
        AddKey(d, Solar, "PV");

        AddKey(d, Hybrid, "Hybriddrift");
        AddKey(d, Hybrid, "Plug-in hybrid");
        AddKey(d, Hybrid, "PHEV");
        AddKey(d, Hybrid, "HEV");
        AddKey(d, Hybrid, "MHEV");
        AddKey(d, Hybrid, "Mild hybrid");
        AddKey(d, Hybrid, "Mildhybrid");
        AddKey(d, Hybrid, "Laddhybrid");

        AddKey(d, Petrol, "Bensindriven");
        AddKey(d, Petrol, "Bensinmotor");
        AddKey(d, Petrol, "Gasoline");
        AddKey(d, Petrol, "Gas");
        AddKey(d, Petrol, "Petrol engine");

        AddKey(d, Diesel, "Dieseldriven");
        AddKey(d, Diesel, "Dieselmotor");
        AddKey(d, Diesel, "Diesel engine");

        AddKey(d, Hydrogen, "Vätgasdriven");
        AddKey(d, Hydrogen, "H2");
        AddKey(d, Hydrogen, "Fuel cell");
        AddKey(d, Hydrogen, "Bränslecell");
        AddKey(d, Hydrogen, "FCEV");

        AddKey(d, Pneumatic, "Pneumatik");
        AddKey(d, Pneumatic, "Air-powered");
        AddKey(d, Pneumatic, "Air powered");
        AddKey(d, Pneumatic, "Tryckluft");
        AddKey(d, Pneumatic, "Tryckluftsdriven");
        AddKey(d, Pneumatic, "Compressed air");
        AddKey(d, Pneumatic, "Komprimerad luft");

        AddKey(d, Hydraulic, "Hydraulik");
        AddKey(d, Hydraulic, "Hydrauliskt");
        AddKey(d, Hydraulic, "Hydrauldriven");

        AddKey(d, Manual, "Manuellt");
        AddKey(d, Manual, "Manuell drift");
        AddKey(d, Manual, "Hand-driven");
        AddKey(d, Manual, "Handdriven");
        AddKey(d, Manual, "Muskelkraft");
        AddKey(d, Manual, "Human-powered");
        AddKey(d, Manual, "Pedal");
        AddKey(d, Manual, "Pedaldriven");
        AddKey(d, Manual, "No power");
        AddKey(d, Manual, "Strömlös");

        return d;
    }

    public static bool operator ==(PowerSource? a, PowerSource? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(PowerSource? a, PowerSource? b) => !(a == b);

    public bool Equals(PowerSource? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is PowerSource other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(PowerSource? other) =>
        other is null ? 1 : _order.CompareTo(other._order);

    public static bool operator <(PowerSource a, PowerSource b) => a.CompareTo(b) < 0;
    public static bool operator >(PowerSource a, PowerSource b) => a.CompareTo(b) > 0;
    public static bool operator <=(PowerSource a, PowerSource b) => a.CompareTo(b) <= 0;
    public static bool operator >=(PowerSource a, PowerSource b) => a.CompareTo(b) >= 0;
}
