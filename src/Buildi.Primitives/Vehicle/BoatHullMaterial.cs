using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// The material used for a boat's hull (<c>skrovmaterial</c>),
/// e.g. <c>Glasfiber</c>, <c>Aluminium</c>, <c>Stål</c>.
/// </summary>
/// <remarks>
/// <para>Captures the most common Swedish recreational and commercial hull materials:
/// fibreglass (<c>glasfiber</c>), aluminium, steel, wood, plastic (rotomoulded
/// polyethylene), carbon fibre, and inflatable PVC/Hypalon.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/sv/sjofart/Fritidsbatar/">Transportstyrelsen — fritidsbåtar</see></description></item>
/// </list>
/// </remarks>
public sealed class BoatHullMaterial : IEquatable<BoatHullMaterial>, IComparable<BoatHullMaterial>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Boat Hull Material",
        "Båt skrovmaterial",
        "🛥️",
        ["https://www.transportstyrelsen.se/sv/sjofart/Fritidsbatar/"]);

    private static readonly Lazy<Dictionary<string, BoatHullMaterial>> Lookup = new(BuildLookup);

    /// <summary>Canonical English value, e.g. <c>Fiberglass</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Fiberglass</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Glasfiber</c>.</summary>
    public string LocalizedName { get; }

    public static readonly BoatHullMaterial Fiberglass = new("Fiberglass", "Fiberglass", "Glasfiber");
    public static readonly BoatHullMaterial Aluminum = new("Aluminum", "Aluminum", "Aluminium");
    public static readonly BoatHullMaterial Steel = new("Steel", "Steel", "Stål");
    public static readonly BoatHullMaterial Wood = new("Wood", "Wood", "Trä");
    public static readonly BoatHullMaterial Plastic = new("Plastic", "Plastic", "Plast");
    public static readonly BoatHullMaterial CarbonFiber = new("Carbon fiber", "Carbon fiber", "Kolfiber");
    public static readonly BoatHullMaterial Inflatable = new("Inflatable", "Inflatable PVC/Hypalon", "Uppblåsbar (PVC/Hypalon)");
    public static readonly BoatHullMaterial Ferrocement = new("Ferrocement", "Ferrocement", "Cementarmerad");
    public static readonly BoatHullMaterial Composite = new("Composite", "Composite", "Komposit");

    /// <summary>All predefined hull materials.</summary>
    public static IReadOnlyList<BoatHullMaterial> All { get; } =
    [
        Fiberglass, Aluminum, Steel, Wood, Plastic, CarbonFiber, Inflatable, Ferrocement, Composite
    ];

    private BoatHullMaterial(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    public static bool TryParse(string? input, out BoatHullMaterial? result)
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

    public static BoatHullMaterial Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid boat hull material.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Glasfiber</c> (Swedish) or
    /// <c>Fiberglass</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Fiberglass</c>. Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical English value, e.g. <c>Fiberglass</c>.</summary>
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

    private static void AddKey(Dictionary<string, BoatHullMaterial> d, BoatHullMaterial value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, BoatHullMaterial> BuildLookup()
    {
        var d = new Dictionary<string, BoatHullMaterial>(StringComparer.OrdinalIgnoreCase);

        foreach (var m in All)
        {
            AddKey(d, m, m.Value);
            AddKey(d, m, m.EnglishName);
            AddKey(d, m, m.LocalizedName);
        }

        AddKey(d, Fiberglass, "Fibreglass");
        AddKey(d, Fiberglass, "GRP");
        AddKey(d, Fiberglass, "GFK");
        AddKey(d, Fiberglass, "Glasfiberarmerad plast");
        AddKey(d, Fiberglass, "Glasfiberplast");
        AddKey(d, Fiberglass, "Plastbåt");
        AddKey(d, Aluminum, "Aluminium");
        AddKey(d, Aluminum, "Alu");
        AddKey(d, Steel, "Stål");
        AddKey(d, Steel, "Järn");
        AddKey(d, Wood, "Mahogny");
        AddKey(d, Wood, "Mahogany");
        AddKey(d, Wood, "Wooden");
        AddKey(d, Plastic, "Polyeten");
        AddKey(d, Plastic, "Polyethylene");
        AddKey(d, Plastic, "PE");
        AddKey(d, Plastic, "Roplene");
        AddKey(d, Plastic, "Roto-moulded");
        AddKey(d, Plastic, "Rotomoulded");
        AddKey(d, CarbonFiber, "Carbon");
        AddKey(d, CarbonFiber, "Kol");
        AddKey(d, Inflatable, "PVC");
        AddKey(d, Inflatable, "Hypalon");
        AddKey(d, Inflatable, "Gummibåt");
        AddKey(d, Inflatable, "RIB");
        AddKey(d, Composite, "Kompositmaterial");

        return d;
    }

    public static bool operator ==(BoatHullMaterial? a, BoatHullMaterial? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(BoatHullMaterial? a, BoatHullMaterial? b) => !(a == b);

    public bool Equals(BoatHullMaterial? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is BoatHullMaterial other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(BoatHullMaterial? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(BoatHullMaterial a, BoatHullMaterial b) => a.CompareTo(b) < 0;
    public static bool operator >(BoatHullMaterial a, BoatHullMaterial b) => a.CompareTo(b) > 0;
    public static bool operator <=(BoatHullMaterial a, BoatHullMaterial b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BoatHullMaterial a, BoatHullMaterial b) => a.CompareTo(b) >= 0;
}
