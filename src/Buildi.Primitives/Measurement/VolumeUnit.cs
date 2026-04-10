using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of volume identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base unit (liter).
/// </summary>
public sealed class VolumeUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Volume Unit", "Volymenhet", "🧪", []);

    private static readonly Lazy<Dictionary<string, VolumeUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in liters.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private VolumeUnit(string symbol, string englishName, string localizedName,
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

    public static VolumeUnit Microliter { get; } = new("µL", "microliter", "mikroliter", "microliters", "mikroliter", 0.000001m,
        "µl", "ul", "microlitre", "microlitres");
    public static VolumeUnit Milliliter { get; } = new("mL", "milliliter", "milliliter", "milliliters", "milliliter", 0.001m,
        "ml", "millilitre", "millilitres");
    public static VolumeUnit Centiliter { get; } = new("cL", "centiliter", "centiliter", "centiliters", "centiliter", 0.01m,
        "cl", "centilitre", "centilitres");
    public static VolumeUnit Deciliter { get; } = new("dL", "deciliter", "deciliter", "deciliters", "deciliter", 0.1m,
        "dl", "decilitre", "decilitres");
    public static VolumeUnit Liter { get; } = new("L", "liter", "liter", "liters", "liter", 1m,
        "litre", "litres");
    public static VolumeUnit Hectoliter { get; } = new("hL", "hectoliter", "hektoliter", "hectoliters", "hektoliter", 100m,
        "hl", "hectolitre", "hectolitres");
    public static VolumeUnit CubicMeter { get; } = new("m³", "cubic meter", "kubikmeter", "cubic meters", "kubikmeter", 1000m,
        "m3", "cbm", "kubikmeter", "cubic metre", "cubic metres");
    public static VolumeUnit Gallon { get; } = new("gal", "gallon", "gallon", "gallons", "gallon", 3.785411784m, "gallon", "gallons");
    public static VolumeUnit Pint { get; } = new("pt", "pint", "pint", "pints", "pint", 0.473176473m);
    public static VolumeUnit FluidOunce { get; } = new("fl oz", "fluid ounce", "vätskeans", "fluid ounces", "vätskeans", 0.0295735296m,
        "fluid ounce", "fluid ounces", "floz");
    public static VolumeUnit Cup { get; } = new("cup", "cup", "kopp", "cups", "koppar", 0.2365882365m);

    public static IReadOnlyList<VolumeUnit> All { get; } =
        [Microliter, Milliliter, Centiliter, Deciliter, Liter, Hectoliter, CubicMeter, Gallon, Pint, FluidOunce, Cup];

    /// <summary>The base unit: liter.</summary>
    public static VolumeUnit BaseUnit => Liter;

    /// <summary>
    /// Metric units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes imperial and niche units.
    /// </summary>
    public static IReadOnlyList<VolumeUnit> NaturalScale { get; } =
        [Milliliter, Deciliter, Liter, CubicMeter];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in liters).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static VolumeUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out VolumeUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static VolumeUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown volume unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, VolumeUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, VolumeUnit>(StringComparer.OrdinalIgnoreCase);
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
