using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of frequency identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (hertz).
/// </summary>
public sealed class FrequencyUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Frequency Unit", "Frekvensenhet", "📡", []);

    private static readonly Lazy<Dictionary<string, FrequencyUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in hertz.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private FrequencyUnit(string symbol, string englishName, string localizedName,
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

    public static FrequencyUnit Hertz { get; } = new("Hz", "hertz", "hertz", "hertz", "hertz", 1m);
    public static FrequencyUnit Kilohertz { get; } = new("kHz", "kilohertz", "kilohertz", "kilohertz", "kilohertz", 1000m);
    public static FrequencyUnit Megahertz { get; } = new("MHz", "megahertz", "megahertz", "megahertz", "megahertz", 1_000_000m);
    public static FrequencyUnit Gigahertz { get; } = new("GHz", "gigahertz", "gigahertz", "gigahertz", "gigahertz", 1_000_000_000m);
    public static FrequencyUnit Terahertz { get; } = new("THz", "terahertz", "terahertz", "terahertz", "terahertz", 1_000_000_000_000m);
    public static FrequencyUnit RevolutionsPerMinute { get; } = new(
        "rpm",
        "revolution per minute",
        "varv per minut",
        "revolutions per minute",
        "varv per minut",
        1m / 60m,
        "RPM",
        "rev/min",
        "varv/min",
        "r/min");

    public static IReadOnlyList<FrequencyUnit> All { get; } =
        [Hertz, Kilohertz, Megahertz, Gigahertz, Terahertz, RevolutionsPerMinute];

    /// <summary>The base SI unit: hertz.</summary>
    public static FrequencyUnit BaseUnit => Hertz;

    /// <summary>
    /// SI units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes non-SI units (rpm).
    /// </summary>
    public static IReadOnlyList<FrequencyUnit> NaturalScale { get; } =
        [Hertz, Kilohertz, Megahertz, Gigahertz, Terahertz];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in hertz).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static FrequencyUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out FrequencyUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static FrequencyUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown frequency unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, FrequencyUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, FrequencyUnit>(StringComparer.OrdinalIgnoreCase);
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
