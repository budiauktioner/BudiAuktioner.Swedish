using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A temperature scale identified by its symbol. Each unit exposes English and Swedish names
/// and singular and plural forms. Conversions to the thermodynamic temperature base (kelvin)
/// use offsets, not a single multiplicative factor — see <see cref="Temperature"/>.
/// </summary>
public sealed class TemperatureUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Temperature Unit", "Temperaturenhet", "🌡️", []);

    private static readonly Lazy<Dictionary<string, TemperatureUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    private readonly string[] _aliases;

    private TemperatureUnit(string symbol, string englishName, string localizedName,
        string pluralEnglish, string pluralSwedish, params string[] aliases)
    {
        Symbol = symbol;
        EnglishName = englishName;
        LocalizedName = localizedName;
        PluralEnglish = pluralEnglish;
        PluralSwedish = pluralSwedish;
        _aliases = aliases;
    }

    public static TemperatureUnit Celsius { get; } = new(
        "°C", "degree Celsius", "grad Celsius", "degrees Celsius", "grader Celsius",
        "C", "celsius", "grader");

    public static TemperatureUnit Fahrenheit { get; } = new(
        "°F", "degree Fahrenheit", "grad Fahrenheit", "degrees Fahrenheit", "grader Fahrenheit",
        "F", "fahrenheit");

    public static TemperatureUnit Kelvin { get; } = new(
        "K", "kelvin", "kelvin", "kelvin", "kelvin", "kelvin");

    public static IReadOnlyList<TemperatureUnit> All { get; } = [Celsius, Fahrenheit, Kelvin];

    /// <summary>The SI base unit for thermodynamic temperature: kelvin.</summary>
    public static TemperatureUnit BaseUnit => Kelvin;

    public static bool TryParse(string? input, out TemperatureUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static TemperatureUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown temperature unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, TemperatureUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, TemperatureUnit>(StringComparer.OrdinalIgnoreCase);
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
