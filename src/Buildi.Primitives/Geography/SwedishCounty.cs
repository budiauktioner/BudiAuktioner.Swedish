using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Geography;

/// <summary>
/// A Swedish county (<c>län</c>) is one of SCB's 21 official county divisions. This type lets you work with both the official 2-digit county code and the county name, and parse by either code or name.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/">SCB - Counties and municipalities</see></description></item>
/// <item><description><see href="https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/counties-and-municipalities-in-numerical-order/">SCB - Counties and municipalities in numerical order</see></description></item>
/// <item><description><see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625 (coordinate location)</see> — geographic coordinates (county seat / <c>residensstad</c>)</description></item>
/// </list>
/// </remarks>
public sealed class SwedishCounty : IEquatable<SwedishCounty>, IComparable<SwedishCounty>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("County", "Län", "🏛️", ["https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/", "https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/counties-and-municipalities-in-numerical-order/"]);

    private const int MaxInputLength = 100;

    private static readonly Dictionary<string, (string Swedish, string English)> Counties = new(StringComparer.Ordinal)
    {
        ["01"] = ("Stockholms län", "Stockholm County"),
        ["03"] = ("Uppsala län", "Uppsala County"),
        ["04"] = ("Södermanlands län", "Södermanland County"),
        ["05"] = ("Östergötlands län", "Östergötland County"),
        ["06"] = ("Jönköpings län", "Jönköping County"),
        ["07"] = ("Kronobergs län", "Kronoberg County"),
        ["08"] = ("Kalmar län", "Kalmar County"),
        ["09"] = ("Gotlands län", "Gotland County"),
        ["10"] = ("Blekinge län", "Blekinge County"),
        ["12"] = ("Skåne län", "Skåne County"),
        ["13"] = ("Hallands län", "Halland County"),
        ["14"] = ("Västra Götalands län", "Västra Götaland County"),
        ["17"] = ("Värmlands län", "Värmland County"),
        ["18"] = ("Örebro län", "Örebro County"),
        ["19"] = ("Västmanlands län", "Västmanland County"),
        ["20"] = ("Dalarnas län", "Dalarna County"),
        ["21"] = ("Gävleborgs län", "Gävleborg County"),
        ["22"] = ("Västernorrlands län", "Västernorrland County"),
        ["23"] = ("Jämtlands län", "Jämtland County"),
        ["24"] = ("Västerbottens län", "Västerbotten County"),
        ["25"] = ("Norrbottens län", "Norrbotten County"),
    };

    /// <summary>
    /// Approximate geographic coordinates for each county, based on the county seat
    /// (<c>residensstad</c>) coordinates from
    /// <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    private static readonly Dictionary<string, (double Lat, double Lon)> Coordinates = new(StringComparer.Ordinal)
    {
        ["01"] = (59.3275, 18.054719),      // Stockholm
        ["03"] = (59.866667, 17.633333),    // Uppsala
        ["04"] = (58.752778, 17.008611),    // Nyköping
        ["05"] = (58.4, 15.616667),         // Linköping
        ["06"] = (57.783333, 14.2),         // Jönköping
        ["07"] = (56.883333, 14.8),         // Växjö
        ["08"] = (56.666667, 16.366667),    // Kalmar
        ["09"] = (57.615278, 18.280556),    // Gotland (Visby)
        ["10"] = (56.183333, 15.65),        // Karlskrona
        ["12"] = (55.565, 13.018611),       // Malmö
        ["13"] = (56.666667, 12.85),        // Halmstad
        ["14"] = (57.7, 11.933333),         // Göteborg
        ["17"] = (59.383333, 13.533333),    // Karlstad
        ["18"] = (59.273889, 15.213333),    // Örebro
        ["19"] = (59.616667, 16.533333),    // Västerås
        ["20"] = (60.6, 15.633333),         // Falun
        ["21"] = (60.666667, 17.166667),    // Gävle
        ["22"] = (62.633333, 17.933333),    // Härnösand
        ["23"] = (63.183333, 14.666667),    // Östersund
        ["24"] = (63.833333, 20.25),        // Umeå
        ["25"] = (65.584444, 22.153889),    // Luleå
    };

    private static readonly Dictionary<string, SwedishCounty> ByCode = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, SwedishCounty> ByName = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Regex ScanPattern;

    public string Code { get; }
    public string LocalizedName { get; }
    public string EnglishName { get; }

    /// <summary>
    /// The approximate geographic coordinate (WGS 84) of the county seat (<c>residensstad</c>),
    /// e.g. <c>59.3275°N, 18.054719°E</c> for Stockholms län.
    /// Sourced from <see href="https://www.wikidata.org/wiki/Property:P625">Wikidata P625</see>.
    /// </summary>
    public GeoCoordinate Coordinate { get; }

    /// <summary>
    /// The approximate latitude (WGS 84) of the county seat, e.g. <c>59.3275</c> for Stockholms län.
    /// </summary>
    public double Latitude => Coordinate.Latitude;

    /// <summary>
    /// The approximate longitude (WGS 84) of the county seat, e.g. <c>18.054719</c> for Stockholms län.
    /// </summary>
    public double Longitude => Coordinate.Longitude;

    static SwedishCounty()
    {
        foreach (var (code, names) in Counties)
        {
            var (lat, lon) = Coordinates.GetValueOrDefault(code);
            var county = new SwedishCounty(code, names.Swedish, names.English, GeoCoordinate.Create(lat, lon));
            ByCode[code] = county;
            ByName[county.LocalizedName] = county;
            ByName[county.EnglishName] = county;

            if (county.LocalizedName.EndsWith(" län", StringComparison.Ordinal))
            {
                var baseSv = county.LocalizedName[..^4];
                ByName.TryAdd(baseSv, county);
                AddGenitive(ByName, baseSv, county);
            }

            if (county.EnglishName.EndsWith(" County", StringComparison.Ordinal))
            {
                var baseEn = county.EnglishName[..^7];
                ByName.TryAdd(baseEn, county);
                AddGenitive(ByName, baseEn, county);
            }
        }

        var scanNames = ByName.Keys
            .OrderByDescending(n => n.Length)
            .Select(Regex.Escape);
        ScanPattern = new Regex(
            @"\b(?:" + string.Join('|', scanNames) + @")\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    private static void AddGenitive(Dictionary<string, SwedishCounty> dict, string name, SwedishCounty county)
    {
        if (name.EndsWith('s') || name.EndsWith('S')) return;
        dict.TryAdd(name + "s", county);
    }

    private SwedishCounty(string code, string localizedName, string englishName, GeoCoordinate coordinate)
    {
        Code = code;
        LocalizedName = localizedName;
        EnglishName = englishName;
        Coordinate = coordinate;
    }

    /// <summary>
    /// Searches <paramref name="text"/> for substrings that match known Swedish county names
    /// (both Swedish and English forms, e.g. "Stockholms län", "Stockholm County").
    /// Results use <see cref="TextMatchConfidence.Low"/> since county names may appear in
    /// non-geographic contexts. No guarantee is made that a match represents a county reference
    /// in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishCounty>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishCounty>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var county)) continue;
            results.Add(new TextCandidate<SwedishCounty>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishCounty),
                TextCandidateCategory.Geography,
                county!.ToNormalizedString(),
                county.ToString(),
                county.LocalizedName,
                TextMatchConfidence.Low,
                county));
        }
        return results;
    }

    public static bool TryParse(string? input, out SwedishCounty? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length > MaxInputLength) return false;

        var digits = InputSanitization.KeepDigits(trimmed);
        if (digits.Length == 2 && ByCode.TryGetValue(digits, out result))
            return true;

        return ByName.TryGetValue(trimmed, out result);
    }

    public static SwedishCounty Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish county.", nameof(input));

        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the Swedish county name, for example <c>Stockholms län</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var result) ? result!.LocalizedName : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;

    /// <summary>
    /// Returns the normalized county code as 2 digits, for example <c>01</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var result)) return result!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;
    /// <summary>
    /// Returns the normalized county code as 2 digits, for example <c>01</c>.
    /// </summary>
    public string ToNormalizedString() => Code;
    /// <summary>County name in the current display language, for example <c>Stockholms län</c> or <c>Stockholm County</c> depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>
    /// Returns the county in the current display language (Swedish when <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/>
    /// is true, otherwise English), for example <c>Stockholms län</c> or <c>Stockholm County</c>.
    /// </summary>
    public string ToDisplayString() => DisplayName;

    /// <summary>
    /// Returns the English county name, for example <c>Stockholm County</c>.
    /// </summary>
    public string ToEnglishString() => EnglishName;

    /// <summary>
    /// Returns the county name in Swedish (the local language), for example <c>Stockholms län</c>.
    /// </summary>
    public string ToLocalString() => LocalizedName;

    /// <summary>
    /// Returns the county in the current display language, for example <c>Stockholms län</c> or <c>Stockholm County</c>.
    /// </summary>
    public override string ToString() => DisplayName;

    /// <summary>
    /// Calculates the distance between two counties (by county seat) as a <see cref="Length"/>,
    /// e.g. <c>541.3 km</c>.
    /// </summary>
    public static Length Distance(SwedishCounty a, SwedishCounty b) =>
        GeoCoordinate.Distance(a.Coordinate, b.Coordinate);

    /// <summary>
    /// Calculates the distance from a county (by county seat) to a geographic coordinate as a <see cref="Length"/>.
    /// </summary>
    public static Length Distance(SwedishCounty county, GeoCoordinate coordinate) =>
        GeoCoordinate.Distance(county.Coordinate, coordinate);

    /// <summary>
    /// Calculates the distance from this county's seat to the given <paramref name="coordinate"/>
    /// as a <see cref="Length"/>.
    /// </summary>
    public Length DistanceTo(GeoCoordinate coordinate) =>
        GeoCoordinate.Distance(Coordinate, coordinate);

    /// <summary>
    /// Calculates the distance from this county to <paramref name="other"/> as a <see cref="Length"/>.
    /// </summary>
    public Length DistanceTo(SwedishCounty other) =>
        GeoCoordinate.Distance(Coordinate, other.Coordinate);

    public bool Equals(SwedishCounty? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is SwedishCounty other && Equals(other);
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishCounty? a, SwedishCounty? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishCounty? a, SwedishCounty? b) => !(a == b);
    public int CompareTo(SwedishCounty? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishCounty left, SwedishCounty right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishCounty left, SwedishCounty right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishCounty left, SwedishCounty right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishCounty left, SwedishCounty right) => left.CompareTo(right) >= 0;
}
