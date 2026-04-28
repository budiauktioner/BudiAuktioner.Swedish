using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A tire season/intended-use category (<c>däcktyp</c>),
/// e.g. <c>Summer</c>, <c>Winter (studded)</c>, <c>Winter (friction)</c>, <c>All-season</c>.
/// </summary>
/// <remarks>
/// <para>Mirrors how Swedish tire shops and the auction market categorize tires by season and
/// stud configuration. Use <see cref="TireDimension"/> for the actual size notation
/// (e.g. <c>205/55R16</c>) and this type for the season/use class.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/sv/vagtrafik/Fordon/Fordonsregler/Vinterdack/">Transportstyrelsen — Vinterdäck</see></description></item>
/// </list>
/// </remarks>
public sealed class TireType : IEquatable<TireType>, IComparable<TireType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Tire Type",
        "Däcktyp",
        "🛞",
        ["https://www.transportstyrelsen.se/sv/vagtrafik/Fordon/Fordonsregler/Vinterdack/"]);

    private static readonly Lazy<Dictionary<string, TireType>> Lookup = new(BuildLookup);

    /// <summary>Canonical English value, e.g. <c>Summer</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Summer</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Sommardäck</c>.</summary>
    public string LocalizedName { get; }

    public static readonly TireType Summer = new("Summer", "Summer", "Sommardäck");
    public static readonly TireType WinterStudded = new("Winter (studded)", "Winter (studded)", "Vinterdäck (dubb)");
    public static readonly TireType WinterFriction = new("Winter (friction)", "Winter (friction)", "Vinterdäck (friktion)");
    public static readonly TireType AllSeason = new("All-season", "All-season", "Helårsdäck");
    public static readonly TireType AllTerrain = new("All-terrain", "All-terrain", "Terrängdäck");
    public static readonly TireType MudTerrain = new("Mud-terrain", "Mud-terrain", "Lerdäck");
    public static readonly TireType Track = new("Track", "Track / racing", "Bandäck / racing");
    public static readonly TireType Industrial = new("Industrial", "Industrial", "Industridäck");
    public static readonly TireType Agricultural = new("Agricultural", "Agricultural", "Lantbruksdäck");
    public static readonly TireType Spare = new("Spare", "Spare / temporary", "Reservdäck");

    /// <summary>All predefined tire types.</summary>
    public static IReadOnlyList<TireType> All { get; } =
    [
        Summer, WinterStudded, WinterFriction, AllSeason, AllTerrain, MudTerrain,
        Track, Industrial, Agricultural, Spare
    ];

    private TireType(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    public static bool TryParse(string? input, out TireType? result)
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

    public static TireType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid tire type.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Sommardäck</c> (Swedish) or
    /// <c>Summer</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Summer</c>, <c>Winter (studded)</c>, <c>All-season</c>.
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

    /// <summary>Returns the canonical English value, e.g. <c>Summer</c>.</summary>
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

    private static void AddKey(Dictionary<string, TireType> d, TireType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, TireType> BuildLookup()
    {
        var d = new Dictionary<string, TireType>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in All)
        {
            AddKey(d, t, t.Value);
            AddKey(d, t, t.EnglishName);
            AddKey(d, t, t.LocalizedName);
        }

        AddKey(d, Summer, "Summer tires");
        AddKey(d, Summer, "Sommar");
        AddKey(d, Summer, "Sommardäck");
        AddKey(d, WinterStudded, "Studded winter");
        AddKey(d, WinterStudded, "Studded");
        AddKey(d, WinterStudded, "Dubbdäck");
        AddKey(d, WinterStudded, "Dubbade vinterdäck");
        AddKey(d, WinterStudded, "Vinterdäck dubbade");
        AddKey(d, WinterFriction, "Friction");
        AddKey(d, WinterFriction, "Studless winter");
        AddKey(d, WinterFriction, "Nordic winter");
        AddKey(d, WinterFriction, "Friktionsdäck");
        AddKey(d, WinterFriction, "Odubbade vinterdäck");
        AddKey(d, AllSeason, "All season");
        AddKey(d, AllSeason, "All-weather");
        AddKey(d, AllSeason, "Allroundsdäck");
        AddKey(d, AllSeason, "Helår");
        AddKey(d, AllTerrain, "All terrain");
        AddKey(d, AllTerrain, "AT");
        AddKey(d, AllTerrain, "Off-road");
        AddKey(d, MudTerrain, "Mud terrain");
        AddKey(d, MudTerrain, "MT");
        AddKey(d, Track, "Racing");
        AddKey(d, Track, "Slick");
        AddKey(d, Track, "Bandäck");
        AddKey(d, Industrial, "Industri");
        AddKey(d, Industrial, "Industridäck");
        AddKey(d, Industrial, "Solid tire");
        AddKey(d, Industrial, "Massivdäck");
        AddKey(d, Agricultural, "Lantbruk");
        AddKey(d, Agricultural, "Traktordäck");
        AddKey(d, Agricultural, "Tractor");
        AddKey(d, Spare, "Spare wheel");
        AddKey(d, Spare, "Reservhjul");
        AddKey(d, Spare, "Donut");

        return d;
    }

    public static bool operator ==(TireType? a, TireType? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(TireType? a, TireType? b) => !(a == b);

    public bool Equals(TireType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is TireType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(TireType? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(TireType a, TireType b) => a.CompareTo(b) < 0;
    public static bool operator >(TireType a, TireType b) => a.CompareTo(b) > 0;
    public static bool operator <=(TireType a, TireType b) => a.CompareTo(b) <= 0;
    public static bool operator >=(TireType a, TireType b) => a.CompareTo(b) >= 0;
}
