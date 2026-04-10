using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of volumetric flow rate identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base unit (liters per minute).
/// </summary>
public sealed class FlowRateUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Flow Rate Unit", "Flödesenhet", "🌊", []);

    private static readonly Lazy<Dictionary<string, FlowRateUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in liters per minute.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private FlowRateUnit(string symbol, string englishName, string localizedName,
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

    public static FlowRateUnit LitersPerSecond { get; } = new("L/s", "liter per second", "liter per sekund",
        "liters per second", "liter per sekund", 60m,
        "l/s");
    public static FlowRateUnit LitersPerMinute { get; } = new("L/min", "liter per minute", "liter per minut",
        "liters per minute", "liter per minut", 1m,
        "l/min");
    public static FlowRateUnit LitersPerHour { get; } = new("L/h", "liter per hour", "liter per timme",
        "liters per hour", "liter per timme", 1m / 60m,
        "l/h", "l/timme", "liter/timme", "liter/h");
    public static FlowRateUnit CubicMetersPerHour { get; } = new("m³/h", "cubic meter per hour", "kubikmeter per timme",
        "cubic meters per hour", "kubikmeter per timme", 1000m / 60m,
        "m3/h", "m³/tim", "m3/tim", "cbm/h");
    public static FlowRateUnit CubicMetersPerMinute { get; } = new("m³/min", "cubic meter per minute", "kubikmeter per minut",
        "cubic meters per minute", "kubikmeter per minut", 1000m,
        "m3/min");
    public static FlowRateUnit GallonsPerMinute { get; } = new("gal/min", "gallon per minute", "gallon per minut",
        "gallons per minute", "gallon per minut", 3.785411784m,
        "gpm", "GPM");

    public static IReadOnlyList<FlowRateUnit> All { get; } =
        [LitersPerSecond, LitersPerMinute, LitersPerHour, CubicMetersPerHour, CubicMetersPerMinute, GallonsPerMinute];

    /// <summary>The base unit: liters per minute.</summary>
    public static FlowRateUnit BaseUnit => LitersPerMinute;

    /// <summary>
    /// Practical units ordered small-to-large, suitable for auto-scaling display.
    /// </summary>
    public static IReadOnlyList<FlowRateUnit> NaturalScale { get; } =
        [LitersPerHour, LitersPerMinute, CubicMetersPerHour];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in liters per minute).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static FlowRateUnit GetNatural(decimal baseValue)
    {
        var abs = Math.Abs(baseValue);
        var result = NaturalScale[0];
        for (var i = 1; i < NaturalScale.Count; i++)
        {
            if (abs / NaturalScale[i].ToBaseUnitFactor >= 1m)
                result = NaturalScale[i];
            else
                break;
        }
        return result;
    }

    /// <summary>Converts a value in this unit to liters per minute (exact for units involving ÷60).</summary>
    internal decimal ToLitersPerMinute(decimal valueInThisUnit)
    {
        if (ReferenceEquals(this, LitersPerHour)) return valueInThisUnit / 60m;
        if (ReferenceEquals(this, CubicMetersPerHour)) return valueInThisUnit * 1000m / 60m;
        return valueInThisUnit * ToBaseUnitFactor;
    }

    /// <summary>Converts liters per minute to a value in this unit (exact for units involving ÷60).</summary>
    internal decimal FromLitersPerMinute(decimal litersPerMinute)
    {
        if (ReferenceEquals(this, LitersPerHour)) return litersPerMinute * 60m;
        if (ReferenceEquals(this, CubicMetersPerHour)) return litersPerMinute * 60m / 1000m;
        return litersPerMinute / ToBaseUnitFactor;
    }

    public static bool TryParse(string? input, out FlowRateUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static FlowRateUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown flow rate unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, FlowRateUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, FlowRateUnit>(StringComparer.OrdinalIgnoreCase);
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
