using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of mass identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (kilogram).
/// </summary>
public sealed class WeightUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Weight Unit", "Viktenhet", "⚖️", []);

    private static readonly Lazy<Dictionary<string, WeightUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in kilograms.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private WeightUnit(string symbol, string englishName, string localizedName,
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

    public static WeightUnit Microgram { get; } = new("µg", "microgram", "mikrogram", "micrograms", "mikrogram", 0.000000001m,
        "ug");
    public static WeightUnit Milligram { get; } = new("mg", "milligram", "milligram", "milligrams", "milligram", 0.000001m);
    public static WeightUnit Gram { get; } = new("g", "gram", "gram", "grams", "gram", 0.001m);
    public static WeightUnit Hectogram { get; } = new("hg", "hectogram", "hektogram", "hectograms", "hektogram", 0.1m,
        "hekto");
    public static WeightUnit Kilogram { get; } = new("kg", "kilogram", "kilogram", "kilograms", "kilogram", 1m,
        "kilo", "kilos");
    public static WeightUnit MetricTon { get; } = new("t", "metric ton", "ton", "metric tons", "ton", 1000m,
        "ton", "tonne", "tonnes", "metric ton", "metric tons");
    public static WeightUnit Pound { get; } = new("lb", "pound", "pund", "pounds", "pund", 0.45359237m, "pound", "pounds", "lbs");
    public static WeightUnit Ounce { get; } = new("oz", "ounce", "uns", "ounces", "uns", 0.028349523125m, "ounce", "ounces");
    public static WeightUnit Stone { get; } = new("st", "stone", "stone", "stones", "stone", 6.35029318m, "stone", "stones");

    public static IReadOnlyList<WeightUnit> All { get; } =
        [Microgram, Milligram, Gram, Hectogram, Kilogram, MetricTon, Pound, Ounce, Stone];

    /// <summary>The base SI unit: kilogram.</summary>
    public static WeightUnit BaseUnit => Kilogram;

    /// <summary>
    /// Metric SI units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes imperial and niche units.
    /// </summary>
    public static IReadOnlyList<WeightUnit> NaturalScale { get; } =
        [Milligram, Gram, Kilogram, MetricTon];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in kilograms).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static WeightUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out WeightUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static WeightUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown weight unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, WeightUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, WeightUnit>(StringComparer.OrdinalIgnoreCase);
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
