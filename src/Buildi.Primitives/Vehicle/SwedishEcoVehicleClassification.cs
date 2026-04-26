using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A Swedish eco-vehicle classification (<c>miljöbilsklassning</c>) used by
/// Transportstyrelsen and the Swedish tax legislation, e.g. <c>Miljöbil 2007</c>,
/// <c>Miljöbil 2013</c>, <c>Supermiljöbil</c>, <c>Klimatbonusbil</c>.
/// </summary>
/// <remarks>
/// <para>This is a Swedish-specific classification distinct from the EU
/// <see cref="EuroEmissionClass"/>. Each classification corresponds to a definition
/// in the Swedish vehicle tax / environmental subsidy framework that has applied at
/// some point in time. The classifications have been progressively replaced as
/// emissions limits have tightened and as the subsidy schemes have been reformed.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — fordon och miljö</description></item>
/// <item><description><see href="https://www.skatteverket.se/">Skatteverket</see> — förmånsbeskattning av miljöbilar</description></item>
/// </list>
/// </remarks>
public sealed class SwedishEcoVehicleClassification : IEquatable<SwedishEcoVehicleClassification>, IComparable<SwedishEcoVehicleClassification>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Swedish Eco-Vehicle Classification",
        "Miljöbilsklassning",
        "🌿",
        ["https://www.transportstyrelsen.se/", "https://www.skatteverket.se/"]);

    private static readonly Lazy<Dictionary<string, SwedishEcoVehicleClassification>> Lookup = new(BuildLookup);

    /// <summary>Canonical Swedish label, e.g. <c>Miljöbil 2013</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Eco vehicle 2013</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name; same as <see cref="Value"/>.</summary>
    public string LocalizedName { get; }

    /// <summary>First year (inclusive) the classification applied; <c>null</c> for indefinite ranges.</summary>
    public int? IntroductionYear { get; }

    /// <summary>Last year (inclusive) the classification applied; <c>null</c> while still active.</summary>
    public int? EndYear { get; }

    /// <summary>English description of the rule.</summary>
    public string EnglishDescription { get; }

    /// <summary>Swedish description of the rule.</summary>
    public string LocalizedDescription { get; }

    public static readonly SwedishEcoVehicleClassification Miljobil2007 = new(
        "Miljöbil 2007", "Eco vehicle 2007", 2007, 2012,
        "Definition under the 2007 Swedish eco-vehicle scheme: low CO₂ emissions and/or alternative fuel propulsion.",
        "Miljöbilsdefinition från 2007 års system: låga CO₂-utsläpp och/eller alternativa drivmedel.");

    public static readonly SwedishEcoVehicleClassification Miljobil2013 = new(
        "Miljöbil 2013", "Eco vehicle 2013", 2013, 2017,
        "Definition under the 2013 Swedish eco-vehicle scheme: stricter CO₂ limits depending on weight.",
        "Miljöbilsdefinition från 2013 års system: striktare CO₂-gränser beroende på fordonsvikt.");

    public static readonly SwedishEcoVehicleClassification Supermiljobil = new(
        "Supermiljöbil", "Super eco vehicle", 2012, 2018,
        "Vehicles emitting at most 50 g CO₂/km, eligible for the Swedish super eco-vehicle premium 2012–2018.",
        "Fordon som släpper ut högst 50 g CO₂/km, berättigade till supermiljöbilspremie 2012–2018.");

    public static readonly SwedishEcoVehicleClassification Klimatbonusbil = new(
        "Klimatbonusbil", "Climate bonus vehicle", 2018, 2022,
        "Vehicles eligible for the Swedish climate bonus 2018–2022 based on CO₂ emissions and electric propulsion.",
        "Fordon som varit berättigade till klimatbonus 2018–2022 utifrån CO₂-utsläpp och elektrisk drift.");

    public static readonly SwedishEcoVehicleClassification Bonusbil = new(
        "Bonusbil", "Bonus vehicle", 2018, 2022,
        "Synonym for klimatbonusbil — climate bonus eligible vehicle.",
        "Synonymt med klimatbonusbil — fordon berättigade till klimatbonus.");

    public static readonly SwedishEcoVehicleClassification Elbil = new(
        "Elbil", "Electric vehicle", 2010, null,
        "Battery electric vehicle — included as a Swedish eco-vehicle classification when no Euro stage applies.",
        "Helelektriskt fordon — registreras som miljöbilsklassning när Euroklass inte är tillämplig.");

    /// <summary>All predefined classifications.</summary>
    public static IReadOnlyList<SwedishEcoVehicleClassification> All { get; } =
    [
        Miljobil2007, Miljobil2013, Supermiljobil, Klimatbonusbil, Bonusbil, Elbil
    ];

    private SwedishEcoVehicleClassification(string value, string englishName, int? introductionYear, int? endYear,
        string englishDescription, string localizedDescription)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = value;
        IntroductionYear = introductionYear;
        EndYear = endYear;
        EnglishDescription = englishDescription;
        LocalizedDescription = localizedDescription;
    }

    public static bool TryParse(string? input, out SwedishEcoVehicleClassification? result)
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

    public static SwedishEcoVehicleClassification Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid Swedish eco-vehicle classification.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the canonical Swedish label, e.g. <c>Miljöbil 2013</c>. Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical Swedish label, e.g. <c>Miljöbil 2013</c>. Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical Swedish label, e.g. <c>Miljöbil 2013</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the canonical Swedish label, e.g. <c>Miljöbil 2013</c>.</summary>
    public override string ToString() => Value;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"\s+", " ", RegexOptions.CultureInvariant).Trim();
        return folded;
    }

    private static void AddKey(Dictionary<string, SwedishEcoVehicleClassification> d, SwedishEcoVehicleClassification value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, SwedishEcoVehicleClassification> BuildLookup()
    {
        var d = new Dictionary<string, SwedishEcoVehicleClassification>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in All)
        {
            AddKey(d, c, c.Value);
            AddKey(d, c, c.EnglishName);
        }

        AddKey(d, Miljobil2007, "Miljöbil2007");
        AddKey(d, Miljobil2007, "MB2007");
        AddKey(d, Miljobil2013, "Miljöbil2013");
        AddKey(d, Miljobil2013, "MB2013");
        AddKey(d, Supermiljobil, "Super eco vehicle");
        AddKey(d, Klimatbonusbil, "Climate bonus car");
        AddKey(d, Klimatbonusbil, "Climate bonus vehicle");
        AddKey(d, Klimatbonusbil, "Klimatbonus");
        AddKey(d, Bonusbil, "Bonus car");
        AddKey(d, Elbil, "Electric car");
        AddKey(d, Elbil, "BEV");
        AddKey(d, Elbil, "EV");

        return d;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"\b(?:Miljöbil(?:\s*20\d{2})?|Supermiljöbil|Klimatbonusbil)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for Swedish eco-vehicle classifications such as
    /// <c>Miljöbil 2013</c>, <c>Supermiljöbil</c>, and <c>Klimatbonusbil</c>.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishEcoVehicleClassification>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishEcoVehicleClassification>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var c)) continue;
            results.Add(new TextCandidate<SwedishEcoVehicleClassification>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(SwedishEcoVehicleClassification), TextCandidateCategory.Vehicle,
                c!.ToNormalizedString(), c.ToString(),
                c.ToMaskedString(),
                TextMatchConfidence.Medium,
                c));
        }
        return results;
    }

    public static bool operator ==(SwedishEcoVehicleClassification? a, SwedishEcoVehicleClassification? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishEcoVehicleClassification? a, SwedishEcoVehicleClassification? b) => !(a == b);

    public bool Equals(SwedishEcoVehicleClassification? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SwedishEcoVehicleClassification other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(SwedishEcoVehicleClassification? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(SwedishEcoVehicleClassification a, SwedishEcoVehicleClassification b) => a.CompareTo(b) < 0;
    public static bool operator >(SwedishEcoVehicleClassification a, SwedishEcoVehicleClassification b) => a.CompareTo(b) > 0;
    public static bool operator <=(SwedishEcoVehicleClassification a, SwedishEcoVehicleClassification b) => a.CompareTo(b) <= 0;
    public static bool operator >=(SwedishEcoVehicleClassification a, SwedishEcoVehicleClassification b) => a.CompareTo(b) >= 0;
}
