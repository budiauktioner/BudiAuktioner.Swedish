using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Clothing target gender (<c>klädkön</c>) as used in product feeds and fashion e-commerce,
/// e.g. <c>Male</c> (<c>Herr</c>), <c>Female</c> (<c>Dam</c>), <c>Unisex</c>.
/// </summary>
/// <remarks>
/// <para>Parsing accepts English (male, female, unisex, boys, girls), Swedish (herr, dam, pojke, flicka),
/// and common synonyms (man, kvinna, kille, tjej, etc.) — case-insensitive.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://support.google.com/merchants/answer/6324479">Google Merchant Center — gender attribute</see></description></item>
/// <item><description><see href="https://schema.org/GenderType">Schema.org — GenderType</see></description></item>
/// </list>
/// </remarks>
public sealed class ClothingGender : IEquatable<ClothingGender>, IComparable<ClothingGender>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Clothing Gender", "Klädkön", "🚻", ["https://support.google.com/merchants/answer/6324479", "https://schema.org/GenderType"]);

    private static readonly Lazy<Dictionary<string, ClothingGender>> Lookup = new(BuildLookup);

    /// <summary>Canonical English value, e.g. <c>Male</c>, <c>Female</c>, <c>Unisex</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Male</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Swedish display name, e.g. <c>Herr</c>.</summary>
    public string LocalizedName { get; }

    private readonly int _sortOrder;

    public static readonly ClothingGender Male = new("Male", "Male", "Herr", 0);
    public static readonly ClothingGender Female = new("Female", "Female", "Dam", 1);
    public static readonly ClothingGender Unisex = new("Unisex", "Unisex", "Unisex", 2);
    public static readonly ClothingGender Boys = new("Boys", "Boys", "Pojke", 3);
    public static readonly ClothingGender Girls = new("Girls", "Girls", "Flicka", 4);

    /// <summary>All predefined clothing genders.</summary>
    public static IReadOnlyList<ClothingGender> All { get; } =
    [
        Male, Female, Unisex, Boys, Girls
    ];

    private ClothingGender(string value, string englishName, string localizedName, int sortOrder)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        _sortOrder = sortOrder;
    }

    /// <summary>
    /// Attempts to parse a clothing gender from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out ClothingGender? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = trimmed.ToLowerInvariant();
        if (Lookup.Value.TryGetValue(key, out var fromDict))
        {
            result = fromDict;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses a clothing gender. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static ClothingGender Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid clothing gender.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized clothing gender.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Herr</c> (Swedish) or <c>Male</c> (English).
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var e) ? e!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical value, e.g. <c>Male</c>, <c>Female</c>, <c>Unisex</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical value.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical value, e.g. <c>Male</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture, e.g. <c>Herr</c> or <c>Male</c>.</summary>
    public override string ToString() => DisplayName;

    private static void AddKey(Dictionary<string, ClothingGender> d, ClothingGender value, string key)
    {
        var k = key.ToLowerInvariant();
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, ClothingGender> BuildLookup()
    {
        var d = new Dictionary<string, ClothingGender>(StringComparer.OrdinalIgnoreCase);

        foreach (var g in All)
        {
            AddKey(d, g, g.Value);
            AddKey(d, g, g.EnglishName);
            AddKey(d, g, g.LocalizedName);
        }

        // Male aliases
        AddKey(d, Male, "man");
        AddKey(d, Male, "men");
        AddKey(d, Male, "men's");
        AddKey(d, Male, "mens");
        AddKey(d, Male, "herrar");
        AddKey(d, Male, "herrkläder");
        AddKey(d, Male, "gentleman");
        AddKey(d, Male, "gentlemen");

        // Female aliases
        AddKey(d, Female, "woman");
        AddKey(d, Female, "women");
        AddKey(d, Female, "women's");
        AddKey(d, Female, "womens");
        AddKey(d, Female, "damer");
        AddKey(d, Female, "kvinna");
        AddKey(d, Female, "kvinnor");
        AddKey(d, Female, "damkläder");
        AddKey(d, Female, "lady");
        AddKey(d, Female, "ladies");

        // Unisex aliases
        AddKey(d, Unisex, "uni");
        AddKey(d, Unisex, "both");

        // Boys aliases
        AddKey(d, Boys, "boy");
        AddKey(d, Boys, "pojkar");
        AddKey(d, Boys, "pojk");
        AddKey(d, Boys, "kille");
        AddKey(d, Boys, "killar");

        // Girls aliases
        AddKey(d, Girls, "girl");
        AddKey(d, Girls, "flickor");
        AddKey(d, Girls, "tjej");
        AddKey(d, Girls, "tjejer");

        return d;
    }

    public static bool operator ==(ClothingGender? a, ClothingGender? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(ClothingGender? a, ClothingGender? b) => !(a == b);

    public bool Equals(ClothingGender? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ClothingGender other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(ClothingGender? other) =>
        other is null ? 1 : _sortOrder.CompareTo(other._sortOrder);

    public static bool operator <(ClothingGender left, ClothingGender right) => left.CompareTo(right) < 0;
    public static bool operator >(ClothingGender left, ClothingGender right) => left.CompareTo(right) > 0;
    public static bool operator <=(ClothingGender left, ClothingGender right) => left.CompareTo(right) <= 0;
    public static bool operator >=(ClothingGender left, ClothingGender right) => left.CompareTo(right) >= 0;
}
