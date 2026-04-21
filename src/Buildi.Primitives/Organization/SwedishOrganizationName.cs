using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Organization;

/// <summary>
/// The registered trade name (<c>organisationsnamn</c> / <c>företagsnamn</c>) of a legal entity.
/// Swedish company names are registered with Bolagsverket. Validation accepts names of 2–200
/// characters. Parsing extracts metadata such as whether the name contains Swedish organizational
/// indicators and an inferred <see cref="SwedishOrganizationType"/> from the name alone.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://bolagsverket.se">Bolagsverket</see> — Swedish Companies Registration Office</description></item>
/// </list>
/// </remarks>
public sealed class SwedishOrganizationName : IEquatable<SwedishOrganizationName>, IComparable<SwedishOrganizationName>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Organization Name", "Organisationsnamn", "🏷️", ["https://bolagsverket.se"]);

    private const int MaxInputLength = 300;

    // Scope: strict Swedish company-name characters per Bolagsverket conventions (Unicode
    // letters, digits, whitespace, and the punctuation - ' & . , / : ( ) +). For broader
    // multi-jurisdictional names that may contain pipes (LEGAL||TRADE) or double-quoted
    // distinctive parts (SIA "Example LV"), use EuOrganizationName instead.
    private static readonly Regex CompanyNamePattern = new(@"^[\p{L}\d\s\-\'\&\.,/:()\+]+$", RegexOptions.Compiled);

    private static readonly string[] OrgSuffixTokens =
        ["AB", "HB", "KB", "BRF", "HSB", "EF"];

    private static readonly string[] OrgSuffixPhrases =
        [" EK.FÖR.", " EK FÖR", " EKONOMISK FÖRENING", " IDEELL FÖRENING",
         " SAMFÄLLIGHETSFÖRENING", " RIKSFÖRBUND"];

    private static readonly string[] OrgPrefixes =
        ["STIFTELSEN ", "BRF ", "HSB "];

    private static readonly string[] OrgKeywords =
        [" KOMMUN", " REGION", " LANDSTING", " MYNDIGHET", " STIFTELSE", " FÖRENING"];

    private static readonly HashSet<string> KnownAbbreviations =
        new(["AB", "HB", "KB", "BRF", "HSB", "EF"], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns <see langword="true"/> if the name matches a well-known Swedish government agency
    /// (<c>myndighet</c>). This is a curated list of the most commonly encountered agencies.
    /// The check is case-insensitive.
    /// </summary>
    public static bool IsKnownGovernmentAgency(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return KnownGovernmentAgencies.Contains(InputSanitization.CollapseWhitespace(name).ToUpperInvariant());
    }

    private static readonly HashSet<string> KnownGovernmentAgencies = new(StringComparer.OrdinalIgnoreCase)
    {
        // Finansdepartementet
        "SKATTEVERKET",
        "TULLVERKET",
        "KRONOFOGDEN",
        "STATISTISKA CENTRALBYRÅN",

        // Justitiedepartementet
        "KRIMINALVÅRDEN",
        "MIGRATIONSVERKET",
        "DOMSTOLSVERKET",
        "BROTTSFÖREBYGGANDE RÅDET",

        // Socialdepartementet
        "FÖRSÄKRINGSKASSAN",
        "SOCIALSTYRELSEN",

        // Utbildningsdepartementet
        "SKOLVERKET",
        "CENTRALA STUDIESTÖDSNÄMNDEN",
        "UNIVERSITETS- OCH HÖGSKOLERÅDET",

        // Näringsdepartementet
        "BOLAGSVERKET",
        "TRAFIKVERKET",
        "TRANSPORTSTYRELSEN",
        "TILLVÄXTVERKET",
        "PATENT- OCH REGISTRERINGSVERKET",

        // Arbetsmarknadsdepartementet
        "ARBETSFÖRMEDLINGEN",
        "ARBETSMILJÖVERKET",

        // Miljödepartementet
        "NATURVÅRDSVERKET",

        // Försvarsdepartementet
        "FÖRSVARSMAKTEN",

        // Infrastrukturdepartementet
        "POST- OCH TELESTYRELSEN",
        "SJÖFARTSVERKET",

        // Finansiella/ekonomiska
        "RIKSBANKEN",
        "RIKSREVISIONEN",
        "RIKSGÄLDEN",
        "RIKSGÄLDSKONTORET",
        "EKONOMISTYRNINGSVERKET",

        // Jordbruk & livsmedel
        "JORDBRUKSVERKET",
        "LIVSMEDELSVERKET",

        // Byggnation & fastighet
        "LANTMÄTERIET",
        "BOVERKET",

        // Övriga centrala myndigheter
        "KONSUMENTVERKET",
        "RIKSARKIVET",
        "KUNGLIGA BIBLIOTEKET",
        "KEMIKALIEINSPEKTIONEN",
        "INTEGRITETSSKYDDSMYNDIGHETEN",
        "ELSÄKERHETSVERKET",
        "HAVS- OCH VATTENMYNDIGHETEN",
        "SKOGSSTYRELSEN",
        "VALMYNDIGHETEN",
        "LÄNSSTYRELSEN",
        "DATAINSPEKTIONEN",
        "KONKURRENSVERKET",
        "UPPHANDLINGSMYNDIGHETEN",
    };

    private static readonly CultureInfo SvCulture = CultureInfo.GetCultureInfo("sv-SE");

    /// <summary>The whitespace-collapsed name as provided, e.g. <c>VOLVO AB</c> or <c>Volvo AB</c>.</summary>
    public string Value { get; }

    /// <summary>
    /// The name with normalized casing. ALL CAPS and all lowercase input is auto-capitalized
    /// while preserving known abbreviations (AB, HB, etc.) in uppercase. Mixed-case input is
    /// preserved as-is. Example: <c>VOLVO AB</c> → <c>Volvo AB</c>.
    /// </summary>
    public string CasingNormalizedValue { get; }

    /// <summary>
    /// Whether the name contains Swedish organizational indicators such as corporate suffixes
    /// (<c>AB</c>, <c>HB</c>), phrases (<c>Ekonomisk förening</c>), prefixes (<c>Stiftelsen</c>),
    /// or keywords (<c>kommun</c>, <c>stiftelse</c>).
    /// </summary>
    public bool HasOrganizationIndicators { get; }

    /// <summary>
    /// The best-guess <see cref="SwedishOrganizationType"/> inferred from the name alone, without an
    /// organization number. Returns <see cref="SwedishOrganizationType.Unknown"/> when no indicators
    /// are detected.
    /// </summary>
    public SwedishOrganizationType InferredSwedishOrganizationType { get; }

    private SwedishOrganizationName(string value, string casingNormalized, bool hasIndicators, SwedishOrganizationType inferredType)
    {
        Value = value;
        CasingNormalizedValue = casingNormalized;
        HasOrganizationIndicators = hasIndicators;
        InferredSwedishOrganizationType = inferredType;
    }

    public static bool TryParse(string? input, out SwedishOrganizationName? result)
    {
        result = null;
        var normalized = InputSanitization.CollapseWhitespace(input);
        if (normalized.Length > MaxInputLength) return false;
        if (!Validate(normalized)) return false;

        var forInference = StripPublSuffix(normalized);
        var hasIndicators = DetectOrganizationIndicators(forInference);
        var inferredType = InferSwedishOrganizationTypeFromName(forInference);
        var casingNormalized = NormalizeCasing(normalized);

        result = new SwedishOrganizationName(normalized, casingNormalized, hasIndicators, inferredType);
        return true;
    }

    public static SwedishOrganizationName Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid organization name.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the best-guess <see cref="SwedishOrganizationType"/> inferred from the name alone.
    /// This is the same value exposed as <see cref="InferredSwedishOrganizationType"/> on the parsed
    /// instance, but can be called without parsing. Returns <see cref="SwedishOrganizationType.Unknown"/>
    /// when no indicators are detected.
    /// </summary>
    public static SwedishOrganizationType InferSwedishOrganizationType(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return SwedishOrganizationType.Unknown;
        return InferSwedishOrganizationTypeFromName(StripPublSuffix(InputSanitization.CollapseWhitespace(name)));
    }

    /// <summary>
    /// Normalizes the casing of an organization name. When all letters share the same case
    /// (all upper or all lower), auto-capitalizes each word while preserving known Swedish
    /// corporate abbreviations (<c>AB</c>, <c>HB</c>, <c>KB</c>, <c>BRF</c>, <c>HSB</c>,
    /// <c>EF</c>) in uppercase. Mixed-case input is preserved as-is.
    /// </summary>
    public static string NormalizeCasing(string name)
    {
        var letters = name.Where(char.IsLetter).ToArray();
        if (letters.Length == 0) return name;

        var allSameCase = letters.All(char.IsLower) || letters.All(char.IsUpper);
        if (!allSameCase) return name;

        var words = name.Split(' ');
        for (var i = 0; i < words.Length; i++)
        {
            if (KnownAbbreviations.Contains(words[i]))
                words[i] = words[i].ToUpperInvariant();
            else
                words[i] = CapitalizeWord(words[i]);
        }
        return string.Join(" ", words);
    }

    /// <summary>
    /// Returns the organization name with collapsed whitespace, for example <c>Budi Auktioner AB</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized organization name with collapsed whitespace, for example <c>Budi Auktioner AB</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized organization name with collapsed whitespace, for example <c>Budi Auktioner AB</c>.
    /// </summary>
    public string ToNormalizedString() => Value;

    /// <summary>
    /// Returns the organization name with collapsed whitespace, for example <c>Budi Auktioner AB</c>.
    /// </summary>
    public override string ToString() => Value;

    // --- Internal / private ---

    private static bool DetectOrganizationIndicators(string collapsed)
    {
        var upper = collapsed.ToUpperInvariant();
        if (upper.Length == 0) return false;

        var words = upper.Split(' ');
        if (words.Length > 0 && OrgSuffixTokens.Contains(words[^1]))
            return true;

        foreach (var phrase in OrgSuffixPhrases)
            if (upper.EndsWith(phrase, StringComparison.Ordinal)) return true;

        foreach (var prefix in OrgPrefixes)
            if (upper.StartsWith(prefix, StringComparison.Ordinal)) return true;

        foreach (var keyword in OrgKeywords)
            if (upper.Contains(keyword, StringComparison.Ordinal)) return true;

        if (KnownGovernmentAgencies.Contains(upper))
            return true;

        return false;
    }

    private static SwedishOrganizationType InferSwedishOrganizationTypeFromName(string collapsed)
    {
        var upper = collapsed.ToUpperInvariant();
        if (upper.Length == 0) return SwedishOrganizationType.Unknown;

        var words = upper.Split(' ');
        var lastWord = words.Length > 0 ? words[^1] : "";

        if (lastWord == "AB") return SwedishOrganizationType.Aktiebolag;
        if (lastWord == "KB" || OrganizationValidationUtils.NameContains(upper, "KOMMANDITBOLAG"))
            return SwedishOrganizationType.Kommanditbolag;
        if (lastWord == "HB" || OrganizationValidationUtils.NameContains(upper, "HANDELSBOLAG"))
            return SwedishOrganizationType.Handelsbolag;

        if (OrganizationValidationUtils.NameContainsStandalone(upper, "BRF") ||
            OrganizationValidationUtils.NameContains(upper, "BOSTADSRÄTTSFÖRENING", "BOSTADSRATTSFORENING"))
            return SwedishOrganizationType.Bostadsrattsforening;

        if (OrganizationValidationUtils.NameContains(upper,
                "SAMFÄLLIGHETSFÖRENING", "SAMFALLIGHETSFORENING", "SAMFALLIGHETSSFORENING",
                "SAMFÄLLIGHET", "SAMFALLIGHET"))
            return SwedishOrganizationType.Samfallighetsforening;

        if (lastWord == "HSB" || upper.StartsWith("HSB ", StringComparison.Ordinal))
            return SwedishOrganizationType.EkonomiskForening;

        if (upper.EndsWith(" EK.FÖR.", StringComparison.Ordinal) ||
            upper.EndsWith(" EK FÖR", StringComparison.Ordinal) ||
            OrganizationValidationUtils.NameContains(upper, "EKONOMISK FÖRENING"))
            return SwedishOrganizationType.EkonomiskForening;

        if (OrganizationValidationUtils.NameContains(upper, "IDEELL FÖRENING"))
            return SwedishOrganizationType.IdeellForening;

        if (upper.StartsWith("STIFTELSEN ", StringComparison.Ordinal) ||
            OrganizationValidationUtils.NameContains(upper, "STIFTELSE"))
            return SwedishOrganizationType.Stiftelse;

        if (OrganizationValidationUtils.NameContains(upper, "KOMMUN"))
            return SwedishOrganizationType.Kommun;

        if (OrganizationValidationUtils.NameContains(upper, "REGION", "LANDSTING"))
            return SwedishOrganizationType.Region;

        if (OrganizationValidationUtils.NameContains(upper, "FÖRSAMLING", "FORSAMLING"))
            return SwedishOrganizationType.Forsamling;

        if (OrganizationValidationUtils.NameContains(upper, "MYNDIGHET"))
            return SwedishOrganizationType.OffentligSektor;

        if (KnownGovernmentAgencies.Contains(upper))
            return SwedishOrganizationType.OffentligSektor;

        if (OrganizationValidationUtils.NameContains(upper, "DÖDSBO", "DODSBO"))
            return SwedishOrganizationType.Dodsbo;

        if (lastWord == "EF" || OrganizationValidationUtils.NameContains(upper, "ENSKILD FIRMA"))
            return SwedishOrganizationType.EnskildFirma;

        if (OrganizationValidationUtils.NameContains(upper, "PRIVATPERSON"))
            return SwedishOrganizationType.Privatperson;

        if (OrganizationValidationUtils.NameContainsStandalone(upper, "EUROPABOLAG", "SE-BOLAG"))
            return SwedishOrganizationType.Europabolag;
        if (OrganizationValidationUtils.NameContainsStandalone(upper, "EUROPEISK EKONOMISK INTRESSEGRUPPERING", "EEIG"))
            return SwedishOrganizationType.EuropeiskEkonomiskIntressegruppering;
        if (OrganizationValidationUtils.NameContainsStandalone(upper, "SCE") ||
            OrganizationValidationUtils.NameContains(upper, "EUROPEISK KOOPERATIV FÖRENING"))
            return SwedishOrganizationType.SCEForening;
        if (OrganizationValidationUtils.NameContainsStandalone(upper, "FILIAL"))
            return SwedishOrganizationType.Filial;

        if (OrganizationValidationUtils.NameContains(upper, "RIKSFÖRBUND", "FÖRENING"))
            return SwedishOrganizationType.IdeellForening;

        return SwedishOrganizationType.Unknown;
    }

    private static string StripPublSuffix(string name)
    {
        var trimmed = name.TrimEnd();
        if (trimmed.EndsWith("(publ)", StringComparison.OrdinalIgnoreCase))
            return trimmed[..^6].TrimEnd();
        return name;
    }

    private static bool Validate(string? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value)) return false;
        if (value.Any(char.IsControl)) return false;
        if (value.Length < 2 || value.Length > 200) return false;
        return CompanyNamePattern.IsMatch(value);
    }

    private static string CapitalizeWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        var lowered = SvCulture.TextInfo.ToLower(word);
        if (lowered.Length == 0) return lowered;
        return char.ToUpper(lowered[0], SvCulture) + lowered[1..];
    }

    public bool Equals(SwedishOrganizationName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is SwedishOrganizationName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishOrganizationName? a, SwedishOrganizationName? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishOrganizationName? a, SwedishOrganizationName? b) => !(a == b);
    public int CompareTo(SwedishOrganizationName? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishOrganizationName left, SwedishOrganizationName right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishOrganizationName left, SwedishOrganizationName right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishOrganizationName left, SwedishOrganizationName right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishOrganizationName left, SwedishOrganizationName right) => left.CompareTo(right) >= 0;
}
