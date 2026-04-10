using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of area identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (square meter).
/// </summary>
public sealed class AreaUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Area Unit", "Ytenhet", "📐", []);

    private static readonly Lazy<Dictionary<string, AreaUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in square meters.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private AreaUnit(string symbol, string englishName, string localizedName,
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

    public static AreaUnit SquareMillimeter { get; } = new("mm²", "square millimeter", "kvadratmillimeter", "square millimeters", "kvadratmillimeter", 0.000001m,
        "mm2", "sq mm", "square millimetre", "square millimetres");

    public static AreaUnit SquareCentimeter { get; } = new("cm²", "square centimeter", "kvadratcentimeter", "square centimeters", "kvadratcentimeter", 0.0001m,
        "cm2", "sq cm", "square centimetre", "square centimetres");

    public static AreaUnit SquareMeter { get; } = new("m²", "square meter", "kvadratmeter", "square meters", "kvadratmeter", 1m,
        "m2", "sq m", "sqm", "square metre", "square metres");

    public static AreaUnit SquareKilometer { get; } = new("km²", "square kilometer", "kvadratkilometer", "square kilometers", "kvadratkilometer", 1000000m,
        "km2", "sq km", "square kilometre", "square kilometres");

    public static AreaUnit Hectare { get; } = new("ha", "hectare", "hektar", "hectares", "hektar", 10000m);

    public static AreaUnit Acre { get; } = new("acre", "acre", "acre", "acres", "acre", 4046.8564224m);

    public static AreaUnit SquareFoot { get; } = new("sq ft", "square foot", "kvadratfot", "square feet", "kvadratfot", 0.09290304m,
        "sqft", "square feet", "square foot");

    public static AreaUnit SquareInch { get; } = new("sq in", "square inch", "kvadrattum", "square inches", "kvadrattum", 0.00064516m,
        "sqin", "square inches", "square inch");

    public static AreaUnit SquareYard { get; } = new("sq yd", "square yard", "kvadratyard", "square yards", "kvadratyard", 0.83612736m,
        "sqyd", "square yards", "square yard");

    public static IReadOnlyList<AreaUnit> All { get; } =
    [
        SquareMillimeter, SquareCentimeter, SquareMeter, SquareKilometer,
        Hectare, Acre, SquareFoot, SquareInch, SquareYard
    ];

    /// <summary>The base SI unit: square meter.</summary>
    public static AreaUnit BaseUnit => SquareMeter;

    /// <summary>
    /// Metric units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes imperial units.
    /// </summary>
    public static IReadOnlyList<AreaUnit> NaturalScale { get; } =
        [SquareCentimeter, SquareMeter, Hectare, SquareKilometer];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in square meters).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static AreaUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out AreaUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static AreaUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown area unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, AreaUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, AreaUnit>(StringComparer.OrdinalIgnoreCase);
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
