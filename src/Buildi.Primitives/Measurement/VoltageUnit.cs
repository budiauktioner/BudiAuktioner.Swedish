using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of electric potential identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (volt).
/// </summary>
public sealed class VoltageUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Voltage Unit", "Spänningsenhet", "🔌", []);

    private static readonly Lazy<Dictionary<string, VoltageUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in volts.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private VoltageUnit(string symbol, string englishName, string localizedName,
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

    public static VoltageUnit Microvolt { get; } = new("µV", "microvolt", "mikrovolt", "microvolts", "mikrovolt", 0.000001m,
        "uV");
    public static VoltageUnit Millivolt { get; } = new("mV", "millivolt", "millivolt", "millivolts", "millivolt", 0.001m);
    public static VoltageUnit Volt { get; } = new("V", "volt", "volt", "volts", "volt", 1m);
    public static VoltageUnit Kilovolt { get; } = new("kV", "kilovolt", "kilovolt", "kilovolts", "kilovolt", 1000m);
    public static VoltageUnit Megavolt { get; } = new("MV", "megavolt", "megavolt", "megavolts", "megavolt", 1_000_000m);

    public static IReadOnlyList<VoltageUnit> All { get; } =
        [Microvolt, Millivolt, Volt, Kilovolt, Megavolt];

    /// <summary>The base SI unit: volt.</summary>
    public static VoltageUnit BaseUnit => Volt;

    /// <summary>
    /// SI units ordered small-to-large, suitable for auto-scaling display.
    /// </summary>
    public static IReadOnlyList<VoltageUnit> NaturalScale { get; } =
        [Millivolt, Volt, Kilovolt];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in volts).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static VoltageUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out VoltageUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var t = input.Trim();
        if (t.Length == 2 && (t[1] == 'V' || t[1] == 'v'))
        {
            if (t[0] == 'm') { result = Millivolt; return true; }
            if (t[0] == 'M') { result = Megavolt; return true; }
        }

        return BySymbol.Value.TryGetValue(t, out result);
    }

    public static VoltageUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown voltage unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, VoltageUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, VoltageUnit>(StringComparer.OrdinalIgnoreCase);
        foreach (var u in All)
        {
            if (u == Millivolt || u == Megavolt)
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
