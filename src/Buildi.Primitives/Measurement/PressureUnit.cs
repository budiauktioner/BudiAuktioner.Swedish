using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of pressure identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (pascal).
/// </summary>
public sealed class PressureUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Pressure Unit", "Tryckenhet", "🎯", []);

    private static readonly Lazy<Dictionary<string, PressureUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in pascals.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private PressureUnit(string symbol, string englishName, string localizedName,
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

    public static PressureUnit Pascal { get; } = new("Pa", "pascal", "pascal", "pascals", "pascal", 1m);
    public static PressureUnit Hectopascal { get; } = new("hPa", "hectopascal", "hektopascal", "hectopascals", "hektopascal", 100m);
    public static PressureUnit Kilopascal { get; } = new("kPa", "kilopascal", "kilopascal", "kilopascals", "kilopascal", 1000m);
    public static PressureUnit Megapascal { get; } = new("MPa", "megapascal", "megapascal", "megapascals", "megapascal", 1_000_000m);
    public static PressureUnit Gigapascal { get; } = new("GPa", "gigapascal", "gigapascal", "gigapascals", "gigapascal", 1_000_000_000m);
    public static PressureUnit Bar { get; } = new("bar", "bar", "bar", "bars", "bar", 100_000m);
    public static PressureUnit Millibar { get; } = new("mbar", "millibar", "millibar", "millibars", "millibar", 100m);
    public static PressureUnit Psi { get; } = new("PSI", "psi", "psi", "psi", "psi", 6894.757293168m,
        "psi", "lb/in²", "pounds per square inch");
    public static PressureUnit Atmosphere { get; } = new("atm", "atmosphere", "atmosfär", "atmospheres", "atmosfärer", 101_325m);
    public static PressureUnit MillimeterOfMercury { get; } = new("mmHg", "millimeter of mercury", "millimeter kvicksilver", "millimeters of mercury", "millimeter kvicksilver", 133.322387415m,
        "mm Hg", "torr", "millimetre of mercury", "millimetres of mercury");

    public static IReadOnlyList<PressureUnit> All { get; } =
        [Pascal, Hectopascal, Kilopascal, Megapascal, Gigapascal, Bar, Millibar, Psi, Atmosphere, MillimeterOfMercury];

    /// <summary>The base SI unit: pascal.</summary>
    public static PressureUnit BaseUnit => Pascal;

    /// <summary>
    /// Common metric units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes imperial and niche units (PSI, atm, mmHg).
    /// </summary>
    public static IReadOnlyList<PressureUnit> NaturalScale { get; } =
        [Pascal, Hectopascal, Kilopascal, Bar, Megapascal];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in pascals).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static PressureUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out PressureUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static PressureUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown pressure unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, PressureUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, PressureUnit>(StringComparer.OrdinalIgnoreCase);
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
