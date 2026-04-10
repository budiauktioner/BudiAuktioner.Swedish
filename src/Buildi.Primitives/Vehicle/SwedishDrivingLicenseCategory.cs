using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Broad vehicle group associated with a <see cref="SwedishDrivingLicenseCategory"/>.
/// </summary>
public enum SwedishDrivingLicenseVehicleGroup
{
    /// <summary>Unknown or unclassified.</summary>
    Unknown = 0,

    /// <summary>Moped (EU moped class I).</summary>
    Moped,

    /// <summary>Motorcycle (two- or three-wheeled).</summary>
    Motorcycle,

    /// <summary>Passenger car and light lorry.</summary>
    Car,

    /// <summary>Heavy lorry / truck.</summary>
    Truck,

    /// <summary>Bus.</summary>
    Bus
}

/// <summary>
/// A Swedish/EU driving license category (<c>körkortsbehörighet</c>), such as <c>B</c>, <c>C1E</c>,
/// or <c>AM</c>. Each category describes the class of vehicles the holder is entitled to drive,
/// along with metadata like minimum age, vehicle group, trailer eligibility, and validity period.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/en/road/driving-licences/im-going-to-take-my-driving-licence/driving-licence-categories/">Transportstyrelsen — Driving licence categories</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Driving_licence_in_Sweden">Wikipedia — Driving licence in Sweden</see></description></item>
/// <item><description>EU Directive 2006/126/EC — harmonized EU driving licence categories</description></item>
/// </list>
/// </remarks>
public sealed class SwedishDrivingLicenseCategory : IEquatable<SwedishDrivingLicenseCategory>, IComparable<SwedishDrivingLicenseCategory>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Driving License Category", "Körkortsbehörighet", "🪪", ["https://www.transportstyrelsen.se/en/road/driving-licences/im-going-to-take-my-driving-licence/driving-licence-categories/", "https://en.wikipedia.org/wiki/Driving_licence_in_Sweden"]);

    private static readonly Lazy<Dictionary<string, SwedishDrivingLicenseCategory>> Lookup = new(BuildLookup);

    /// <summary>Canonical category code, e.g. <c>B</c>, <c>C1E</c>, <c>AM</c>.</summary>
    public string Code { get; }

    /// <summary>Swedish name, e.g. <c>Personbil och lätt lastbil</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>English name, e.g. <c>Car and light lorry</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Short description of what vehicles the holder may drive.</summary>
    public string Description { get; }

    /// <summary>Standard minimum age requirement.</summary>
    public int MinimumAge { get; }

    /// <summary>Broad vehicle group this category belongs to.</summary>
    public SwedishDrivingLicenseVehicleGroup VehicleGroup { get; }

    /// <summary><see langword="true"/> for trailer categories (BE, C1E, CE, D1E, DE).</summary>
    public bool IsTrailerCategory { get; }

    /// <summary>License validity period in years (10 for AM/A/B categories, 5 for C/D categories).</summary>
    public int ValidityYears { get; }

    /// <summary>Canonical string form; same as <see cref="Code"/>.</summary>
    public string Value => Code;

    // --- Static instances ---

    /// <summary>Class I moped (EU moped). Minimum age 15.</summary>
    public static readonly SwedishDrivingLicenseCategory AM = new("AM", "Moped klass I", "Class I moped",
        "Class I moped (EU moped), class II moped, tractor type a, class II construction equipment",
        15, SwedishDrivingLicenseVehicleGroup.Moped, false, 10);

    /// <summary>Light motorcycle, max 125 cm³ and 11 kW. Minimum age 16.</summary>
    public static readonly SwedishDrivingLicenseCategory A1 = new("A1", "Lätt motorcykel", "Light motorcycle",
        "Two-wheeled motorcycle max 125 cm³ and 11 kW, or three-wheeled motorcycle max 15 kW",
        16, SwedishDrivingLicenseVehicleGroup.Motorcycle, false, 10);

    /// <summary>Medium motorcycle, max 35 kW. Minimum age 18.</summary>
    public static readonly SwedishDrivingLicenseCategory A2 = new("A2", "Mellanstor motorcykel", "Medium motorcycle",
        "Two-wheeled motorcycle max 35 kW and 0.2 kW/kg, or three-wheeled motorcycle max 15 kW",
        18, SwedishDrivingLicenseVehicleGroup.Motorcycle, false, 10);

    /// <summary>Heavy motorcycle, unrestricted. Minimum age 24 (or 20 after holding A2 for two years).</summary>
    public static readonly SwedishDrivingLicenseCategory A = new("A", "Tung motorcykel", "Heavy motorcycle",
        "Two- and three-wheeled heavy motorcycles of any power",
        24, SwedishDrivingLicenseVehicleGroup.Motorcycle, false, 10);

    /// <summary>Passenger car and light lorry up to 3,500 kg. Minimum age 18.</summary>
    public static readonly SwedishDrivingLicenseCategory B = new("B", "Personbil och lätt lastbil", "Car and light lorry",
        "Passenger car or light lorry max 3,500 kg with a light trailer, off-road vehicles, three- and four-wheeled motorcycles",
        18, SwedishDrivingLicenseVehicleGroup.Car, false, 10);

    /// <summary>Extended B — car with trailer up to combined 4,250 kg. Minimum age 18.</summary>
    public static readonly SwedishDrivingLicenseCategory B96 = new("B96", "Personbil utökad", "Car extended",
        "Vehicle combinations where the combined car and trailer mass exceeds 3,500 kg but not 4,250 kg",
        18, SwedishDrivingLicenseVehicleGroup.Car, false, 10);

    /// <summary>Car with heavy trailer. Minimum age 18.</summary>
    public static readonly SwedishDrivingLicenseCategory BE = new("BE", "Personbil med tungt släp", "Car with heavy trailer",
        "Car or light lorry with one or more trailers, combined mass may exceed 3,500 kg",
        18, SwedishDrivingLicenseVehicleGroup.Car, true, 10);

    /// <summary>Medium heavy lorry up to 7,500 kg. Minimum age 18.</summary>
    public static readonly SwedishDrivingLicenseCategory C1 = new("C1", "Medeltung lastbil", "Medium heavy lorry",
        "Heavy lorry max 7,500 kg or car over 3,500 kg but not exceeding 7,500 kg, with a light trailer",
        18, SwedishDrivingLicenseVehicleGroup.Truck, false, 5);

    /// <summary>Medium heavy lorry with heavy trailer, combined max 12,000 kg. Minimum age 18.</summary>
    public static readonly SwedishDrivingLicenseCategory C1E = new("C1E", "Medeltung lastbil med tungt släp", "Medium heavy lorry with heavy trailer",
        "C1 or B vehicle with one or more trailers, combined mass max 12,000 kg",
        18, SwedishDrivingLicenseVehicleGroup.Truck, true, 5);

    /// <summary>Heavy lorry over 3,500 kg. Minimum age 21.</summary>
    public static readonly SwedishDrivingLicenseCategory C = new("C", "Tung lastbil", "Heavy lorry",
        "Heavy lorry over 3,500 kg with a light trailer max 750 kg",
        21, SwedishDrivingLicenseVehicleGroup.Truck, false, 5);

    /// <summary>Heavy lorry with heavy trailer, unrestricted. Minimum age 21.</summary>
    public static readonly SwedishDrivingLicenseCategory CE = new("CE", "Tung lastbil med tungt släp", "Heavy lorry with heavy trailer",
        "Heavy lorry with one or more trailers of any weight",
        21, SwedishDrivingLicenseVehicleGroup.Truck, true, 5);

    /// <summary>Medium bus, max 16 passengers. Minimum age 21.</summary>
    public static readonly SwedishDrivingLicenseCategory D1 = new("D1", "Mellanstor buss", "Medium bus",
        "Bus max 16 passengers (plus driver), max 8 metres, with a light trailer max 750 kg",
        21, SwedishDrivingLicenseVehicleGroup.Bus, false, 5);

    /// <summary>Medium bus with heavy trailer. Minimum age 21.</summary>
    public static readonly SwedishDrivingLicenseCategory D1E = new("D1E", "Mellanstor buss med tungt släp", "Medium bus with heavy trailer",
        "D1 bus with one or more trailers of any weight",
        21, SwedishDrivingLicenseVehicleGroup.Bus, true, 5);

    /// <summary>Bus of any size. Minimum age 24.</summary>
    public static readonly SwedishDrivingLicenseCategory D = new("D", "Buss", "Bus",
        "Bus of any length and passenger capacity, with a light trailer max 750 kg",
        24, SwedishDrivingLicenseVehicleGroup.Bus, false, 5);

    /// <summary>Bus with heavy trailer, unrestricted. Minimum age 24.</summary>
    public static readonly SwedishDrivingLicenseCategory DE = new("DE", "Buss med tungt släp", "Bus with heavy trailer",
        "Bus with one or more trailers of any weight",
        24, SwedishDrivingLicenseVehicleGroup.Bus, true, 5);

    /// <summary>All 15 categories in order: AM, A1, A2, A, B, B96, BE, C1, C1E, C, CE, D1, D1E, D, DE.</summary>
    public static IReadOnlyList<SwedishDrivingLicenseCategory> All { get; } =
    [
        AM, A1, A2, A, B, B96, BE, C1, C1E, C, CE, D1, D1E, D, DE
    ];

    private SwedishDrivingLicenseCategory(
        string code, string localizedName, string englishName, string description,
        int minimumAge, SwedishDrivingLicenseVehicleGroup vehicleGroup, bool isTrailerCategory, int validityYears)
    {
        Code = code;
        LocalizedName = localizedName;
        EnglishName = englishName;
        Description = description;
        MinimumAge = minimumAge;
        VehicleGroup = vehicleGroup;
        IsTrailerCategory = isTrailerCategory;
        ValidityYears = validityYears;
    }

    /// <summary>
    /// Attempts to parse a driving license category code or Swedish/English name (case-insensitive).
    /// Accepts codes like <c>B</c>, <c>C1E</c>, <c>AM</c>, as well as Swedish names like
    /// <c>Personbil</c> or aliases like <c>MC</c>.
    /// </summary>
    public static bool TryParse(string? input, out SwedishDrivingLicenseCategory? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var fromDict))
        {
            result = fromDict;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses a driving license category code or name. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static SwedishDrivingLicenseCategory Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid Swedish driving license category.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid driving license category.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the Swedish display name, e.g. <c>Personbil och lätt lastbil</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var e) ? e!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical category code, e.g. <c>B</c>, <c>C1E</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical code.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical category code, e.g. <c>B</c>.</summary>
    public string ToNormalizedString() => Code;

    /// <summary>Category name in the current display language depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the code followed by the display name, e.g. <c>B — Personbil och lätt lastbil</c> or <c>B — Car and light lorry</c>.</summary>
    public override string ToString() => $"{Code} — {DisplayName}";

    /// <summary>Returns the category in the current display language, e.g. <c>Personbil och lätt lastbil</c> or <c>Car and light lorry</c>.</summary>
    public string ToDisplayString() => DisplayName;

    /// <summary>Returns the English name, e.g. <c>Car and light lorry</c>.</summary>
    public string ToEnglishString() => EnglishName;

    /// <summary>Returns the category name in Swedish (the local language), e.g. <c>Personbil och lätt lastbil</c>.</summary>
    public string ToLocalString() => LocalizedName;

    private static readonly Regex ScanPattern = new(
        @"\b(?:AM|A[12]?|B(?:96|E)?|C[1]?E?|D[1]?E?)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Scans unstructured text for substrings that look like driving license category codes
    /// (e.g. <c>B</c>, <c>C1E</c>, <c>AM</c>).
    /// Results are heuristic-based candidates and may include false positives, especially for
    /// short codes like <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c>.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishDrivingLicenseCategory>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishDrivingLicenseCategory>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var cat)) continue;
            var category = cat!;
            results.Add(new TextCandidate<SwedishDrivingLicenseCategory>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(SwedishDrivingLicenseCategory),
                TextCandidateCategory.Vehicle,
                category.ToNormalizedString(),
                category.ToString(),
                category.Code,
                TextMatchConfidence.Low,
                category));
        }

        return results;
    }

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("ö", "o").Replace("ä", "a");
        folded = Regex.Replace(folded, @"\s+", " ", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, SwedishDrivingLicenseCategory> d, SwedishDrivingLicenseCategory value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, SwedishDrivingLicenseCategory> BuildLookup()
    {
        var d = new Dictionary<string, SwedishDrivingLicenseCategory>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in All)
        {
            AddKey(d, c, c.Code);
            AddKey(d, c, c.LocalizedName);
            AddKey(d, c, c.EnglishName);
        }

        AddKey(d, AM, "Moped");
        AddKey(d, AM, "EU-moped");
        AddKey(d, AM, "EU moped");
        AddKey(d, A1, "Latt motorcykel");
        AddKey(d, A2, "Mellanstor motorcykel");
        AddKey(d, A, "Tung motorcykel");
        AddKey(d, A, "MC");
        AddKey(d, A, "Motorcykel");
        AddKey(d, B, "Personbil");
        AddKey(d, B, "Bil");
        AddKey(d, B96, "B utokad");
        AddKey(d, B96, "B extended");
        AddKey(d, BE, "Bil med tungt slap");
        AddKey(d, C1, "Medeltung lastbil");
        AddKey(d, C, "Tung lastbil");
        AddKey(d, C, "Lastbil");
        AddKey(d, D1, "Mellanstor buss");
        AddKey(d, D, "Buss");

        return d;
    }

    public static bool operator ==(SwedishDrivingLicenseCategory? a, SwedishDrivingLicenseCategory? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(SwedishDrivingLicenseCategory? a, SwedishDrivingLicenseCategory? b) => !(a == b);
    public int CompareTo(SwedishDrivingLicenseCategory? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishDrivingLicenseCategory left, SwedishDrivingLicenseCategory right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishDrivingLicenseCategory left, SwedishDrivingLicenseCategory right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishDrivingLicenseCategory left, SwedishDrivingLicenseCategory right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishDrivingLicenseCategory left, SwedishDrivingLicenseCategory right) => left.CompareTo(right) >= 0;

    public bool Equals(SwedishDrivingLicenseCategory? other) =>
        other is not null && string.Equals(Code, other.Code, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SwedishDrivingLicenseCategory other && Equals(other);

    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
}
