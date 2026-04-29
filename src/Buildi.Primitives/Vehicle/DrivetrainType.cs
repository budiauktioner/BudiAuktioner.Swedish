using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Drivetrain layout (<c>drivning</c>) of a road vehicle, e.g. <c>AWD</c> (all-wheel drive,
/// <c>fyrhjulsdrift</c>), <c>FWD</c> (front-wheel drive, <c>framhjulsdrift</c>), or
/// <c>RWD</c> (rear-wheel drive, <c>bakhjulsdrift</c>).
/// </summary>
/// <remarks>
/// <para>Captures which axle(s) receive power from the engine. Each entry exposes the canonical
/// short code, English/Swedish display names, and <see cref="DrivenAxleCount"/>.
/// Recognises Swedish forms (<c>fyrhjulsdriven</c>, <c>framhjulsdrift</c>, <c>bakhjulsdrift</c>),
/// part-time off-road notation (<c>4WD</c>, <c>4x4</c>), and common manufacturer marketing names
/// (<c>4Motion</c>, <c>4Matic</c>, <c>quattro</c>, <c>xDrive</c>) — all of which are accepted as
/// aliases for <see cref="Awd"/>.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Drive_wheel">Wikipedia — Drive wheel</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/All-wheel_drive">Wikipedia — All-wheel drive</see></description></item>
/// </list>
/// </remarks>
public sealed class DrivetrainType : IEquatable<DrivetrainType>, IComparable<DrivetrainType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Drivetrain Type",
        "Drivning",
        "🚗",
        ["https://en.wikipedia.org/wiki/Drive_wheel", "https://en.wikipedia.org/wiki/All-wheel_drive"]);

    private static readonly Lazy<Dictionary<string, DrivetrainType>> Lookup = new(BuildLookup);

    private readonly int _order;

    /// <summary>Canonical short code, e.g. <c>AWD</c>, <c>FWD</c>, <c>RWD</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>All-wheel drive</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Fyrhjulsdrift</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Number of driven axles: 1 for FWD/RWD, 2 for AWD.</summary>
    public int DrivenAxleCount { get; }

    public static readonly DrivetrainType Awd = new("AWD", "All-wheel drive",   "Fyrhjulsdrift",     drivenAxleCount: 2, 0);
    public static readonly DrivetrainType Fwd = new("FWD", "Front-wheel drive", "Framhjulsdrift",    drivenAxleCount: 1, 1);
    public static readonly DrivetrainType Rwd = new("RWD", "Rear-wheel drive",  "Bakhjulsdrift",     drivenAxleCount: 1, 2);

    /// <summary>All predefined drive types.</summary>
    public static IReadOnlyList<DrivetrainType> All { get; } =
    [
        Awd, Fwd, Rwd
    ];

    private DrivetrainType(string value, string englishName, string localizedName, int drivenAxleCount, int order)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        DrivenAxleCount = drivenAxleCount;
        _order = order;
    }

    /// <summary>
    /// Attempts to parse a drive type from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out DrivetrainType? result)
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
    /// Parses a drive type. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static DrivetrainType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid drive type.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized drive type.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Fyrhjulsdrift</c> (Swedish)
    /// or <c>All-wheel drive</c> (English). Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical short code, e.g. <c>AWD</c>, <c>FWD</c>, <c>RWD</c>.
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

    /// <summary>Returns the canonical short code, e.g. <c>AWD</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_/]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, DrivetrainType> d, DrivetrainType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, DrivetrainType> BuildLookup()
    {
        var d = new Dictionary<string, DrivetrainType>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in All)
        {
            AddKey(d, t, t.Value);
            AddKey(d, t, t.EnglishName);
            AddKey(d, t, t.LocalizedName);
        }

        AddKey(d, Awd, "All wheel drive");
        AddKey(d, Awd, "Allwheel drive");
        AddKey(d, Awd, "AWD-system");
        AddKey(d, Awd, "Fyrhjulsdriven");
        AddKey(d, Awd, "Fyrhjulsdrivet");
        AddKey(d, Awd, "Fyrhjul");
        AddKey(d, Awd, "Fyra hjul");
        AddKey(d, Awd, "4WD");
        AddKey(d, Awd, "4-WD");
        AddKey(d, Awd, "4x4");
        AddKey(d, Awd, "4 x 4");
        AddKey(d, Awd, "Four wheel drive");
        AddKey(d, Awd, "Four-wheel drive");
        AddKey(d, Awd, "4Motion");
        AddKey(d, Awd, "4-Motion");
        AddKey(d, Awd, "4Matic");
        AddKey(d, Awd, "4-Matic");
        AddKey(d, Awd, "quattro");
        AddKey(d, Awd, "xDrive");
        AddKey(d, Awd, "x-Drive");
        AddKey(d, Awd, "S-AWC");
        AddKey(d, Awd, "Symmetrical AWD");
        AddKey(d, Awd, "Twincharger");

        AddKey(d, Fwd, "Front wheel drive");
        AddKey(d, Fwd, "Framhjulsdriven");
        AddKey(d, Fwd, "Framhjulsdrivet");
        AddKey(d, Fwd, "Framhjul");
        AddKey(d, Fwd, "Framdrift");
        AddKey(d, Fwd, "2WD front");
        AddKey(d, Fwd, "2WD-front");
        AddKey(d, Fwd, "Front drive");

        AddKey(d, Rwd, "Rear wheel drive");
        AddKey(d, Rwd, "Bakhjulsdriven");
        AddKey(d, Rwd, "Bakhjulsdrivet");
        AddKey(d, Rwd, "Bakhjul");
        AddKey(d, Rwd, "Bakdrift");
        AddKey(d, Rwd, "2WD rear");
        AddKey(d, Rwd, "2WD-rear");
        AddKey(d, Rwd, "Rear drive");

        return d;
    }

    public static bool operator ==(DrivetrainType? a, DrivetrainType? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(DrivetrainType? a, DrivetrainType? b) => !(a == b);

    public bool Equals(DrivetrainType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is DrivetrainType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(DrivetrainType? other) =>
        other is null ? 1 : _order.CompareTo(other._order);

    public static bool operator <(DrivetrainType a, DrivetrainType b) => a.CompareTo(b) < 0;
    public static bool operator >(DrivetrainType a, DrivetrainType b) => a.CompareTo(b) > 0;
    public static bool operator <=(DrivetrainType a, DrivetrainType b) => a.CompareTo(b) <= 0;
    public static bool operator >=(DrivetrainType a, DrivetrainType b) => a.CompareTo(b) >= 0;
}
