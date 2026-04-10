using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A vehicle fuel or energy type (<c>drivmedel</c>) with canonical English identifier,
/// Swedish display name, and Transportstyrelsen code.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — fordonsregister</description></item>
/// <item><description><see href="https://www.energimyndigheten.se/">Energimyndigheten</see> — drivmedelsinformation</description></item>
/// </list>
/// </remarks>
public sealed class FuelType : IEquatable<FuelType>, IComparable<FuelType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Fuel Type", "Drivmedel", "⛽", ["https://www.transportstyrelsen.se/", "https://www.energimyndigheten.se/"]);

    private static readonly Lazy<Dictionary<string, FuelType>> Lookup = new(BuildLookup);

    private static readonly Regex KeyNormalizationPattern = new(
        @"[\s\-/]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly int _order;

    /// <summary>Canonical English identifier, e.g. <c>Petrol</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Petrol</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Bensin</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Short code used by Transportstyrelsen, e.g. <c>BE</c>.</summary>
    public string Code { get; }

    public static readonly FuelType Petrol       = new("Petrol",         "Petrol",         "Bensin",      "BE",   0);
    public static readonly FuelType Diesel       = new("Diesel",         "Diesel",         "Diesel",      "DI",   1);
    public static readonly FuelType Electric     = new("Electric",       "Electric",       "El",          "EL",   2);
    public static readonly FuelType Ethanol      = new("Ethanol",        "Ethanol",        "Etanol",      "ET",   3);
    public static readonly FuelType NaturalGas   = new("Natural gas",    "Natural gas",    "Naturgas",    "CNG",  4);
    public static readonly FuelType Lpg          = new("LPG",            "LPG",            "Gasol",       "LPG",  5);
    public static readonly FuelType Hybrid       = new("Hybrid",         "Hybrid",         "Elhybrid",    "HEV",  6);
    public static readonly FuelType MildHybrid   = new("Mild hybrid",    "Mild hybrid",    "Mildhybrid",  "MHEV", 7);
    public static readonly FuelType PlugInHybrid = new("Plug-in hybrid", "Plug-in hybrid", "Laddhybrid",  "PHEV", 8);
    public static readonly FuelType Hydrogen     = new("Hydrogen",       "Hydrogen",       "Vätgas",      "H2",   9);
    public static readonly FuelType Biodiesel    = new("Biodiesel",      "Biodiesel",      "Biodiesel",   "BIO",  10);
    public static readonly FuelType Methane      = new("Methane",        "Methane",        "Metangas",    "MET",  11);
    public static readonly FuelType Methanol     = new("Methanol",       "Methanol",       "Metanol",     "M85",  12);
    public static readonly FuelType Hvo          = new("HVO",            "HVO",            "HVO",         "HVO",  13);
    public static readonly FuelType Kerosene     = new("Kerosene",       "Kerosene",       "Fotogen",     "FO",   14);
    public static readonly FuelType Other        = new("Other",          "Other",          "Annat",       "ÖVR",  15);

    /// <summary>All predefined fuel types.</summary>
    public static IReadOnlyList<FuelType> All { get; } =
    [
        Petrol, Diesel, Electric, Ethanol, NaturalGas, Lpg,
        Hybrid, MildHybrid, PlugInHybrid, Hydrogen, Biodiesel, Methane,
        Methanol, Hvo, Kerosene, Other
    ];

    private FuelType(string value, string englishName, string localizedName, string code, int order)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        Code = code;
        _order = order;
    }

    /// <summary>
    /// Attempts to parse a fuel type from a name, alias, or Transportstyrelsen code (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out FuelType? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (key.Length == 0) return false;

        if (Lookup.Value.TryGetValue(key, out var found))
        {
            result = found;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses a fuel type from a name, alias, or Transportstyrelsen code.
    /// Throws <see cref="ArgumentException"/> when the input is not recognized.
    /// </summary>
    public static FuelType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid fuel type.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name for the fuel type: Swedish name (e.g. <c>Bensin</c>) when
    /// <see cref="PrimitivesDefaults.UICulture"/> is Swedish, otherwise English name (e.g. <c>Petrol</c>).
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var ft))
            return PrimitivesDefaults.UseLocalizedDisplayNames ? ft!.LocalizedName : ft!.EnglishName;
        if (fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input))
            return input!.Trim();
        return null;
    }

    /// <summary>
    /// Returns the canonical English identifier, e.g. <c>Petrol</c>, <c>Plug-in hybrid</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input
    /// (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var ft)) return ft!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical English identifier, e.g. <c>Petrol</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>
    /// Returns the locale-aware display name: Swedish name (e.g. <c>Bensin</c>) when
    /// <see cref="PrimitivesDefaults.UICulture"/> is Swedish, otherwise English name (e.g. <c>Petrol</c>).
    /// </summary>
    public override string ToString() =>
        PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        return KeyNormalizationPattern.Replace(folded, " ").Trim();
    }

    private static void AddKey(Dictionary<string, FuelType> d, FuelType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, FuelType> BuildLookup()
    {
        var d = new Dictionary<string, FuelType>(StringComparer.OrdinalIgnoreCase);

        foreach (var ft in All)
        {
            AddKey(d, ft, ft.Value);
            AddKey(d, ft, ft.EnglishName);
            AddKey(d, ft, ft.LocalizedName);
            AddKey(d, ft, ft.Code);
        }

        AddKey(d, Petrol, "Gasoline");
        AddKey(d, Petrol, "Gas");
        AddKey(d, Petrol, "Blyfri");
        AddKey(d, Petrol, "Blyfri bensin");
        AddKey(d, Petrol, "95");
        AddKey(d, Petrol, "98");

        AddKey(d, Diesel, "Dieselolja");
        AddKey(d, Diesel, "Dieselbränsle");

        AddKey(d, Electric, "Elmotor");
        AddKey(d, Electric, "Elektrisk");
        AddKey(d, Electric, "Elbil");
        AddKey(d, Electric, "BEV");
        AddKey(d, Electric, "Battery Electric");

        AddKey(d, Ethanol, "E85");
        AddKey(d, Ethanol, "Flexifuel");
        AddKey(d, Ethanol, "Flex Fuel");
        AddKey(d, Ethanol, "FFV");

        AddKey(d, NaturalGas, "Compressed Natural Gas");
        AddKey(d, NaturalGas, "Fordonsgas");
        AddKey(d, NaturalGas, "Komprimerad naturgas");
        AddKey(d, NaturalGas, "NGV");

        AddKey(d, Lpg, "Liquefied Petroleum Gas");
        AddKey(d, Lpg, "Motorgas");
        AddKey(d, Lpg, "Autogas");
        AddKey(d, Lpg, "Flytande gas");
        AddKey(d, Lpg, "Propan");

        AddKey(d, Hybrid, "Bensin/El");
        AddKey(d, Hybrid, "Hybrid Electric");
        AddKey(d, Hybrid, "Full hybrid");
        AddKey(d, Hybrid, "FHEV");

        AddKey(d, MildHybrid, "Mild hybrid");
        AddKey(d, MildHybrid, "48V");
        AddKey(d, MildHybrid, "48V hybrid");

        AddKey(d, PlugInHybrid, "Plug-in");
        AddKey(d, PlugInHybrid, "Laddbar hybrid");

        AddKey(d, Hydrogen, "Bränslecell");
        AddKey(d, Hydrogen, "Fuel Cell");
        AddKey(d, Hydrogen, "FCEV");
        AddKey(d, Hydrogen, "Vätgasbil");

        AddKey(d, Biodiesel, "B100");
        AddKey(d, Biodiesel, "FAME");
        AddKey(d, Biodiesel, "RME");

        AddKey(d, Methane, "Biogas");
        AddKey(d, Methane, "CBG");
        AddKey(d, Methane, "Metan");
        AddKey(d, Methane, "Biometan");
        AddKey(d, Methane, "LBG");
        AddKey(d, Methane, "Flytande biogas");

        AddKey(d, Hvo, "HVO100");
        AddKey(d, Hvo, "Hydrotreated Vegetable Oil");
        AddKey(d, Hvo, "Förnybar diesel");

        AddKey(d, Kerosene, "Paraffin");

        AddKey(d, Other, "Övrigt");
        AddKey(d, Other, "Övrig");

        return d;
    }

    public static bool operator ==(FuelType? a, FuelType? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(FuelType? a, FuelType? b) => !(a == b);

    public bool Equals(FuelType? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is FuelType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(FuelType? other) => other is null ? 1 : _order.CompareTo(other._order);
    public static bool operator <(FuelType a, FuelType b) => a._order < b._order;
    public static bool operator >(FuelType a, FuelType b) => a._order > b._order;
    public static bool operator <=(FuelType a, FuelType b) => a._order <= b._order;
    public static bool operator >=(FuelType a, FuelType b) => a._order >= b._order;
}
