using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of luminance identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (candela per square metre, cd/m²).
/// </summary>
public sealed class LuminanceUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Luminance Unit", "Luminansenhet", "🔆", []);

    private static readonly Lazy<Dictionary<string, LuminanceUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in candela per square metre.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private LuminanceUnit(string symbol, string englishName, string localizedName,
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

    public static LuminanceUnit CandelaPerSquareMetre { get; } = new(
        "cd/m²", "candela per square metre", "candela per kvadratmeter",
        "candela per square metre", "candela per kvadratmeter", 1m,
        "cd/m2", "cd m-2", "cd*m-2", "cd/sqm", "cdm2", "cd/m²", "Cd/m²", "Cd/m2");

    public static LuminanceUnit Nit { get; } = new(
        "nit", "nit", "nit",
        "nits", "nit", 1m,
        "Nit", "NITS", "nits");

    public static LuminanceUnit KilocandelaPerSquareMetre { get; } = new(
        "kcd/m²", "kilocandela per square metre", "kilocandela per kvadratmeter",
        "kilocandela per square metre", "kilocandela per kvadratmeter", 1000m,
        "kcd/m2", "kcdm2", "Kcd/m²", "Kcd/m2");

    public static LuminanceUnit Kilonit { get; } = new(
        "knit", "kilonit", "kilonit",
        "kilonits", "kilonit", 1000m,
        "Knit", "KNIT", "kilonits");

    public static IReadOnlyList<LuminanceUnit> All { get; } =
        [CandelaPerSquareMetre, Nit, KilocandelaPerSquareMetre, Kilonit];

    /// <summary>The base SI unit: candela per square metre.</summary>
    public static LuminanceUnit BaseUnit => CandelaPerSquareMetre;

    /// <summary>
    /// Units ordered small-to-large, suitable for auto-scaling display.
    /// Picks the cd/m² family (not the nit alias) when scaling.
    /// </summary>
    public static IReadOnlyList<LuminanceUnit> NaturalScale { get; } =
        [CandelaPerSquareMetre, KilocandelaPerSquareMetre];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in cd/m²).
    /// </summary>
    public static LuminanceUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out LuminanceUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static LuminanceUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown luminance unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, LuminanceUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, LuminanceUnit>(StringComparer.OrdinalIgnoreCase);
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
