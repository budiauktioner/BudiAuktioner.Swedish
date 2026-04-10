using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of energy identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (joule).
/// </summary>
public sealed class EnergyUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Energy Unit", "Energienhet", "⚡", []);

    private static readonly Lazy<Dictionary<string, EnergyUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in joules.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private EnergyUnit(string symbol, string englishName, string localizedName,
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

    public static EnergyUnit Joule { get; } = new("J", "joule", "joule", "joules", "joule", 1m);
    public static EnergyUnit Kilojoule { get; } = new("kJ", "kilojoule", "kilojoule", "kilojoules", "kilojoule", 1000m);
    public static EnergyUnit Megajoule { get; } = new("MJ", "megajoule", "megajoule", "megajoules", "megajoule", 1_000_000m);
    public static EnergyUnit Gigajoule { get; } = new("GJ", "gigajoule", "gigajoule", "gigajoules", "gigajoule", 1_000_000_000m);
    public static EnergyUnit Terajoule { get; } = new("TJ", "terajoule", "terajoule", "terajoules", "terajoule", 1_000_000_000_000m);
    public static EnergyUnit WattHour { get; } = new("Wh", "watt-hour", "wattimme", "watt-hours", "wattimmar", 3600m,
        "watthour", "watthours", "watt hour", "watt hours");
    public static EnergyUnit KilowattHour { get; } = new("kWh", "kilowatt-hour", "kilowattimme", "kilowatt-hours", "kilowattimmar", 3_600_000m,
        "kilowatthour", "kilowatthours", "kilowatt hour", "kilowatt hours");
    public static EnergyUnit MegawattHour { get; } = new("MWh", "megawatt-hour", "megawattimme", "megawatt-hours", "megawattimmar", 3_600_000_000m,
        "megawatthour", "megawatthours", "megawatt hour", "megawatt hours");
    public static EnergyUnit GigawattHour { get; } = new("GWh", "gigawatt-hour", "gigawattimme", "gigawatt-hours", "gigawattimmar", 3_600_000_000_000m,
        "gigawatthour", "gigawatthours", "gigawatt hour", "gigawatt hours");
    public static EnergyUnit TerawattHour { get; } = new("TWh", "terawatt-hour", "terawattimme", "terawatt-hours", "terawattimmar", 3_600_000_000_000_000m,
        "terawatthour", "terawatthours", "terawatt hour", "terawatt hours");
    public static EnergyUnit Calorie { get; } = new("cal", "calorie", "kalori", "calories", "kalorier", 4.184m,
        "calorie", "calories");
    public static EnergyUnit Kilocalorie { get; } = new("kcal", "kilocalorie", "kilokalori", "kilocalories", "kilokalorier", 4184m,
        "kilocalorie", "kilocalories");
    public static EnergyUnit Btu { get; } = new("BTU", "British thermal unit", "BTU", "British thermal units", "BTU", 1055.05585262m, "btu", "Btu");

    public static IReadOnlyList<EnergyUnit> All { get; } =
        [Joule, Kilojoule, Megajoule, Gigajoule, Terajoule, WattHour, KilowattHour, MegawattHour, GigawattHour, TerawattHour, Calorie, Kilocalorie, Btu];

    /// <summary>The base SI unit: joule.</summary>
    public static EnergyUnit BaseUnit => Joule;

    /// <summary>
    /// Watt-hour units ordered small-to-large, suitable for auto-scaling display
    /// of electrical energy (the most common everyday energy unit in Sweden).
    /// </summary>
    public static IReadOnlyList<EnergyUnit> NaturalScale { get; } =
        [WattHour, KilowattHour, MegawattHour, GigawattHour, TerawattHour];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in joules).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static EnergyUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out EnergyUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static EnergyUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown energy unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, EnergyUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, EnergyUnit>(StringComparer.OrdinalIgnoreCase);
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
