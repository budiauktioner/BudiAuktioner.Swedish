using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// The track (continuous tread) material/construction used on tracked vehicles
/// (<c>bandtyp</c>) such as excavators, dumpers, snow groomers, and military vehicles,
/// e.g. <c>Steel</c>, <c>Rubber</c>, <c>Polyurethane</c>, <c>Half-track</c>.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Continuous_track">Wikipedia — Continuous track</see></description></item>
/// </list>
/// </remarks>
public sealed class TrackType : IEquatable<TrackType>, IComparable<TrackType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Track Type",
        "Bandtyp",
        "🚜",
        ["https://en.wikipedia.org/wiki/Continuous_track"]);

    private static readonly Lazy<Dictionary<string, TrackType>> Lookup = new(BuildLookup);

    /// <summary>Canonical English value, e.g. <c>Steel</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Steel tracks</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Stålband</c>.</summary>
    public string LocalizedName { get; }

    public static readonly TrackType Steel = new("Steel", "Steel tracks", "Stålband");
    public static readonly TrackType Rubber = new("Rubber", "Rubber tracks", "Gummiband");
    public static readonly TrackType Polyurethane = new("Polyurethane", "Polyurethane tracks", "Polyuretanband");
    public static readonly TrackType RubberPad = new("Rubber pad", "Steel tracks with rubber pads", "Stålband med gummiplattor");
    public static readonly TrackType HalfTrack = new("Half-track", "Half-track", "Halvband");
    public static readonly TrackType Composite = new("Composite", "Composite tracks", "Kompositband");

    /// <summary>All predefined track types.</summary>
    public static IReadOnlyList<TrackType> All { get; } =
    [
        Steel, Rubber, Polyurethane, RubberPad, HalfTrack, Composite
    ];

    private TrackType(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    public static bool TryParse(string? input, out TrackType? result)
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

    public static TrackType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid track type.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Stålband</c> (Swedish) or
    /// <c>Steel tracks</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical English value, e.g. <c>Steel</c>, <c>Rubber</c>, <c>Half-track</c>.
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

    /// <summary>Returns the canonical English value, e.g. <c>Steel</c>.</summary>
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

    private static void AddKey(Dictionary<string, TrackType> d, TrackType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, TrackType> BuildLookup()
    {
        var d = new Dictionary<string, TrackType>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in All)
        {
            AddKey(d, t, t.Value);
            AddKey(d, t, t.EnglishName);
            AddKey(d, t, t.LocalizedName);
        }

        AddKey(d, Steel, "Steel track");
        AddKey(d, Steel, "Stål");
        AddKey(d, Rubber, "Rubber track");
        AddKey(d, Rubber, "Gummi");
        AddKey(d, Rubber, "Gummibandsutrustad");
        AddKey(d, Polyurethane, "PU");
        AddKey(d, Polyurethane, "Polyuretan");
        AddKey(d, RubberPad, "Pad");
        AddKey(d, RubberPad, "Steel with rubber pads");
        AddKey(d, RubberPad, "Stål med gummiplattor");
        AddKey(d, HalfTrack, "Halftrack");
        AddKey(d, HalfTrack, "Halvbandvagn");
        AddKey(d, Composite, "Kompositmaterial");

        return d;
    }

    public static bool operator ==(TrackType? a, TrackType? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(TrackType? a, TrackType? b) => !(a == b);

    public bool Equals(TrackType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is TrackType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(TrackType? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(TrackType a, TrackType b) => a.CompareTo(b) < 0;
    public static bool operator >(TrackType a, TrackType b) => a.CompareTo(b) > 0;
    public static bool operator <=(TrackType a, TrackType b) => a.CompareTo(b) <= 0;
    public static bool operator >=(TrackType a, TrackType b) => a.CompareTo(b) >= 0;
}
