using Buildi.Primitives;

namespace Buildi.Primitives.Geography;

/// <summary>
/// A continent (<c>kontinent</c>) identified by its two-letter code, English name, or Swedish name.
/// Seven continents are recognized: Africa, Antarctica, Asia, Europe, North America, Oceania, and South America.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://unstats.un.org/unsd/methodology/m49/">UN M49 Standard</see> — geographic regions</description></item>
/// <item><description><see href="https://datahub.io/core/continent-codes">Datahub — continent codes</see></description></item>
/// </list>
/// </remarks>
public sealed class Continent : IEquatable<Continent>, IComparable<Continent>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Continent", "Kontinent", "🗺️", ["https://unstats.un.org/unsd/methodology/m49/", "https://datahub.io/core/continent-codes"]);

    private const int MaxInputLength = 50;

    private static readonly Dictionary<string, Continent> ByCode;
    private static readonly Dictionary<string, Continent> ByName;

    public static Continent Africa { get; } = new("AF", "Africa", "Afrika");
    public static Continent Antarctica { get; } = new("AN", "Antarctica", "Antarktis");
    public static Continent Asia { get; } = new("AS", "Asia", "Asien");
    public static Continent Europe { get; } = new("EU", "Europe", "Europa");
    public static Continent NorthAmerica { get; } = new("NA", "North America", "Nordamerika");
    public static Continent Oceania { get; } = new("OC", "Oceania", "Oceanien");
    public static Continent SouthAmerica { get; } = new("SA", "South America", "Sydamerika");

    /// <summary>All seven continents.</summary>
    public static IReadOnlyList<Continent> All { get; } = [Africa, Antarctica, Asia, Europe, NorthAmerica, Oceania, SouthAmerica];

    /// <summary>Two-letter continent code, e.g. <c>EU</c>.</summary>
    public string Code { get; }

    /// <summary>English name, e.g. <c>Europe</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Swedish name, e.g. <c>Europa</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Display name in the current UI language, e.g. <c>Europa</c> or <c>Europe</c> depending on <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    public string Value => Code;

    private Continent(string code, string englishName, string localizedName)
    {
        Code = code;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    static Continent()
    {
        ByCode = new(StringComparer.OrdinalIgnoreCase);
        ByName = new(StringComparer.OrdinalIgnoreCase);

        foreach (var c in All)
        {
            ByCode[c.Code] = c;
            ByName[c.EnglishName] = c;
            ByName[c.LocalizedName] = c;
        }

        ByName["North America"] = NorthAmerica;
        ByName["South America"] = SouthAmerica;
        ByName["Nordamerika"] = NorthAmerica;
        ByName["Sydamerika"] = SouthAmerica;
    }

    public static bool TryParse(string? input, out Continent? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var trimmed = input!.Trim();
        if (trimmed.Length > MaxInputLength) return false;

        if (ByCode.TryGetValue(trimmed, out result)) return true;
        if (ByName.TryGetValue(trimmed, out result)) return true;

        return false;
    }

    public static Continent Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Unknown continent.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the continent in the current display language, e.g. <c>Europa</c> or <c>Europe</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => TryParse(input, out var r) ? r!.DisplayName
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
            : null;

    /// <summary>
    /// Returns the normalized two-letter continent code, e.g. <c>EU</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the two-letter continent code, e.g. <c>EU</c>.</summary>
    public string ToNormalizedString() => Code;

    /// <summary>Returns the continent in the current display language, e.g. <c>Europa</c> or <c>Europe</c>.</summary>
    public override string ToString() => DisplayName;

    public bool Equals(Continent? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is Continent other && Equals(other);
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Continent? a, Continent? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Continent? a, Continent? b) => !(a == b);
    public int CompareTo(Continent? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Continent left, Continent right) => left.CompareTo(right) < 0;
    public static bool operator >(Continent left, Continent right) => left.CompareTo(right) > 0;
    public static bool operator <=(Continent left, Continent right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Continent left, Continent right) => left.CompareTo(right) >= 0;
}
