using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Swedish vehicle registration status (<c>fordonsstatus</c>) from Transportstyrelsen.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — fordonsstatus</description></item>
/// </list>
/// </remarks>
public sealed class SwedishVehicleStatus : IEquatable<SwedishVehicleStatus>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Vehicle Status", "Fordonsstatus", "📋", ["https://www.transportstyrelsen.se/"]);

    private static readonly Lazy<Dictionary<string, SwedishVehicleStatus>> Lookup = new(BuildLookup);
    private static readonly Regex WhitespaceRun = new(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Canonical status code, e.g. <c>ITRAFIK</c>.</summary>
    public string Value { get; }

    /// <summary>Status code from Transportstyrelsen, same as <see cref="Value"/>.</summary>
    public string Code => Value;

    /// <summary>English display name, e.g. <c>In service</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>I trafik</c>.</summary>
    public string LocalizedName { get; }

    public static readonly SwedishVehicleStatus InService = new("ITRAFIK", "In service", "I trafik");
    public static readonly SwedishVehicleStatus Deregistered = new("AVST", "Deregistered", "Avställd");
    public static readonly SwedishVehicleStatus Unregistered = new("AVREG", "Unregistered", "Avregistrerad");
    public static readonly SwedishVehicleStatus Stolen = new("STULEN", "Stolen", "Stulen");
    public static readonly SwedishVehicleStatus Exported = new("EXPORT", "Exported", "Exporterad");
    public static readonly SwedishVehicleStatus Scrapped = new("SKROT", "Scrapped", "Skrotad");
    public static readonly SwedishVehicleStatus ReportedStolen = new("ANMSTULEN", "Reported stolen", "Anmäld stulen");

    /// <summary>All predefined vehicle statuses.</summary>
    public static IReadOnlyList<SwedishVehicleStatus> All { get; } =
    [
        InService, Deregistered, Unregistered, Stolen, Exported, Scrapped, ReportedStolen
    ];

    private SwedishVehicleStatus(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    /// <summary>
    /// Attempts to parse a vehicle status code, Swedish name, or English name (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out SwedishVehicleStatus? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var key = NormalizeLookupKey(InputSanitization.SanitizeInput(input!));
        return Lookup.Value.TryGetValue(key, out result);
    }

    public static SwedishVehicleStatus Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid Swedish vehicle status.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the locale-dependent display name, e.g. <c>I trafik</c> or <c>In service</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.ToString();
        if (fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input)) return input!.Trim();
        return null;
    }

    /// <summary>
    /// Returns the canonical status code, e.g. <c>ITRAFIK</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input
    /// (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already equals its canonical code.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical status code, e.g. <c>ITRAFIK</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the locale-dependent display name, e.g. <c>I trafik</c> or <c>In service</c>.</summary>
    public override string ToString() => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    public static bool operator ==(SwedishVehicleStatus? a, SwedishVehicleStatus? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishVehicleStatus? a, SwedishVehicleStatus? b) => !(a == b);
    public bool Equals(SwedishVehicleStatus? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is SwedishVehicleStatus other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("ö", "o").Replace("ä", "a").Replace("å", "a").Replace("é", "e");
        folded = folded.Replace('_', ' ');
        return WhitespaceRun.Replace(folded, " ");
    }

    private static void AddKey(Dictionary<string, SwedishVehicleStatus> d, SwedishVehicleStatus value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d[k] = value;
    }

    private static Dictionary<string, SwedishVehicleStatus> BuildLookup()
    {
        var d = new Dictionary<string, SwedishVehicleStatus>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in All)
        {
            AddKey(d, e, e.Value);
            AddKey(d, e, e.EnglishName);
            AddKey(d, e, e.LocalizedName);
        }

        AddKey(d, InService, "I_TRAFIK");
        AddKey(d, InService, "I TRAFIK");
        AddKey(d, InService, "Påställd");
        AddKey(d, InService, "PASTÄLLD");
        AddKey(d, InService, "Active");
        AddKey(d, InService, "Registered");
        AddKey(d, InService, "In traffic");

        AddKey(d, Deregistered, "AVSTALLD");
        AddKey(d, Deregistered, "AVSTÄLLD");
        AddKey(d, Deregistered, "Off road");
        AddKey(d, Deregistered, "Off-road");
        AddKey(d, Deregistered, "Avställning");

        AddKey(d, Unregistered, "AVREGISTRERAD");
        AddKey(d, Unregistered, "Permanently deregistered");

        AddKey(d, Exported, "EXPORTERAD");

        AddKey(d, Scrapped, "SKROTAD");
        AddKey(d, Scrapped, "Skrotning");

        AddKey(d, ReportedStolen, "ANMÄLD STULEN");

        return d;
    }
}
