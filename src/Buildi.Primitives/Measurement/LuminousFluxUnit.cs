using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of luminous flux identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (lumen).
/// </summary>
public sealed class LuminousFluxUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Luminous Flux Unit", "Ljusflödesenhet", "💡", []);

    private static readonly Lazy<Dictionary<string, LuminousFluxUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in lumens.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private LuminousFluxUnit(string symbol, string englishName, string localizedName,
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

    public static LuminousFluxUnit Lumen { get; } = new("lm", "lumen", "lumen", "lumens", "lumen", 1m);
    public static LuminousFluxUnit Kilolumen { get; } = new("klm", "kilolumen", "kilolumen", "kilolumens", "kilolumen", 1000m,
        "Klm", "kLm");

    public static IReadOnlyList<LuminousFluxUnit> All { get; } =
        [Lumen, Kilolumen];

    /// <summary>The base SI unit: lumen.</summary>
    public static LuminousFluxUnit BaseUnit => Lumen;

    /// <summary>
    /// SI units ordered small-to-large, suitable for auto-scaling display.
    /// </summary>
    public static IReadOnlyList<LuminousFluxUnit> NaturalScale { get; } =
        [Lumen, Kilolumen];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in lumens).
    /// </summary>
    public static LuminousFluxUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out LuminousFluxUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static LuminousFluxUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown luminous flux unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, LuminousFluxUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, LuminousFluxUnit>(StringComparer.OrdinalIgnoreCase);
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
