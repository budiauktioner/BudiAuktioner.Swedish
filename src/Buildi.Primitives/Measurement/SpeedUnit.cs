using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of speed identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (meter per second).
/// </summary>
public sealed class SpeedUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Speed Unit", "Hastighetsenhet", "🏎️", []);

    private static readonly Lazy<Dictionary<string, SpeedUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in meters per second.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private SpeedUnit(string symbol, string englishName, string localizedName,
        string pluralEnglish, string pluralSwedish, decimal toBaseUnitFactor, params string[] aliases)
    {
        Symbol = symbol;
        EnglishName = englishName;
        LocalizedName = localizedName;
        PluralEnglish = pluralEnglish;
        PluralSwedish = pluralSwedish;
        ToBaseUnitFactor = toBaseUnitFactor;
        _aliases = aliases;
    }

    public static SpeedUnit MetersPerSecond { get; } = new("m/s", "meter per second", "meter per sekund",
        "meters per second", "meter per sekund", 1m);

    public static SpeedUnit KilometersPerHour { get; } = new("km/h", "kilometer per hour", "kilometer per timme",
        "kilometers per hour", "kilometer per timme", 5m / 18m,
        "kmh", "km/t", "kph", "kilometre per hour", "kilometres per hour");

    public static SpeedUnit MilesPerHour { get; } = new("mph", "mile per hour", "miles per timme",
        "miles per hour", "miles per timme", 0.44704m, "miles per hour");

    public static SpeedUnit FeetPerSecond { get; } = new("ft/s", "foot per second", "fot per sekund",
        "feet per second", "fot per sekund", 0.3048m);

    public static SpeedUnit Knot { get; } = new("kn", "knot", "knop", "knots", "knop", 1852m / 3600m,
        "knot", "knots", "knop", "kt");

    public static IReadOnlyList<SpeedUnit> All { get; } =
        [MetersPerSecond, KilometersPerHour, MilesPerHour, FeetPerSecond, Knot];

    /// <summary>The base SI unit: meter per second.</summary>
    public static SpeedUnit BaseUnit => MetersPerSecond;

    public static bool TryParse(string? input, out SpeedUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static SpeedUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown speed unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, SpeedUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, SpeedUnit>(StringComparer.OrdinalIgnoreCase);
        foreach (var u in All)
        {
            dict.TryAdd(u.Symbol, u);
            dict.TryAdd(u.EnglishName, u);
            dict.TryAdd(u.LocalizedName, u);
            dict.TryAdd(u.PluralEnglish, u);
            dict.TryAdd(u.PluralSwedish, u);
            foreach (var alias in u._aliases)
                dict.TryAdd(alias, u);
        }
        return dict;
    }
}
