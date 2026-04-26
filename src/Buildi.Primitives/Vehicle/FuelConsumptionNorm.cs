using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A regulatory test cycle / norm under which fuel consumption (and CO₂) is measured for vehicles
/// (<c>förbrukningsnorm</c>), e.g. <c>NEDC</c>, <c>WLTP</c>, <c>EPA</c>.
/// </summary>
/// <remarks>
/// <para>Captures the major regional test cycles used to declare type-approval fuel consumption.
/// In Sweden (and the EU more broadly), <c>NEDC</c> applied to vehicles type-approved before 2017
/// and was succeeded by <c>WLTP</c>. <c>EPA</c> figures are used in the United States, while
/// <c>JC08</c> and <c>WLTC</c>/<c>CLTC</c> are used in Japan and China respectively.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://unece.org/transport/vehicle-regulations">UNECE — vehicle regulations (WLTP, NEDC)</see></description></item>
/// <item><description><see href="https://www.epa.gov/fueleconomy">US EPA — fuel economy</see></description></item>
/// </list>
/// </remarks>
public sealed class FuelConsumptionNorm : IEquatable<FuelConsumptionNorm>, IComparable<FuelConsumptionNorm>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Fuel Consumption Norm",
        "Förbrukningsnorm",
        "🧪",
        ["https://unece.org/transport/vehicle-regulations", "https://www.epa.gov/fueleconomy"]);

    private static readonly Lazy<Dictionary<string, FuelConsumptionNorm>> Lookup = new(BuildLookup);

    /// <summary>Canonical short code, e.g. <c>WLTP</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Worldwide Harmonised Light Vehicles Test Procedure</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>WLTP-cykel</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Region or jurisdiction in which the norm is primarily used, e.g. <c>EU</c>, <c>US</c>, <c>Japan</c>.</summary>
    public string Region { get; }

    public static readonly FuelConsumptionNorm Nedc = new(
        "NEDC", "New European Driving Cycle", "NEDC-cykel", "EU");

    public static readonly FuelConsumptionNorm Wltp = new(
        "WLTP", "Worldwide Harmonised Light Vehicles Test Procedure", "WLTP-cykel", "EU/Worldwide");

    public static readonly FuelConsumptionNorm Epa = new(
        "EPA", "EPA fuel economy estimate", "EPA-cykel", "US");

    public static readonly FuelConsumptionNorm Jc08 = new(
        "JC08", "JC08 driving cycle", "JC08-cykel", "Japan");

    public static readonly FuelConsumptionNorm Cltc = new(
        "CLTC", "China Light-duty vehicle Test Cycle", "CLTC-cykel", "China");

    public static readonly FuelConsumptionNorm Wltc = new(
        "WLTC", "Worldwide Harmonised Light Vehicles Test Cycle", "WLTC-cykel", "Worldwide");

    public static readonly FuelConsumptionNorm Unknown = new(
        "Unknown", "Unknown", "Okänd", "Unspecified");

    /// <summary>All predefined norms.</summary>
    public static IReadOnlyList<FuelConsumptionNorm> All { get; } =
    [
        Nedc, Wltp, Wltc, Epa, Jc08, Cltc, Unknown
    ];

    private FuelConsumptionNorm(string value, string englishName, string localizedName, string region)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        Region = region;
    }

    public static bool TryParse(string? input, out FuelConsumptionNorm? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var v))
        {
            result = v;
            return true;
        }
        return false;
    }

    public static FuelConsumptionNorm Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid fuel consumption norm.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>WLTP-cykel</c> (Swedish) or
    /// the canonical short code (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var n) ? n!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical short code, e.g. <c>WLTP</c>. Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var n)) return n!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical short code, e.g. <c>WLTP</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : Value;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("ö", "o").Replace("ä", "a").Replace("å", "a");
        folded = Regex.Replace(folded, @"[\s\-_]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, FuelConsumptionNorm> d, FuelConsumptionNorm value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, FuelConsumptionNorm> BuildLookup()
    {
        var d = new Dictionary<string, FuelConsumptionNorm>(StringComparer.OrdinalIgnoreCase);

        foreach (var n in All)
        {
            AddKey(d, n, n.Value);
            AddKey(d, n, n.EnglishName);
            AddKey(d, n, n.LocalizedName);
        }

        AddKey(d, Nedc, "New European Driving Cycle");
        AddKey(d, Wltp, "WLTP cycle");
        AddKey(d, Wltp, "WLTP-cykeln");
        AddKey(d, Wltc, "WLTC cycle");
        AddKey(d, Epa, "EPA estimate");
        AddKey(d, Epa, "EPA fuel economy");
        AddKey(d, Unknown, "Unspecified");
        AddKey(d, Unknown, "Okand");
        AddKey(d, Unknown, "N/A");

        return d;
    }

    public static bool operator ==(FuelConsumptionNorm? a, FuelConsumptionNorm? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(FuelConsumptionNorm? a, FuelConsumptionNorm? b) => !(a == b);

    public bool Equals(FuelConsumptionNorm? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is FuelConsumptionNorm other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(FuelConsumptionNorm? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(FuelConsumptionNorm a, FuelConsumptionNorm b) => a.CompareTo(b) < 0;
    public static bool operator >(FuelConsumptionNorm a, FuelConsumptionNorm b) => a.CompareTo(b) > 0;
    public static bool operator <=(FuelConsumptionNorm a, FuelConsumptionNorm b) => a.CompareTo(b) <= 0;
    public static bool operator >=(FuelConsumptionNorm a, FuelConsumptionNorm b) => a.CompareTo(b) >= 0;
}
