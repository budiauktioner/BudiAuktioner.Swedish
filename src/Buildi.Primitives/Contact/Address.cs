using System.Text;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// An address (<c>adress</c>) combines street, postal code, city, and country into one normalized model. This type is useful when you want to parse complete free-text addresses, pass already separated components, or construct an address from already parsed value objects.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnord.se/">PostNord</see> — Swedish postal service</description></item>
/// <item><description><see href="https://www.lantmateriet.se/">Lantmäteriet</see> — Swedish mapping, cadastral and land registration authority</description></item>
/// <item><description><see href="https://www.iso.org/iso-3166-country-codes.html">ISO 3166-1</see> — country codes standard</description></item>
/// </list>
/// </remarks>
public sealed class Address
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Address", "Adress", "🏠", ["https://www.postnord.se/", "https://www.lantmateriet.se/", "https://www.iso.org/iso-3166-country-codes.html"]);

    private const int MaxInputLength = 1000;

    public AddressStreet Street { get; }
    public AddressZipCode? ZipCode { get; }
    public AddressCity? City { get; }
    public Country? Country { get; }

    public string? CareOf => Street.CareOf;
    public string? ApartmentNumber => Street.ApartmentNumber;
    public string? PostBox => Street.PostBox;
    public bool IsPostBox => Street.IsPostBox;

    public string Value => ToNormalizedString();
    public string Formatted => ToString();

    public Address(AddressStreet street, AddressZipCode? zipCode = null, AddressCity? city = null, Country? country = null)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        ZipCode = zipCode;
        City = city;
        Country = country;
    }

    public static bool TryParse(string? input, out Address? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        input = input!.Trim();
        if (input.Length > MaxInputLength) return false;

        var segments = SplitSegments(input);
        if (segments.Count == 0) return false;

        Country? country = null;
        TryConsumeCountry(segments, ref country);

        AddressZipCode? zipCode = null;
        AddressCity? city = null;

        if (segments.Count > 0 && TryConsumeZipAndCitySegment(segments, out zipCode, out city))
        {
            // Consumed from trailing segment.
        }

        var streetText = string.Join(", ", segments);
        if ((zipCode == null || city == null) &&
            TryExtractTrailingZipCity(streetText, out var extractedStreet, out var extractedZip, out var extractedCity))
        {
            streetText = extractedStreet;
            zipCode ??= extractedZip;
            city ??= extractedCity;
        }

        if (string.IsNullOrWhiteSpace(streetText)) return false;
        if (!AddressStreet.TryParse(streetText, city?.Value, zipCode?.Formatted ?? zipCode?.Value, out var street)) return false;

        result = new Address(street!, zipCode, city, country);
        return true;
    }

    public static bool TryParse(
        string? street,
        string? zipCode,
        string? city,
        string? country,
        out Address? result)
    {
        result = null;

        if (!AddressStreet.TryParse(street, city, zipCode, out var streetModel)) return false;

        AddressZipCode? zipCodeModel = null;
        if (!string.IsNullOrWhiteSpace(zipCode) && !AddressZipCode.TryParse(zipCode, out zipCodeModel)) return false;

        AddressCity? cityModel = null;
        if (!string.IsNullOrWhiteSpace(city) && !AddressCity.TryParse(city, out cityModel)) return false;

        Country? countryModel = null;
        if (!string.IsNullOrWhiteSpace(country) && !Country.TryParse(country, out countryModel)) return false;

        result = new Address(streetModel!, zipCodeModel, cityModel, countryModel);
        return true;
    }

    public static Address Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid address.", nameof(input));

        return result!;
    }

    public static Address Parse(string street, string? zipCode = null, string? city = null, string? country = null)
    {
        if (!TryParse(street, zipCode, city, country, out var result))
            throw new ArgumentException("Invalid address.", nameof(street));

        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    private static readonly Regex ZipAnchor = new(
        @"(?<!\d)\d{3}\s?\d{2}(?!\d)", RegexOptions.Compiled);

    private static readonly Regex StreetSuffixPattern = new(
        @"[\p{Lu}][\p{Ll}]+(?:\s+[\p{L}]+)*?" +
        @"(?:gatan|gata|vägen|väg|allén|allé|torget|torg|platsen|plats|" +
        @"stigen|stig|gränden|gränd|gången|gång|leden|led|" +
        @"backen|backe|kajen|kaj|bron|plan|" +
        @"stråket|terrassen|ringen|parken|hamnen)" +
        @"\s+\d{1,4}\s?[\p{Lu}]?",
        RegexOptions.Compiled);

    private const int BackwardScanDistance = 150;
    private const int ForwardScanDistance = 80;

    /// <summary>
    /// Scans unstructured text for potential Swedish addresses using two strategies:
    /// zip-code anchoring (Medium confidence) and street-suffix fallback (Low confidence).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real address in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Address>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Address>>();
        var coveredSpans = new List<(int Start, int End)>();

        FindByZipAnchor(text, results, coveredSpans);
        FindByStreetSuffix(text, results, coveredSpans);

        return results;
    }

    private static readonly HashSet<string> BareSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "gatan", "gata", "vägen", "väg", "allén", "allé", "torget", "torg",
        "platsen", "plats", "stigen", "stig", "gränden", "gränd", "gången",
        "gång", "leden", "led", "backen", "backe", "kajen", "kaj", "bron",
        "plan", "stråket", "terrassen", "ringen", "parken", "hamnen"
    };

    private static void FindByZipAnchor(
        string text, List<TextCandidate<Address>> results, List<(int Start, int End)> coveredSpans)
    {
        foreach (Match zipMatch in ZipAnchor.Matches(text))
        {
            var forwardEnd = TextScanHelpers.ScanForward(text, zipMatch.Index + zipMatch.Length, ForwardScanDistance);
            var backLimit = TextScanHelpers.ScanBackward(text, zipMatch.Index, BackwardScanDistance);

            Address? bestAddress = null;
            int bestStart = -1;
            var bestScore = -1;

            for (var start = zipMatch.Index - 1; start >= backLimit; start--)
            {
                if (start > backLimit && !TextScanHelpers.IsWordBoundary(text, start))
                    continue;

                var trimmedStart = start;
                while (trimmedStart < zipMatch.Index && text[trimmedStart] is ' ' or '\t')
                    trimmedStart++;

                var candidateText = text[trimmedStart..forwardEnd].Trim();
                if (string.IsNullOrWhiteSpace(candidateText)) continue;
                if (!TryParse(candidateText, out var address)) continue;
                if (address!.ZipCode == null || address.City == null) continue;
                if (!address.IsPostBox && address.Street.StreetName != null
                    && BareSuffixes.Contains(address.Street.StreetName))
                    continue;

                var score = ScoreAddress(address);
                if (score > bestScore)
                {
                    bestAddress = address;
                    bestStart = trimmedStart;
                    bestScore = score;
                }
                else if (score == bestScore && trimmedStart > 0 && TextScanHelpers.IsStrongStart(text, trimmedStart))
                {
                    bestAddress = address;
                    bestStart = trimmedStart;
                }
            }

            if (bestAddress == null) continue;

            var actualText = text[bestStart..forwardEnd].Trim();
            var actualStart = bestStart;
            while (actualStart < forwardEnd && text[actualStart] is ' ' or '\t') actualStart++;

            if (TextScanHelpers.IsAlreadyCovered(coveredSpans, actualStart, actualStart + actualText.Length))
                continue;

            results.Add(new TextCandidate<Address>(
                actualStart, actualText.Length, actualText,
                nameof(Address), TextCandidateCategory.Contact,
                bestAddress.ToNormalizedString(), bestAddress.ToString(),
                bestAddress.ToMaskedString(), TextMatchConfidence.Medium, bestAddress));
            coveredSpans.Add((actualStart, actualStart + actualText.Length));
        }
    }

    private static int ScoreAddress(Address address)
    {
        var score = 0;
        if (address.IsPostBox) score += 4;
        if (address.Street.StreetName != null) score += 2;
        if (address.Street.StreetNumber != null) score++;
        if (address.ApartmentNumber != null) score += 2;
        if (address.CareOf != null) score += 2;
        if (address.Country != null) score++;
        return score;
    }

    private static void FindByStreetSuffix(
        string text, List<TextCandidate<Address>> results, List<(int Start, int End)> coveredSpans)
    {
        foreach (Match match in StreetSuffixPattern.Matches(text))
        {
            if (TextScanHelpers.IsAlreadyCovered(coveredSpans, match.Index, match.Index + match.Length))
                continue;

            if (!TryParse(match.Value, out var address)) continue;

            results.Add(new TextCandidate<Address>(
                match.Index, match.Length, match.Value,
                nameof(Address), TextCandidateCategory.Contact,
                address!.ToNormalizedString(), address.ToString(),
                address.ToMaskedString(), TextMatchConfidence.Low, address));
            coveredSpans.Add((match.Index, match.Index + match.Length));
        }
    }

    /// <summary>
    /// Returns the address as a single human-readable line, for example
    /// <c>c/o Anna Svensson, Storgatan 12 lgh 1201, 114 53 Stockholm, Sverige</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var result) ? result!.ToString() : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized address using normalized component values, for example
    /// <c>c/o Anna Svensson, Storgatan 12 lgh 1201, 11453, Stockholm, SE</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var result)) return result!.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized address using normalized component values, for example
    /// <c>c/o Anna Svensson, Storgatan 12 lgh 1201, 11453, Stockholm, SE</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var parts = new List<string>
        {
            BuildStreetLine()
        };

        if (ZipCode != null) parts.Add(ZipCode.Value);
        if (City != null) parts.Add(City.Value);
        if (Country != null) parts.Add(Country.Alpha2Code);

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Returns the address as multiple lines in the current display language, suitable for postal labels.
    /// </summary>
    public string ToMultilineString()
    {
        var useEnglish = !PrimitivesDefaults.UseLocalizedDisplayNames;
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            lines.Add($"c/o {CareOf}");

        lines.Add(BuildStreetCoreLine());

        var locality = BuildLocalityLine();
        if (!string.IsNullOrWhiteSpace(locality))
            lines.Add(locality);

        if (Country != null && (!IsSweden(Country) || useEnglish))
            lines.Add(useEnglish ? Country.EnglishName : Country.DisplayName);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns the address as a single line with the country name in the current display language
    /// (Swedish when <see cref="PrimitivesDefaults.UseLocalizedDisplayNames"/> is true, otherwise English),
    /// for example <c>Storgatan 12, 114 53 Stockholm, Tyskland</c> or <c>Storgatan 12, 114 53 Stockholm, Germany</c>.
    /// </summary>
    public string ToDisplayString() => BuildSingleLine(useEnglish: !PrimitivesDefaults.UseLocalizedDisplayNames);

    /// <summary>
    /// Returns the address as a single line with the country name in English,
    /// for example <c>Storgatan 12, 114 53 Stockholm, Sweden</c>.
    /// </summary>
    public string ToEnglishString() => BuildSingleLine(useEnglish: true);

    /// <summary>
    /// Returns the address as a single line with the country name in the country's own native language (endonym),
    /// for example <c>Storgatan 12, 114 53 Stockholm, Deutschland</c> for a German address.
    /// </summary>
    public string ToNativeString() => BuildSingleLineWithNativeCountry();

    /// <summary>
    /// Returns the address as a single human-readable line in the current display language,
    /// for example <c>c/o Anna Svensson, Storgatan 12 lgh 1201, 114 53 Stockholm, Tyskland</c>.
    /// </summary>
    public override string ToString() => ToDisplayString();

    /// <summary>
    /// Returns the address in domestic format for the address's own country — country name
    /// is omitted and the zip code is formatted using country-specific rules when available.
    /// For example a Polish address returns <c>ul. Wiejska 4/6/8, 00-902 Warszawa</c>
    /// instead of <c>ul. Wiejska 4/6/8, 00902 Warszawa, Polen</c>.
    /// </summary>
    public string ToDomesticString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            parts.Add($"c/o {CareOf}");

        parts.Add(BuildStreetCoreLine());

        var locality = BuildDomesticLocalityLine();
        if (!string.IsNullOrWhiteSpace(locality))
            parts.Add(locality);

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Returns the address as multiple lines in domestic format — country name is omitted,
    /// zip code uses country-specific formatting when available.
    /// </summary>
    public string ToDomesticMultilineString()
    {
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            lines.Add($"c/o {CareOf}");

        lines.Add(BuildStreetCoreLine());

        var locality = BuildDomesticLocalityLine();
        if (!string.IsNullOrWhiteSpace(locality))
            lines.Add(locality);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Attempts to wrap this address in a country-specific type when the country is known
    /// and a specific implementation exists. Returns <see langword="null"/> when the country
    /// is unknown or no country-specific type matches.
    /// </summary>
    public ICountryAddress? AsCountryAddress()
    {
        if (Country == null) return null;
        var reg = CountryRegistry.GetValueOrDefault(Country.Alpha2Code);
        return reg?.TryWrap(this);
    }

    private delegate bool TryParseAddress<T>(string? street, string? zipCode, string? city, string? country, out T? result) where T : class;

    private sealed record CountryRegistration(
        Country Country,
        Func<Address, ICountryAddress?> TryWrap,
        Func<string, string?> FormatZip);

    private static CountryRegistration Reg<TAddr>(
        Country country,
        TryParseAddress<TAddr> tryParse,
        Func<string, string?> formatZip) where TAddr : class, ICountryAddress
    {
        var alpha2 = country.Alpha2Code;
        return new CountryRegistration(
            country,
            a => tryParse(a.Street.Street, a.ZipCode?.Value, a.City?.Value, alpha2, out var r) ? r : null,
            formatZip);
    }

    private static readonly Dictionary<string, CountryRegistration> CountryRegistry = new CountryRegistration[]
    {
        Reg<SwedishAddress>(Country.Sweden, SwedishAddress.TryParse, v => SwedishAddressZipCode.Format(v)),
        Reg<NorwegianAddress>(Country.Norway, NorwegianAddress.TryParse, v => NorwegianAddressZipCode.Format(v)),
        Reg<FinnishAddress>(Country.Finland, FinnishAddress.TryParse, v => FinnishAddressZipCode.Format(v)),
        Reg<DanishAddress>(Country.Denmark, DanishAddress.TryParse, v => DanishAddressZipCode.Format(v)),
        Reg<GermanAddress>(Country.Germany, GermanAddress.TryParse, v => GermanAddressZipCode.Format(v)),
        Reg<PolishAddress>(Country.Poland, PolishAddress.TryParse, v => PolishAddressZipCode.Format(v)),
        Reg<EstonianAddress>(Country.Estonia, EstonianAddress.TryParse, v => EstonianAddressZipCode.Format(v)),
        Reg<LithuanianAddress>(Country.Lithuania, LithuanianAddress.TryParse, v => LithuanianAddressZipCode.Format(v)),
        Reg<RomanianAddress>(Country.Romania, RomanianAddress.TryParse, v => RomanianAddressZipCode.Format(v)),
        Reg<BulgarianAddress>(Country.Bulgaria, BulgarianAddress.TryParse, v => BulgarianAddressZipCode.Format(v)),
        Reg<LatvianAddress>(Country.Latvia, LatvianAddress.TryParse, v => LatvianAddressZipCode.Format(v)),
        Reg<CzechAddress>(Country.CzechRepublic, CzechAddress.TryParse, v => CzechAddressZipCode.Format(v)),
        Reg<SpanishAddress>(Country.Spain, SpanishAddress.TryParse, v => SpanishAddressZipCode.Format(v)),
        Reg<DutchAddress>(Country.Netherlands, DutchAddress.TryParse, v => DutchAddressZipCode.Format(v)),
        Reg<GreekAddress>(Country.Greece, GreekAddress.TryParse, v => GreekAddressZipCode.Format(v)),
        Reg<ItalianAddress>(Country.Italy, ItalianAddress.TryParse, v => ItalianAddressZipCode.Format(v)),
        Reg<SlovenianAddress>(Country.Slovenia, SlovenianAddress.TryParse, v => SlovenianAddressZipCode.Format(v)),
        Reg<CroatianAddress>(Country.Croatia, CroatianAddress.TryParse, v => CroatianAddressZipCode.Format(v)),
        Reg<PortugueseAddress>(Country.Portugal, PortugueseAddress.TryParse, v => PortugueseAddressZipCode.Format(v)),
        Reg<HungarianAddress>(Country.Hungary, HungarianAddress.TryParse, v => HungarianAddressZipCode.Format(v)),
        Reg<FrenchAddress>(Country.France, FrenchAddress.TryParse, v => FrenchAddressZipCode.Format(v)),
        Reg<SlovakAddress>(Country.Slovakia, SlovakAddress.TryParse, v => SlovakAddressZipCode.Format(v)),
        Reg<BelgianAddress>(Country.Belgium, BelgianAddress.TryParse, v => BelgianAddressZipCode.Format(v)),
        Reg<BritishAddress>(Country.UnitedKingdom, BritishAddress.TryParse, v => BritishAddressZipCode.Format(v)),
        Reg<AustrianAddress>(Country.Austria, AustrianAddress.TryParse, v => AustrianAddressZipCode.Format(v)),
        Reg<CypriotAddress>(Country.Cyprus, CypriotAddress.TryParse, v => CypriotAddressZipCode.Format(v)),
        Reg<IcelandicAddress>(Country.Iceland, IcelandicAddress.TryParse, v => IcelandicAddressZipCode.Format(v)),
        Reg<SwissAddress>(Country.Switzerland, SwissAddress.TryParse, v => SwissAddressZipCode.Format(v)),
        Reg<IrishAddress>(Country.Ireland, IrishAddress.TryParse, v => IrishAddressZipCode.Format(v)),
        Reg<LuxembourgishAddress>(Country.Luxembourg, LuxembourgishAddress.TryParse, v => LuxembourgishAddressZipCode.Format(v)),
        Reg<MalteseAddress>(Country.Malta, MalteseAddress.TryParse, v => MalteseAddressZipCode.Format(v)),
        Reg<LiechtensteinAddress>(Country.Liechtenstein, LiechtensteinAddress.TryParse, v => LiechtensteinAddressZipCode.Format(v)),
    }.ToDictionary(r => r.Country.Alpha2Code, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The set of countries that have specific address and zip code type implementations.
    /// </summary>
    public static IReadOnlyList<Country> SupportedCountryAddressTypes { get; } =
        CountryRegistry.Values.Select(r => r.Country).ToList().AsReadOnly();

    private string? BuildDomesticLocalityLine()
    {
        var builder = new StringBuilder();

        var zipDisplay = GetCountryFormattedZip() ?? ZipCode?.Formatted;
        if (zipDisplay != null)
            builder.Append(zipDisplay);

        if (City != null)
        {
            if (builder.Length > 0)
                builder.Append(' ');
            builder.Append(City.Value);
        }

        return builder.Length == 0 ? null : builder.ToString();
    }

    private string? GetCountryFormattedZip()
    {
        if (ZipCode == null || Country == null) return null;
        var reg = CountryRegistry.GetValueOrDefault(Country.Alpha2Code);
        return reg?.FormatZip(ZipCode.Value);
    }

    private string BuildSingleLine(bool useEnglish)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            parts.Add($"c/o {CareOf}");

        parts.Add(BuildStreetCoreLine());

        var locality = BuildLocalityLine();
        if (!string.IsNullOrWhiteSpace(locality))
            parts.Add(locality);

        if (Country != null && (!IsSweden(Country) || useEnglish))
            parts.Add(useEnglish ? Country.EnglishName : Country.DisplayName);

        return string.Join(", ", parts);
    }

    private string BuildSingleLineWithNativeCountry()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            parts.Add($"c/o {CareOf}");

        parts.Add(BuildStreetCoreLine());

        var locality = BuildLocalityLine();
        if (!string.IsNullOrWhiteSpace(locality))
            parts.Add(locality);

        if (Country != null && !IsSweden(Country))
            parts.Add(Country.NativeName);

        return string.Join(", ", parts);
    }

    private string BuildStreetLine()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            parts.Add($"c/o {CareOf}");

        parts.Add(BuildStreetCoreLine());
        return string.Join(", ", parts);
    }

    private string BuildStreetCoreLine()
    {
        if (!string.IsNullOrWhiteSpace(ApartmentNumber))
            return $"{Street.Street} lgh {ApartmentNumber}";

        return Street.Street;
    }

    private string? BuildLocalityLine()
    {
        var builder = new StringBuilder();

        if (ZipCode != null)
            builder.Append(ZipCode.Formatted);

        if (City != null)
        {
            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append(City.Value);
        }

        return builder.Length == 0 ? null : builder.ToString();
    }

    private static List<string> SplitSegments(string input)
    {
        return input
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Split(['\n', ','], StringSplitOptions.RemoveEmptyEntries)
            .Select(InputSanitization.CollapseWhitespace)
            .Where(static s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    private static void TryConsumeCountry(List<string> segments, ref Country? country)
    {
        if (segments.Count == 0 || country != null) return;

        if (Country.TryParse(segments[^1], out var directCountry))
        {
            country = directCountry;
            segments.RemoveAt(segments.Count - 1);
            return;
        }

        var combined = segments[^1];
        var tokens = combined.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var tokenCount = Math.Min(5, tokens.Length); tokenCount >= 1; tokenCount--)
        {
            var suffix = string.Join(" ", tokens[^tokenCount..]);
            if (!Country.TryParse(suffix, out var parsedCountry)) continue;

            var prefix = string.Join(" ", tokens[..^tokenCount]);
            country = parsedCountry;

            if (string.IsNullOrWhiteSpace(prefix))
                segments.RemoveAt(segments.Count - 1);
            else
                segments[^1] = prefix;

            return;
        }
    }

    private static bool TryConsumeZipAndCitySegment(List<string> segments, out AddressZipCode? zipCode, out AddressCity? city)
    {
        zipCode = null;
        city = null;
        if (segments.Count == 0) return false;

        var last = segments[^1];
        if (!TryParseZipAndCity(last, out zipCode, out city)) return false;

        segments.RemoveAt(segments.Count - 1);
        return true;
    }

    private static bool TryExtractTrailingZipCity(
        string input,
        out string street,
        out AddressZipCode? zipCode,
        out AddressCity? city)
    {
        street = input;
        zipCode = null;
        city = null;

        var normalized = InputSanitization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(normalized)) return false;

        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 3) return false;

        for (var zipStart = tokens.Length - 2; zipStart >= 1; zipStart--)
        {
            for (var zipTokenCount = 1; zipTokenCount <= Math.Min(3, tokens.Length - zipStart - 1); zipTokenCount++)
            {
                var zipCandidate = string.Join(" ", tokens[zipStart..(zipStart + zipTokenCount)]);
                var cityCandidate = string.Join(" ", tokens[(zipStart + zipTokenCount)..]);
                var streetCandidate = string.Join(" ", tokens[..zipStart]);

                if (!AddressZipCode.TryParse(zipCandidate, out var parsedZip)) continue;
                if (!AddressCity.TryParse(cityCandidate, out var parsedCity)) continue;
                if (string.IsNullOrWhiteSpace(streetCandidate)) continue;

                street = streetCandidate;
                zipCode = parsedZip;
                city = parsedCity;
                return true;
            }
        }

        return false;
    }

    private static bool TryParseZipAndCity(string input, out AddressZipCode? zipCode, out AddressCity? city)
    {
        zipCode = null;
        city = null;

        var normalized = InputSanitization.CollapseWhitespace(input);
        if (string.IsNullOrWhiteSpace(normalized)) return false;

        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2) return false;

        for (var zipTokenCount = Math.Min(3, tokens.Length - 1); zipTokenCount >= 1; zipTokenCount--)
        {
            var prefixZip = string.Join(" ", tokens[..zipTokenCount]);
            var suffixCity = string.Join(" ", tokens[zipTokenCount..]);

            if (AddressZipCode.TryParse(prefixZip, out var prefixZipModel) &&
                AddressCity.TryParse(suffixCity, out var suffixCityModel))
            {
                zipCode = prefixZipModel;
                city = suffixCityModel;
                return true;
            }
        }

        for (var zipTokenCount = Math.Min(3, tokens.Length - 1); zipTokenCount >= 1; zipTokenCount--)
        {
            var prefixCity = string.Join(" ", tokens[..^zipTokenCount]);
            var suffixZip = string.Join(" ", tokens[^zipTokenCount..]);

            if (AddressCity.TryParse(prefixCity, out var prefixCityModel) &&
                AddressZipCode.TryParse(suffixZip, out var suffixZipModel))
            {
                zipCode = suffixZipModel;
                city = prefixCityModel;
                return true;
            }
        }

        return false;
    }

    private static bool IsSweden(Country country) =>
        country.Alpha2Code.Equals("SE", StringComparison.OrdinalIgnoreCase);
}
