using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Garment fit/silhouette (<c>passform</c>) used in apparel product feeds and fashion e-commerce,
/// e.g. <c>Slim</c>, <c>Regular</c>, <c>Loose</c>, <c>Oversized</c>, <c>Tailored</c>.
/// </summary>
/// <remarks>
/// <para>Captures the silhouette/fit dimension of a garment, complementing
/// <see cref="AdultClothingSize"/> (numeric/letter size) and <see cref="ClothingGender"/>
/// (target audience). Recognises Swedish synonyms such as <c>Smal</c>, <c>Lös</c>,
/// <c>Skräddarsydd</c>, and common retail aliases like <c>Skinny</c>, <c>Relaxed</c>, <c>Boxy</c>.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Clothing_sizes">Wikipedia — Clothing sizes</see></description></item>
/// </list>
/// </remarks>
public sealed class ClothingFit : IEquatable<ClothingFit>, IComparable<ClothingFit>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Clothing Fit",
        "Passform",
        "🧥",
        ["https://en.wikipedia.org/wiki/Clothing_sizes"]);

    private static readonly Lazy<Dictionary<string, ClothingFit>> Lookup = new(BuildLookup);

    private readonly int _order;

    /// <summary>Canonical English value, e.g. <c>Slim</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Slim</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Smal</c>.</summary>
    public string LocalizedName { get; }

    public static readonly ClothingFit Slim      = new("Slim",      "Slim",      "Smal",         0);
    public static readonly ClothingFit Regular   = new("Regular",   "Regular",   "Normal",       1);
    public static readonly ClothingFit Loose     = new("Loose",     "Loose",     "Lös",          2);
    public static readonly ClothingFit Oversized = new("Oversized", "Oversized", "Oversize",     3);
    public static readonly ClothingFit Tailored  = new("Tailored",  "Tailored",  "Skräddarsydd", 4);

    /// <summary>All predefined clothing fits.</summary>
    public static IReadOnlyList<ClothingFit> All { get; } =
    [
        Slim, Regular, Loose, Oversized, Tailored
    ];

    private ClothingFit(string value, string englishName, string localizedName, int order)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        _order = order;
    }

    /// <summary>
    /// Attempts to parse a clothing fit from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out ClothingFit? result)
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
    /// Parses a clothing fit. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static ClothingFit Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid clothing fit.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized clothing fit.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Smal</c> (Swedish) or <c>Slim</c> (English).
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Slim</c>, <c>Oversized</c>.
    /// Returns <see langword="null"/> when invalid.
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

    /// <summary>Returns the canonical English value, e.g. <c>Slim</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture, e.g. <c>Smal</c> or <c>Slim</c>.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_/]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, ClothingFit> d, ClothingFit value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, ClothingFit> BuildLookup()
    {
        var d = new Dictionary<string, ClothingFit>(StringComparer.OrdinalIgnoreCase);

        foreach (var f in All)
        {
            AddKey(d, f, f.Value);
            AddKey(d, f, f.EnglishName);
            AddKey(d, f, f.LocalizedName);
        }

        AddKey(d, Slim, "Smal");
        AddKey(d, Slim, "Slim fit");
        AddKey(d, Slim, "Slim-fit");
        AddKey(d, Slim, "Tight");
        AddKey(d, Slim, "Skinny");
        AddKey(d, Slim, "Skinny fit");
        AddKey(d, Slim, "Figurnära");
        AddKey(d, Slim, "Åtsittande");

        AddKey(d, Regular, "Normal");
        AddKey(d, Regular, "Standard");
        AddKey(d, Regular, "Regular fit");
        AddKey(d, Regular, "Standard fit");
        AddKey(d, Regular, "Straight");
        AddKey(d, Regular, "Straight fit");
        AddKey(d, Regular, "Klassisk passform");
        AddKey(d, Regular, "Normal passform");
        AddKey(d, Regular, "Rak");
        AddKey(d, Regular, "Rak passform");

        AddKey(d, Loose, "Lös");
        AddKey(d, Loose, "Lös passform");
        AddKey(d, Loose, "Relaxed");
        AddKey(d, Loose, "Relaxed fit");
        AddKey(d, Loose, "Loose fit");
        AddKey(d, Loose, "Vid");
        AddKey(d, Loose, "Vid passform");
        AddKey(d, Loose, "Comfort fit");
        AddKey(d, Loose, "Bekväm passform");

        AddKey(d, Oversized, "Oversize");
        AddKey(d, Oversized, "Oversize fit");
        AddKey(d, Oversized, "Oversized fit");
        AddKey(d, Oversized, "Boxy");
        AddKey(d, Oversized, "Boxy fit");
        AddKey(d, Oversized, "Överdimensionerad");
        AddKey(d, Oversized, "Extra vid");
        AddKey(d, Oversized, "XXL fit");

        AddKey(d, Tailored, "Skräddarsydd");
        AddKey(d, Tailored, "Tailored fit");
        AddKey(d, Tailored, "Slim tailored");
        AddKey(d, Tailored, "Slim-tailored");
        AddKey(d, Tailored, "Anpassad passform");
        AddKey(d, Tailored, "Formell passform");

        return d;
    }

    public static bool operator ==(ClothingFit? a, ClothingFit? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(ClothingFit? a, ClothingFit? b) => !(a == b);

    public bool Equals(ClothingFit? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ClothingFit other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(ClothingFit? other) =>
        other is null ? 1 : _order.CompareTo(other._order);

    public static bool operator <(ClothingFit a, ClothingFit b) => a.CompareTo(b) < 0;
    public static bool operator >(ClothingFit a, ClothingFit b) => a.CompareTo(b) > 0;
    public static bool operator <=(ClothingFit a, ClothingFit b) => a.CompareTo(b) <= 0;
    public static bool operator >=(ClothingFit a, ClothingFit b) => a.CompareTo(b) >= 0;
}
