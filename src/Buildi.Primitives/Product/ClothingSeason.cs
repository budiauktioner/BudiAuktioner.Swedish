using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Clothing/seasonal product season (<c>säsong</c>) used in fashion product feeds and retail catalogs,
/// e.g. <c>Spring</c>, <c>Summer</c>, <c>Autumn</c>, <c>Winter</c>, <c>All-Season</c>.
/// </summary>
/// <remarks>
/// <para>Captures the seasonal classification commonly used by fashion retailers, Google Merchant
/// Center seasonal product feeds, and outdoor/equipment marketplaces. Each entry exposes the
/// calendar months it typically covers in the Northern Hemisphere and an <see cref="IsAllSeason"/>
/// flag for year-round products.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://support.google.com/merchants/answer/6324470">Google Merchant Center — apparel attributes</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Season">Wikipedia — Season</see></description></item>
/// </list>
/// </remarks>
public sealed class ClothingSeason : IEquatable<ClothingSeason>, IComparable<ClothingSeason>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Clothing Season",
        "Säsong",
        "🍂",
        ["https://support.google.com/merchants/answer/6324470", "https://en.wikipedia.org/wiki/Season"]);

    private static readonly Lazy<Dictionary<string, ClothingSeason>> Lookup = new(BuildLookup);

    private readonly int _order;

    /// <summary>Canonical English value, e.g. <c>Spring</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Spring</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Vår</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>
    /// Calendar months (1–12) typically covered by this season in the Northern Hemisphere.
    /// All-season returns months 1–12.
    /// </summary>
    public IReadOnlyList<int> MonthsCovered { get; }

    /// <summary>
    /// Returns <see langword="true"/> for the year-round / all-season entry.
    /// </summary>
    public bool IsAllSeason { get; }

    public static readonly ClothingSeason Spring     = new("Spring",     "Spring",     "Vår",        [3, 4, 5],            isAllSeason: false, 0);
    public static readonly ClothingSeason Summer     = new("Summer",     "Summer",     "Sommar",     [6, 7, 8],            isAllSeason: false, 1);
    public static readonly ClothingSeason Autumn     = new("Autumn",     "Autumn",     "Höst",       [9, 10, 11],          isAllSeason: false, 2);
    public static readonly ClothingSeason Winter     = new("Winter",     "Winter",     "Vinter",     [12, 1, 2],           isAllSeason: false, 3);
    public static readonly ClothingSeason AllSeason  = new("All-Season", "All-Season", "Året runt",  [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12], isAllSeason: true, 4);

    /// <summary>All predefined clothing seasons.</summary>
    public static IReadOnlyList<ClothingSeason> All { get; } =
    [
        Spring, Summer, Autumn, Winter, AllSeason
    ];

    private ClothingSeason(string value, string englishName, string localizedName, int[] monthsCovered, bool isAllSeason, int order)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        MonthsCovered = monthsCovered;
        IsAllSeason = isAllSeason;
        _order = order;
    }

    /// <summary>
    /// Attempts to parse a clothing season from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out ClothingSeason? result)
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

    /// <summary>
    /// Parses a clothing season. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static ClothingSeason Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid clothing season.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized clothing season.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Vår</c> (Swedish) or <c>Spring</c> (English).
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Spring</c>, <c>All-Season</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical value.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical English value, e.g. <c>Spring</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture, e.g. <c>Vår</c> or <c>Spring</c>.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_/]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, ClothingSeason> d, ClothingSeason value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, ClothingSeason> BuildLookup()
    {
        var d = new Dictionary<string, ClothingSeason>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in All)
        {
            AddKey(d, s, s.Value);
            AddKey(d, s, s.EnglishName);
            AddKey(d, s, s.LocalizedName);
        }

        AddKey(d, Spring, "Vår");
        AddKey(d, Spring, "Vårsäsong");
        AddKey(d, Spring, "Springtime");
        AddKey(d, Spring, "SS");
        AddKey(d, Spring, "Spring/Summer");

        AddKey(d, Summer, "Sommar");
        AddKey(d, Summer, "Sommarsäsong");
        AddKey(d, Summer, "Summertime");

        AddKey(d, Autumn, "Höst");
        AddKey(d, Autumn, "Höstsäsong");
        AddKey(d, Autumn, "Fall");
        AddKey(d, Autumn, "Autumnal");
        AddKey(d, Autumn, "AW");
        AddKey(d, Autumn, "FW");
        AddKey(d, Autumn, "Autumn/Winter");
        AddKey(d, Autumn, "Fall/Winter");

        AddKey(d, Winter, "Vinter");
        AddKey(d, Winter, "Vintersäsong");
        AddKey(d, Winter, "Wintertime");

        AddKey(d, AllSeason, "All Season");
        AddKey(d, AllSeason, "All-season");
        AddKey(d, AllSeason, "Allseason");
        AddKey(d, AllSeason, "All-year");
        AddKey(d, AllSeason, "All year");
        AddKey(d, AllSeason, "Year-round");
        AddKey(d, AllSeason, "Year round");
        AddKey(d, AllSeason, "Året om");
        AddKey(d, AllSeason, "Året runt");
        AddKey(d, AllSeason, "Helår");
        AddKey(d, AllSeason, "Allroundsäsong");
        AddKey(d, AllSeason, "4-season");
        AddKey(d, AllSeason, "Four-season");
        AddKey(d, AllSeason, "Multi-season");

        return d;
    }

    public static bool operator ==(ClothingSeason? a, ClothingSeason? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(ClothingSeason? a, ClothingSeason? b) => !(a == b);

    public bool Equals(ClothingSeason? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ClothingSeason other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(ClothingSeason? other) =>
        other is null ? 1 : _order.CompareTo(other._order);

    public static bool operator <(ClothingSeason a, ClothingSeason b) => a.CompareTo(b) < 0;
    public static bool operator >(ClothingSeason a, ClothingSeason b) => a.CompareTo(b) > 0;
    public static bool operator <=(ClothingSeason a, ClothingSeason b) => a.CompareTo(b) <= 0;
    public static bool operator >=(ClothingSeason a, ClothingSeason b) => a.CompareTo(b) >= 0;
}
