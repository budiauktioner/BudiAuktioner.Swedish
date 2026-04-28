using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A vehicle suspension/damping technology (<c>fjädring</c>),
/// e.g. <c>Coil spring</c>, <c>Leaf spring</c>, <c>Air</c>, <c>Hydropneumatic</c>, <c>Torsion bar</c>.
/// </summary>
/// <remarks>
/// <para>Captures the most common suspension technologies used on cars, trucks, buses, trailers,
/// caravans, and heavy machinery, with Swedish-language synonyms.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Car_suspension">Wikipedia — Car suspension</see></description></item>
/// </list>
/// </remarks>
public sealed class SuspensionType : IEquatable<SuspensionType>, IComparable<SuspensionType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Suspension Type",
        "Fjädring",
        "🛞",
        ["https://en.wikipedia.org/wiki/Car_suspension"]);

    private static readonly Lazy<Dictionary<string, SuspensionType>> Lookup = new(BuildLookup);

    /// <summary>Canonical English value, e.g. <c>Coil spring</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Coil spring</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Spiralfjäder</c>.</summary>
    public string LocalizedName { get; }

    public static readonly SuspensionType CoilSpring = new("Coil spring", "Coil spring", "Spiralfjäder");
    public static readonly SuspensionType LeafSpring = new("Leaf spring", "Leaf spring", "Bladfjäder");
    public static readonly SuspensionType Air = new("Air", "Air suspension", "Luftfjädring");
    public static readonly SuspensionType Hydropneumatic = new("Hydropneumatic", "Hydropneumatic", "Hydropneumatisk");
    public static readonly SuspensionType TorsionBar = new("Torsion bar", "Torsion bar", "Torsionsstav");
    public static readonly SuspensionType Adaptive = new("Adaptive", "Adaptive", "Adaptiv");
    public static readonly SuspensionType MagneticRide = new("Magnetic ride", "Magnetic ride", "Magnetisk fjädring");
    public static readonly SuspensionType Independent = new("Independent", "Independent", "Individuell hjulupphängning");
    public static readonly SuspensionType MacPherson = new("MacPherson strut", "MacPherson strut", "MacPherson-fjäderben");
    public static readonly SuspensionType DoubleWishbone = new("Double wishbone", "Double wishbone", "Dubbla A-armar");
    public static readonly SuspensionType MultiLink = new("Multi-link", "Multi-link", "Multilänk");
    public static readonly SuspensionType SolidAxle = new("Solid axle", "Solid axle", "Stel axel");
    public static readonly SuspensionType Rigid = new("Rigid", "Rigid (unsprung)", "Stel (utan fjädring)");

    /// <summary>All predefined suspension types.</summary>
    public static IReadOnlyList<SuspensionType> All { get; } =
    [
        CoilSpring, LeafSpring, Air, Hydropneumatic, TorsionBar, Adaptive, MagneticRide,
        Independent, MacPherson, DoubleWishbone, MultiLink, SolidAxle, Rigid
    ];

    private SuspensionType(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    public static bool TryParse(string? input, out SuspensionType? result)
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

    public static SuspensionType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid suspension type.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Luftfjädring</c> (Swedish) or
    /// <c>Air suspension</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Air</c>, <c>Coil spring</c>, <c>Multi-link</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical English value, e.g. <c>Coil spring</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_/()]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, SuspensionType> d, SuspensionType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, SuspensionType> BuildLookup()
    {
        var d = new Dictionary<string, SuspensionType>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in All)
        {
            AddKey(d, s, s.Value);
            AddKey(d, s, s.EnglishName);
            AddKey(d, s, s.LocalizedName);
        }

        AddKey(d, CoilSpring, "Coil");
        AddKey(d, CoilSpring, "Coil-spring");
        AddKey(d, CoilSpring, "Spiralfjäder");
        AddKey(d, CoilSpring, "Skruvfjäder");
        AddKey(d, LeafSpring, "Leaf");
        AddKey(d, LeafSpring, "Leaf-spring");
        AddKey(d, LeafSpring, "Bladfjäder");
        AddKey(d, Air, "Air spring");
        AddKey(d, Air, "Pneumatic");
        AddKey(d, Air, "Luftfjäder");
        AddKey(d, Air, "Bälg");
        AddKey(d, Air, "Bellow");
        AddKey(d, Hydropneumatic, "Hydraulic");
        AddKey(d, Hydropneumatic, "Hydraulisk");
        AddKey(d, TorsionBar, "Torsion");
        AddKey(d, TorsionBar, "Torsionsfjäder");
        AddKey(d, Adaptive, "Adaptive damping");
        AddKey(d, Adaptive, "Active suspension");
        AddKey(d, Adaptive, "Aktiv fjädring");
        AddKey(d, MagneticRide, "MagneRide");
        AddKey(d, MagneticRide, "Magnetorheological");
        AddKey(d, Independent, "IRS");
        AddKey(d, Independent, "Independent rear suspension");
        AddKey(d, MacPherson, "MacPherson");
        AddKey(d, MacPherson, "McPherson");
        AddKey(d, DoubleWishbone, "Wishbone");
        AddKey(d, DoubleWishbone, "Double-wishbone");
        AddKey(d, DoubleWishbone, "A-arms");
        AddKey(d, MultiLink, "Multilink");
        AddKey(d, MultiLink, "Multi link");
        AddKey(d, SolidAxle, "Live axle");
        AddKey(d, SolidAxle, "Beam axle");
        AddKey(d, SolidAxle, "Stel bakaxel");
        AddKey(d, Rigid, "Unsprung");
        AddKey(d, Rigid, "Ofjädrad");

        return d;
    }

    public static bool operator ==(SuspensionType? a, SuspensionType? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(SuspensionType? a, SuspensionType? b) => !(a == b);

    public bool Equals(SuspensionType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SuspensionType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(SuspensionType? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(SuspensionType a, SuspensionType b) => a.CompareTo(b) < 0;
    public static bool operator >(SuspensionType a, SuspensionType b) => a.CompareTo(b) > 0;
    public static bool operator <=(SuspensionType a, SuspensionType b) => a.CompareTo(b) <= 0;
    public static bool operator >=(SuspensionType a, SuspensionType b) => a.CompareTo(b) >= 0;
}
