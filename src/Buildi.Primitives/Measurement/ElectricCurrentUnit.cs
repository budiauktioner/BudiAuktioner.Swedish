using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of electric current identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (ampere).
/// </summary>
public sealed class ElectricCurrentUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Electric Current Unit", "Strömenhet", "⚡", []);

    private static readonly Lazy<Dictionary<string, ElectricCurrentUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in amperes.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private ElectricCurrentUnit(string symbol, string englishName, string localizedName,
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

    public static ElectricCurrentUnit Microampere { get; } = new("µA", "microampere", "mikroampere", "microamperes", "mikroampere", 0.000001m,
        "uA");
    public static ElectricCurrentUnit Milliampere { get; } = new("mA", "milliampere", "milliampere", "milliamperes", "milliampere", 0.001m);
    public static ElectricCurrentUnit Ampere { get; } = new("A", "ampere", "ampere", "amperes", "ampere", 1m,
        "amp", "amps");
    public static ElectricCurrentUnit Kiloampere { get; } = new("kA", "kiloampere", "kiloampere", "kiloamperes", "kiloampere", 1000m);

    public static IReadOnlyList<ElectricCurrentUnit> All { get; } =
        [Microampere, Milliampere, Ampere, Kiloampere];

    /// <summary>The base SI unit: ampere.</summary>
    public static ElectricCurrentUnit BaseUnit => Ampere;

    /// <summary>
    /// SI units ordered small-to-large, suitable for auto-scaling display.
    /// </summary>
    public static IReadOnlyList<ElectricCurrentUnit> NaturalScale { get; } =
        [Milliampere, Ampere, Kiloampere];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in amperes).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static ElectricCurrentUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out ElectricCurrentUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static ElectricCurrentUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown electric current unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, ElectricCurrentUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, ElectricCurrentUnit>(StringComparer.OrdinalIgnoreCase);
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
