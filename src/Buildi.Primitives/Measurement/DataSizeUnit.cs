using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A unit of digital data size identified by its symbol. Each unit exposes English and Swedish names,
/// singular and plural forms, and a conversion factor to the base unit (byte).
/// </summary>
public sealed class DataSizeUnit
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Data Size Unit", "Datastorleksenhet", "💾", []);

    private static readonly Lazy<Dictionary<string, DataSizeUnit>> BySymbol = new(BuildSymbolIndex);

    public string Symbol { get; }
    public string EnglishName { get; }
    public string LocalizedName { get; }
    public string PluralEnglish { get; }
    public string PluralSwedish { get; }

    /// <summary>Multiply a value in this unit by this factor to get the value in bytes.</summary>
    public decimal ToBaseUnitFactor { get; }

    private readonly string[] _aliases;

    private DataSizeUnit(string symbol, string englishName, string localizedName,
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

    public static DataSizeUnit Byte { get; } = new("B", "byte", "byte", "bytes", "byte", 1m, "byte", "bytes");
    public static DataSizeUnit Kilobyte { get; } = new("KB", "kilobyte", "kilobyte", "kilobytes", "kilobyte", 1000m, "kilobyte", "kilobytes");
    public static DataSizeUnit Megabyte { get; } = new("MB", "megabyte", "megabyte", "megabytes", "megabyte", 1_000_000m, "megabyte", "megabytes");
    public static DataSizeUnit Gigabyte { get; } = new("GB", "gigabyte", "gigabyte", "gigabytes", "gigabyte", 1_000_000_000m, "gigabyte", "gigabytes");
    public static DataSizeUnit Terabyte { get; } = new("TB", "terabyte", "terabyte", "terabytes", "terabyte", 1_000_000_000_000m, "terabyte", "terabytes");
    public static DataSizeUnit Petabyte { get; } = new("PB", "petabyte", "petabyte", "petabytes", "petabyte", 1_000_000_000_000_000m, "petabyte", "petabytes");
    public static DataSizeUnit Exabyte { get; } = new("EB", "exabyte", "exabyte", "exabytes", "exabyte", 1_000_000_000_000_000_000m, "exabyte", "exabytes");
    public static DataSizeUnit Kibibyte { get; } = new("KiB", "kibibyte", "kibibyte", "kibibytes", "kibibyte", 1024m, "kibibyte", "kibibytes");
    public static DataSizeUnit Mebibyte { get; } = new("MiB", "mebibyte", "mebibyte", "mebibytes", "mebibyte", 1048576m, "mebibyte", "mebibytes");
    public static DataSizeUnit Gibibyte { get; } = new("GiB", "gibibyte", "gibibyte", "gibibytes", "gibibyte", 1073741824m, "gibibyte", "gibibytes");
    public static DataSizeUnit Tebibyte { get; } = new("TiB", "tebibyte", "tebibyte", "tebibytes", "tebibyte", 1099511627776m, "tebibyte", "tebibytes");
    public static DataSizeUnit Pebibyte { get; } = new("PiB", "pebibyte", "pebibyte", "pebibytes", "pebibyte", 1125899906842624m, "pebibyte", "pebibytes");
    public static DataSizeUnit Exbibyte { get; } = new("EiB", "exbibyte", "exbibyte", "exbibytes", "exbibyte", 1152921504606846976m, "exbibyte", "exbibytes");

    public static IReadOnlyList<DataSizeUnit> All { get; } =
        [Byte, Kilobyte, Megabyte, Gigabyte, Terabyte, Petabyte, Exabyte, Kibibyte, Mebibyte, Gibibyte, Tebibyte, Pebibyte, Exbibyte];

    /// <summary>The base unit: byte.</summary>
    public static DataSizeUnit BaseUnit => Byte;

    /// <summary>
    /// Decimal SI units ordered small-to-large, suitable for auto-scaling display.
    /// Excludes binary units (KiB, MiB, etc.).
    /// </summary>
    public static IReadOnlyList<DataSizeUnit> NaturalScale { get; } =
        [Byte, Kilobyte, Megabyte, Gigabyte, Terabyte, Petabyte, Exabyte];

    /// <summary>
    /// Returns the most human-readable unit for the given <paramref name="baseValue"/> (in bytes).
    /// Picks the largest unit on <see cref="NaturalScale"/> where the absolute value is at least 1.
    /// </summary>
    public static DataSizeUnit GetNatural(decimal baseValue)
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

    public static bool TryParse(string? input, out DataSizeUnit? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return BySymbol.Value.TryGetValue(input.Trim(), out result);
    }

    public static DataSizeUnit Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown data size unit.", nameof(input));
        return result!;
    }

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    private static Dictionary<string, DataSizeUnit> BuildSymbolIndex()
    {
        var dict = new Dictionary<string, DataSizeUnit>(StringComparer.OrdinalIgnoreCase);
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
