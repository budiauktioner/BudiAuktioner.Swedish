using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of power identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (watt).
/// </summary>
public sealed class PowerUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Power Unit", "Effektenhet", "💡", []);

    private static readonly Lazy<Dictionary<string, PowerUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in watts.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private PowerUnit(string symbol, string englishName, string localizedName,
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

    public static PowerUnit Microwatt { get; } = new("µW", "microwatt", "mikrowatt", "microwatts", "mikrowatt", 0.000001m,
        "uW");
    public static PowerUnit Milliwatt { get; } = new("mW", "milliwatt", "milliwatt", "milliwatts", "milliwatt", 0.001m);
    public static PowerUnit Watt { get; } = new("W", "watt", "watt", "watts", "watt", 1m);
    public static PowerUnit Kilowatt { get; } = new("kW", "kilowatt", "kilowatt", "kilowatts", "kilowatt", 1000m);
    public static PowerUnit Megawatt { get; } = new("MW", "megawatt", "megawatt", "megawatts", "megawatt", 1_000_000m);
    public static PowerUnit Gigawatt { get; } = new("GW", "gigawatt", "gigawatt", "gigawatts", "gigawatt", 1_000_000_000m);
    public static PowerUnit Terawatt { get; } = new("TW", "terawatt", "terawatt", "terawatts", "terawatt", 1_000_000_000_000m);
    public static PowerUnit Horsepower { get; } = new("HP", "horsepower", "hästkraft", "horsepower", "hästkrafter", 745.69987158227022m,
        "hp", "horsepower", "hk", "hästkraft", "hästkrafter");

    public static IReadOnlyList<PowerUnit> All { get; } =
        [Microwatt, Milliwatt, Watt, Kilowatt, Megawatt, Gigawatt, Terawatt, Horsepower];

    /// <summary>The base SI unit: watt.</summary>
    public static PowerUnit BaseUnit => Watt;

    /// <summary>
    /// SI units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes non-SI units (horsepower).
    /// </summary>
    public static IReadOnlyList<PowerUnit> NaturalScale { get; } =
        [Watt, Kilowatt, Megawatt, Gigawatt];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in watts).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static PowerUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out PowerUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var t = input.Trim();
        if (t.Length == 2 && (t[1] == 'W' || t[1] == 'w'))
        {
            if (t[0] == 'm') { result = Milliwatt; return true; }
            if (t[0] == 'M') { result = Megawatt; return true; }
        }

        return BySymbol.Value.TryGetValue(t, out result);
    }

    public static PowerUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown power unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, PowerUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, PowerUnit>(StringComparer.OrdinalIgnoreCase);
        foreach (var u in All)
        {
            if (u == Milliwatt || u == Megawatt)
            {
                dict.TryAdd(u.EnglishName, u);
                dict.TryAdd(u.LocalizedName, u);
                dict.TryAdd(u.PluralEnglish, u);
                dict.TryAdd(u.PluralSwedish, u);
                foreach (var alias in u._aliases)
                    dict.TryAdd(alias, u);
                continue;
            }

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
