using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of rotational speed identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base unit (revolutions per minute).
/// </summary>
public sealed class RotationalSpeedUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Rotational Speed Unit", "Varvtalsenhet", "🔄", []);

    private static readonly Lazy<Dictionary<string, RotationalSpeedUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in RPM.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private RotationalSpeedUnit(string symbol, string englishName, string localizedName,
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

    /// <summary>Revolutions per minute (varv per minut). Base unit.</summary>
    public static RotationalSpeedUnit Rpm { get; } = new("rpm", "revolution per minute", "varv per minut", "revolutions per minute", "varv per minut", 1m,
        "RPM", "r/min", "rev/min", "varv/min", "v/min", "vpm");

    /// <summary>Revolutions per second (varv per sekund). 1 rps = 60 rpm.</summary>
    public static RotationalSpeedUnit Rps { get; } = new("rps", "revolution per second", "varv per sekund", "revolutions per second", "varv per sekund", 60m,
        "RPS", "r/s", "rev/s", "varv/s");

    /// <summary>Radians per second (radian per sekund). 1 rad/s ≈ 9.5493 rpm.</summary>
    public static RotationalSpeedUnit RadiansPerSecond { get; } = new("rad/s", "radian per second", "radian per sekund", "radians per second", "radianer per sekund", 9.5492965855137m,
        "rad/sec");

    public static IReadOnlyList<RotationalSpeedUnit> All { get; } = [Rpm, Rps, RadiansPerSecond];

    /// <summary>The base unit: revolutions per minute (rpm).</summary>
    public static RotationalSpeedUnit BaseUnit => Rpm;

    public static bool TryParse(string? input, out RotationalSpeedUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static RotationalSpeedUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown rotational speed unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, RotationalSpeedUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, RotationalSpeedUnit>(StringComparer.OrdinalIgnoreCase);
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
