using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Swedish vehicle type classification (<c>fordonsslag</c>) as used by Transportstyrelsen.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — fordonsklassificering</description></item>
/// </list>
/// </remarks>
public sealed class SwedishVehicleType : IEquatable<SwedishVehicleType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Vehicle Type", "Fordonsslag", "🚙", ["https://www.transportstyrelsen.se/"]);

    private static readonly Lazy<Dictionary<string, SwedishVehicleType>> Lookup = new(BuildLookup);
    private static readonly Regex WhitespaceRun = new(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Canonical Transportstyrelsen code, e.g. <c>PB</c>.</summary>
    public string Value { get; }

    /// <summary>Short code from Transportstyrelsen, same as <see cref="Value"/>.</summary>
    public string Code => Value;

    /// <summary>English display name, e.g. <c>Passenger car</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Personbil</c>.</summary>
    public string LocalizedName { get; }

    public static readonly SwedishVehicleType PassengerCar = new("PB", "Passenger car", "Personbil");
    public static readonly SwedishVehicleType Truck = new("LB", "Truck", "Lastbil");
    public static readonly SwedishVehicleType Bus = new("BU", "Bus", "Buss");
    public static readonly SwedishVehicleType Motorcycle = new("MC", "Motorcycle", "Motorcykel");
    public static readonly SwedishVehicleType MopedClassI = new("MR", "Moped class I", "Moped klass I");
    public static readonly SwedishVehicleType MopedClassII = new("EU-M", "Moped class II", "EU-moped");
    public static readonly SwedishVehicleType Trailer = new("SL", "Trailer", "Släpvagn");
    public static readonly SwedishVehicleType SemiTrailer = new("SA", "Semi-trailer", "Påhängsvagn");
    public static readonly SwedishVehicleType Caravan = new("HV", "Caravan", "Husvagn");
    public static readonly SwedishVehicleType Motorhome = new("HB", "Motorhome", "Husbil");
    public static readonly SwedishVehicleType AllTerrainVehicle = new("TK", "All-terrain vehicle", "Terrängvagn");
    public static readonly SwedishVehicleType Snowmobile = new("TM", "Snowmobile", "Terrängskoter");
    public static readonly SwedishVehicleType Atv = new("TH", "ATV", "Terränghjuling");
    public static readonly SwedishVehicleType ATractor = new("AT", "A-tractor", "A-traktor");
    public static readonly SwedishVehicleType PowerVehicleI = new("EP", "Power vehicle cl. I", "Motorredskap klass I");
    public static readonly SwedishVehicleType PowerVehicleII = new("MRK2", "Power vehicle cl. II", "Motorredskap klass II");
    public static readonly SwedishVehicleType LightTruck = new("LL", "Light truck", "Lätt lastbil");

    /// <summary>All predefined vehicle types.</summary>
    public static IReadOnlyList<SwedishVehicleType> All { get; } =
    [
        PassengerCar, Truck, Bus, Motorcycle, MopedClassI, MopedClassII,
        Trailer, SemiTrailer, Caravan, Motorhome, AllTerrainVehicle,
        Snowmobile, Atv, ATractor, PowerVehicleI, PowerVehicleII, LightTruck
    ];

    private SwedishVehicleType(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    /// <summary>
    /// Attempts to parse a vehicle type code, Swedish name, or English name (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out SwedishVehicleType? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var key = NormalizeLookupKey(InputSanitization.SanitizeInput(input!));
        return Lookup.Value.TryGetValue(key, out result);
    }

    public static SwedishVehicleType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid Swedish vehicle type.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the locale-dependent display name, e.g. <c>Personbil</c> or <c>Passenger car</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.ToString();
        if (fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input)) return input!.Trim();
        return null;
    }

    /// <summary>
    /// Returns the canonical Transportstyrelsen code, e.g. <c>PB</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input
    /// (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already equals its canonical code.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical Transportstyrelsen code, e.g. <c>PB</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the locale-dependent display name, e.g. <c>Personbil</c> or <c>Passenger car</c>.</summary>
    public override string ToString() => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    public static bool operator ==(SwedishVehicleType? a, SwedishVehicleType? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishVehicleType? a, SwedishVehicleType? b) => !(a == b);
    public bool Equals(SwedishVehicleType? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is SwedishVehicleType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("ö", "o").Replace("ä", "a").Replace("å", "a").Replace("é", "e");
        folded = folded.Replace('_', ' ');
        return WhitespaceRun.Replace(folded, " ");
    }

    private static void AddKey(Dictionary<string, SwedishVehicleType> d, SwedishVehicleType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d[k] = value;
    }

    private static Dictionary<string, SwedishVehicleType> BuildLookup()
    {
        var d = new Dictionary<string, SwedishVehicleType>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in All)
        {
            AddKey(d, e, e.Value);
            AddKey(d, e, e.EnglishName);
            AddKey(d, e, e.LocalizedName);
        }

        AddKey(d, PassengerCar, "Car");
        AddKey(d, PassengerCar, "Bil");

        AddKey(d, Truck, "Lorry");
        AddKey(d, Truck, "Tung lastbil");

        AddKey(d, Bus, "Coach");
        AddKey(d, Bus, "Linjebuss");
        AddKey(d, Bus, "Turistbuss");

        AddKey(d, Motorcycle, "Motorbike");

        AddKey(d, MopedClassI, "Moped klass 1");
        AddKey(d, MopedClassI, "Moped I");
        AddKey(d, MopedClassI, "Moped 1");
        AddKey(d, MopedClassI, "EU-moped klass I");

        AddKey(d, MopedClassII, "EUM");
        AddKey(d, MopedClassII, "EU moped");
        AddKey(d, MopedClassII, "Moped klass II");
        AddKey(d, MopedClassII, "Moped klass 2");
        AddKey(d, MopedClassII, "Moped II");
        AddKey(d, MopedClassII, "Moped 2");

        AddKey(d, Trailer, "Släp");

        AddKey(d, SemiTrailer, "Semitrailer");

        AddKey(d, Caravan, "Campingvagn");

        AddKey(d, Motorhome, "Camper");
        AddKey(d, Motorhome, "Campingbil");

        AddKey(d, Snowmobile, "Snöskoter");
        AddKey(d, Snowmobile, "Skoter");

        AddKey(d, Atv, "Quad");
        AddKey(d, Atv, "Quadbike");
        AddKey(d, Atv, "Fyrhjuling");

        AddKey(d, ATractor, "Atraktor");
        AddKey(d, ATractor, "A tractor");
        AddKey(d, ATractor, "EPA-traktor");
        AddKey(d, ATractor, "EPA");

        AddKey(d, PowerVehicleI, "Motorredskap klass 1");
        AddKey(d, PowerVehicleI, "Motorredskap I");

        AddKey(d, PowerVehicleII, "Motorredskap klass 2");
        AddKey(d, PowerVehicleII, "Motorredskap II");

        AddKey(d, LightTruck, "Skåpbil");

        return d;
    }
}
