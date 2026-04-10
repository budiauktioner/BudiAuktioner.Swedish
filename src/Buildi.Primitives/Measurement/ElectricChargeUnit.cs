using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of electric charge identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base unit (ampere-hour).
/// </summary>
public sealed class ElectricChargeUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Electric Charge Unit", "Laddningsenhet", "🔋", []);

    private static readonly Lazy<Dictionary<string, ElectricChargeUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in ampere-hours.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private ElectricChargeUnit(string symbol, string englishName, string localizedName,
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

    public static ElectricChargeUnit MilliampereHour { get; } = new("mAh", "milliampere-hour", "milliamperetimme",
        "milliampere-hours", "milliamperetimmar", 0.001m,
        "milliampere hour", "milliampere hours", "milliamperehour", "milliamperehours");

    public static ElectricChargeUnit AmpereHour { get; } = new("Ah", "ampere-hour", "amperetimme",
        "ampere-hours", "amperetimmar", 1m,
        "ampere hour", "ampere hours", "amperehour", "amperehours",
        "amp-hour", "amp-hours", "amp hour", "amp hours");

    /// <summary>1 C = 1/3600 Ah (1 Ah = 3600 C). Use <see cref="ToAmpereHours"/> for exact conversion.</summary>
    public static ElectricChargeUnit Coulomb { get; } = new("C", "coulomb", "coulomb", "coulombs", "coulomb", 1m / 3600m);

    public static IReadOnlyList<ElectricChargeUnit> All { get; } =
        [MilliampereHour, AmpereHour, Coulomb];

    /// <summary>The base unit: ampere-hour.</summary>
    public static ElectricChargeUnit BaseUnit => AmpereHour;

    /// <summary>
    /// Practical units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes SI coulombs.
    /// </summary>
    public static IReadOnlyList<ElectricChargeUnit> NaturalScale { get; } =
        [MilliampereHour, AmpereHour];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in ampere-hours).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static ElectricChargeUnit GetNatural(decimal baseValue)
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

    /// <summary>Converts a value in this unit to ampere-hours (exact for coulombs).</summary>
    internal decimal ToAmpereHours(decimal valueInThisUnit) =>
        ReferenceEquals(this, Coulomb) ? valueInThisUnit / 3600m : valueInThisUnit * ToBaseUnitFactor;

    /// <summary>Converts ampere-hours to a value in this unit (exact for coulombs).</summary>
    internal decimal FromAmpereHours(decimal ampereHours) =>
        ReferenceEquals(this, Coulomb) ? ampereHours * 3600m : ampereHours / ToBaseUnitFactor;

    public static bool TryParse(string? input, out ElectricChargeUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static ElectricChargeUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown electric charge unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, ElectricChargeUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, ElectricChargeUnit>(StringComparer.OrdinalIgnoreCase);
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
