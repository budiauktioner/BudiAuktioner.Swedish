using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A passenger or commercial vehicle body type (<c>karosstyp</c>),
/// e.g. <c>Sedan</c>, <c>Hatchback</c>, <c>Stationwagon</c>, <c>SUV</c>, <c>Pickup</c>,
/// <c>Truck</c>, <c>LightTruck</c>, <c>Trailer</c>, <c>Motorhome</c>, <c>Tractor</c>,
/// <c>Dumper</c>, <c>Tipper</c>, <c>OffRoad</c>.
/// </summary>
/// <remarks>
/// <para>Captures the body styles in common use on the Swedish car market and on Swedish
/// auction listings for heavy and commercial vehicles, with Swedish-language synonyms
/// (<c>Kombi</c>, <c>Halvkombi</c>, <c>Cabriolet</c>, <c>Suvkombi</c>, <c>Husbil</c>,
/// <c>Släp</c>, <c>Traktor</c>, <c>Tippbil</c>, <c>Bergsdumper</c>, <c>Lätt lastbil</c>,
/// <c>Terrängbil</c>) and English/American synonyms.</para>
/// <para><b>Distinct categories preserved on purpose</b> — these look related but mean
/// different things in the Swedish vehicle register and auction market, and are kept as
/// separate canonicals rather than collapsed into one:</para>
/// <list type="bullet">
/// <item><description><c>Truck</c> (<c>Tung lastbil</c>, &gt; 3.5 t) vs <c>LightTruck</c> (<c>Lätt lastbil</c>, ≤ 3.5 t) — different EU vehicle category and licence requirements.</description></item>
/// <item><description><c>SUV</c> (city/road-oriented) vs <c>OffRoad</c> (<c>Terrängbil</c>, separate Transportstyrelsen vehicle category).</description></item>
/// <item><description><c>Tipper</c> (road-going <c>Tippbil</c> on a truck chassis) vs <c>Dumper</c> (off-road <c>Bergsdumper</c> / articulated dump truck for construction sites).</description></item>
/// </list>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — fordonsregister</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Car_body_style">Wikipedia — Car body style</see></description></item>
/// </list>
/// </remarks>
public sealed class BodyType : IEquatable<BodyType>, IComparable<BodyType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Body Type",
        "Karosstyp",
        "🚘",
        ["https://www.transportstyrelsen.se/", "https://en.wikipedia.org/wiki/Car_body_style"]);

    private static readonly Lazy<Dictionary<string, BodyType>> Lookup = new(BuildLookup);

    /// <summary>Canonical English value, e.g. <c>Sedan</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Sedan</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Sedan</c> / <c>Kombi</c>.</summary>
    public string LocalizedName { get; }

    public static readonly BodyType Sedan = new("Sedan", "Sedan", "Sedan");
    public static readonly BodyType Hatchback = new("Hatchback", "Hatchback", "Halvkombi");
    public static readonly BodyType Stationwagon = new("Stationwagon", "Station wagon", "Kombi");
    public static readonly BodyType Suv = new("SUV", "SUV", "Suvkombi");
    public static readonly BodyType Crossover = new("Crossover", "Crossover", "Crossover");
    public static readonly BodyType Coupe = new("Coupe", "Coupé", "Coupé");
    public static readonly BodyType Convertible = new("Convertible", "Convertible", "Cabriolet");
    public static readonly BodyType Roadster = new("Roadster", "Roadster", "Roadster");
    public static readonly BodyType Mpv = new("MPV", "MPV", "Familjebuss");
    public static readonly BodyType Minivan = new("Minivan", "Minivan", "Minivan");
    public static readonly BodyType Van = new("Van", "Van", "Skåpbil");
    public static readonly BodyType Pickup = new("Pickup", "Pickup", "Pickup");
    public static readonly BodyType Truck = new("Truck", "Truck", "Lastbil");
    public static readonly BodyType Bus = new("Bus", "Bus", "Buss");
    public static readonly BodyType Microcar = new("Microcar", "Microcar", "Mikrobil");
    public static readonly BodyType Limousine = new("Limousine", "Limousine", "Limousin");
    public static readonly BodyType Targa = new("Targa", "Targa", "Targa");
    public static readonly BodyType Trailer = new("Trailer", "Trailer", "Släp");
    public static readonly BodyType Motorhome = new("Motorhome", "Motorhome", "Husbil");
    public static readonly BodyType Tractor = new("Tractor", "Tractor", "Traktor");
    public static readonly BodyType Tipper = new("Tipper", "Tipper", "Tippbil");
    public static readonly BodyType Dumper = new("Dumper", "Dumper", "Dumper");
    public static readonly BodyType LightTruck = new("Light truck", "Light truck", "Lätt lastbil");
    public static readonly BodyType OffRoad = new("Off-road", "Off-road vehicle", "Terrängbil");

    /// <summary>All predefined body types.</summary>
    public static IReadOnlyList<BodyType> All { get; } =
    [
        Sedan, Hatchback, Stationwagon, Suv, Crossover, Coupe, Convertible, Roadster,
        Mpv, Minivan, Van, Pickup, Truck, LightTruck, Bus, Microcar, Limousine, Targa,
        Trailer, Motorhome, Tractor, Tipper, Dumper, OffRoad
    ];

    private BodyType(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    public static bool TryParse(string? input, out BodyType? result)
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

    public static BodyType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid body type.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Kombi</c> (Swedish) or
    /// <c>Station wagon</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Sedan</c>, <c>Stationwagon</c>, <c>SUV</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical English value, e.g. <c>Sedan</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o").Replace("é", "e");
        folded = Regex.Replace(folded, @"[\s\-_/()]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, BodyType> d, BodyType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, BodyType> BuildLookup()
    {
        var d = new Dictionary<string, BodyType>(StringComparer.OrdinalIgnoreCase);

        foreach (var b in All)
        {
            AddKey(d, b, b.Value);
            AddKey(d, b, b.EnglishName);
            AddKey(d, b, b.LocalizedName);
        }

        AddKey(d, Sedan, "Saloon");
        AddKey(d, Hatchback, "Hatch");
        AddKey(d, Hatchback, "Halvkombi");
        AddKey(d, Hatchback, "5-dörrars");
        AddKey(d, Hatchback, "3-dörrars");
        AddKey(d, Stationwagon, "Wagon");
        AddKey(d, Stationwagon, "Estate");
        AddKey(d, Stationwagon, "Kombi");
        AddKey(d, Stationwagon, "Herrgårdsvagn");
        AddKey(d, Suv, "Sport Utility Vehicle");
        AddKey(d, Suv, "Suvkombi");
        AddKey(d, Suv, "Stadsjeep");
        AddKey(d, Crossover, "CUV");
        AddKey(d, Crossover, "Crossover SUV");
        AddKey(d, Coupe, "Coupé");
        AddKey(d, Coupe, "Coupe");
        AddKey(d, Coupe, "2-door coupe");
        AddKey(d, Convertible, "Cabriolet");
        AddKey(d, Convertible, "Cabrio");
        AddKey(d, Convertible, "Drophead");
        AddKey(d, Convertible, "Soft top");
        AddKey(d, Roadster, "Spider");
        AddKey(d, Roadster, "Spyder");
        AddKey(d, Mpv, "Multi-purpose vehicle");
        AddKey(d, Mpv, "People carrier");
        AddKey(d, Minivan, "MPV minivan");
        AddKey(d, Van, "Cargo van");
        AddKey(d, Van, "Skåpbil");
        AddKey(d, Van, "Transportbil");
        AddKey(d, Pickup, "Pick-up");
        AddKey(d, Pickup, "Flatbed pickup");
        AddKey(d, Pickup, "Pickupbil");
        AddKey(d, Truck, "Lorry");
        AddKey(d, Truck, "HGV");
        AddKey(d, Truck, "Heavy truck");
        AddKey(d, Truck, "Tung lastbil");
        AddKey(d, Truck, "Lastbil");
        AddKey(d, Bus, "Coach");
        AddKey(d, Bus, "Buss");
        AddKey(d, Bus, "Minibus");
        AddKey(d, Bus, "Minibuss");
        AddKey(d, Microcar, "Quadricycle");
        AddKey(d, Microcar, "Mopedbil");
        AddKey(d, Limousine, "Limo");
        AddKey(d, Limousine, "Stretch");
        AddKey(d, Targa, "Targa top");

        AddKey(d, Trailer, "Släp");
        AddKey(d, Trailer, "Släpvagn");
        AddKey(d, Trailer, "Släpkärra");
        AddKey(d, Trailer, "Påhängsvagn");
        AddKey(d, Trailer, "Semi-trailer");
        AddKey(d, Trailer, "Semitrailer");

        AddKey(d, Motorhome, "Husbil");
        AddKey(d, Motorhome, "RV");
        AddKey(d, Motorhome, "Recreational vehicle");
        AddKey(d, Motorhome, "Camper");
        AddKey(d, Motorhome, "Camper van");

        AddKey(d, Tractor, "Traktor");
        AddKey(d, Tractor, "Farm tractor");
        AddKey(d, Tractor, "Lantbrukstraktor");
        AddKey(d, Tractor, "Jordbrukstraktor");

        AddKey(d, Tipper, "Tippbil");
        AddKey(d, Tipper, "Tipper truck");
        AddKey(d, Tipper, "Dump truck");
        AddKey(d, Tipper, "Tipplastbil");

        AddKey(d, Dumper, "Bergsdumper");
        AddKey(d, Dumper, "Articulated dumper");
        AddKey(d, Dumper, "Ramstyrd dumper");
        AddKey(d, Dumper, "Articulated dump truck");
        AddKey(d, Dumper, "ADT");

        AddKey(d, LightTruck, "Lätt lastbil");
        AddKey(d, LightTruck, "Light commercial vehicle");
        AddKey(d, LightTruck, "LCV");
        AddKey(d, LightTruck, "N1");

        AddKey(d, OffRoad, "Terrängbil");
        AddKey(d, OffRoad, "Terrängfordon");
        AddKey(d, OffRoad, "Off-road vehicle");
        AddKey(d, OffRoad, "Off-roader");
        AddKey(d, OffRoad, "ORV");

        return d;
    }

    public static bool operator ==(BodyType? a, BodyType? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(BodyType? a, BodyType? b) => !(a == b);

    public bool Equals(BodyType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is BodyType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(BodyType? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(BodyType a, BodyType b) => a.CompareTo(b) < 0;
    public static bool operator >(BodyType a, BodyType b) => a.CompareTo(b) > 0;
    public static bool operator <=(BodyType a, BodyType b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BodyType a, BodyType b) => a.CompareTo(b) >= 0;
}
