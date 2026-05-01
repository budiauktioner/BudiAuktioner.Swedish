using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of length identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (meter).
/// </summary>
public sealed class LengthUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Length Unit", "Längdenhet", "📏", []);

    private static readonly Lazy<Dictionary<string, LengthUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in meters.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private LengthUnit(string symbol, string englishName, string localizedName,
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

    public static LengthUnit Nanometer { get; } = new("nm", "nanometer", "nanometer", "nanometers", "nanometer", 0.000000001m,
        "nanometre", "nanometres");
    public static LengthUnit Micrometer { get; } = new("µm", "micrometer", "mikrometer", "micrometers", "mikrometer", 0.000001m,
        "micrometre", "micrometres", "um");
    public static LengthUnit Millimeter { get; } = new("mm", "millimeter", "millimeter", "millimeters", "millimeter", 0.001m,
        "millimetre", "millimetres");
    public static LengthUnit Centimeter { get; } = new("cm", "centimeter", "centimeter", "centimeters", "centimeter", 0.01m,
        "centimetre", "centimetres");
    public static LengthUnit Decimeter { get; } = new("dm", "decimeter", "decimeter", "decimeters", "decimeter", 0.1m,
        "decimetre", "decimetres");
    public static LengthUnit Meter { get; } = new("m", "meter", "meter", "meters", "meter", 1m,
        "metre", "metres");
    public static LengthUnit Kilometer { get; } = new("km", "kilometer", "kilometer", "kilometers", "kilometer", 1000m,
        "kilometre", "kilometres");
    public static LengthUnit Inch { get; } = new("in", "inch", "tum", "inches", "tum", 0.0254m, "inch", "inches", "\"", "tum");
    public static LengthUnit Foot { get; } = new("ft", "foot", "fot", "feet", "fot", 0.3048m, "foot", "feet", "fot");
    public static LengthUnit Yard { get; } = new("yd", "yard", "yard", "yards", "yard", 0.9144m, "yard", "yards");
    public static LengthUnit Mile { get; } = new("mi", "mile", "engelsk mil", "miles", "engelska mil", 1609.344m, "mile", "miles");
    public static LengthUnit NauticalMile { get; } = new("nmi", "nautical mile", "nautisk mil", "nautical miles", "nautiska mil", 1852m, "nautical mile", "nautical miles", "sjömil", "NM");
    public static LengthUnit SwedishMile { get; } = new("mil", "Swedish mile", "mil", "Swedish miles", "mil", 10000m);

    public static IReadOnlyList<LengthUnit> All { get; } =
        [Nanometer, Micrometer, Millimeter, Centimeter, Decimeter, Meter, Kilometer, Inch, Foot, Yard, Mile, NauticalMile, SwedishMile];

    /// <summary>The base SI unit: meter.</summary>
    public static LengthUnit BaseUnit => Meter;

    /// <summary>
    /// Metric SI units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes imperial and niche units.
    /// </summary>
    public static IReadOnlyList<LengthUnit> NaturalScale { get; } =
        [Millimeter, Centimeter, Meter, Kilometer];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in meters).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static LengthUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out LengthUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var trimmed = input.Trim();
        if (trimmed == "NM")
        {
            result = NauticalMile;
            return true;
        }
        return BySymbol.Value.TryGetValue(trimmed, out result);
    }

    public static LengthUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown length unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, LengthUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, LengthUnit>(StringComparer.OrdinalIgnoreCase);
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
