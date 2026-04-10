using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of torque identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base SI unit (newton-meter).
/// </summary>
public sealed class TorqueUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Torque Unit", "Vridmomentsenhet", "🔧", []);

    private static readonly Lazy<Dictionary<string, TorqueUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in newton-meters.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private TorqueUnit(string symbol, string englishName, string localizedName,
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

    public static TorqueUnit NewtonMeter { get; } = new(
        "Nm", "newton meter", "newtonmeter", "newton meters", "newtonmeter", 1m,
        "N·m", "newton-meter", "newton-meters",
        "newton metre", "newton metres", "newton-metre", "newton-metres");

    public static TorqueUnit MillinewtonMeter { get; } = new(
        "mNm", "millinewton meter", "millinewtonmeter", "millinewton meters", "millinewtonmeter", 0.001m,
        "mN·m", "millinewton-meter", "millinewton-meters");

    public static TorqueUnit KilonewtonMeter { get; } = new(
        "kNm", "kilonewton meter", "kilonewtonmeter", "kilonewton meters", "kilonewtonmeter", 1000m,
        "kN·m", "kilonewton-meter", "kilonewton-meters");

    public static TorqueUnit FootPound { get; } = new(
        "ft-lb", "foot-pound", "fotpund", "foot-pounds", "fotpund", 1.3558179483314004m,
        "ft·lb", "foot-pounds", "ft lb", "ft-lbs", "ft lbs", "ftlb", "ftlbs");

    public static TorqueUnit KilogramForceMeter { get; } = new(
        "kgf-m", "kilogram-force meter", "kilogramkraftmeter", "kilogram-force meters", "kilogramkraftmeter",
        9.80665m,
        "kgf·m", "kgfm");

    public static TorqueUnit InchPound { get; } = new(
        "in-lb", "inch-pound", "tumpund", "inch-pounds", "tumpund", 0.1129848290276167m,
        "in·lb", "in lb", "in-lbs", "in lbs");

    public static IReadOnlyList<TorqueUnit> All { get; } =
        [NewtonMeter, MillinewtonMeter, KilonewtonMeter, FootPound, KilogramForceMeter, InchPound];

    /// <summary>The base SI unit: newton-meter.</summary>
    public static TorqueUnit BaseUnit => NewtonMeter;

    public static bool TryParse(string? input, out TorqueUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static TorqueUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown torque unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, TorqueUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, TorqueUnit>(StringComparer.OrdinalIgnoreCase);
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
