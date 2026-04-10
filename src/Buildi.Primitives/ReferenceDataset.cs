using Buildi.Primitives.Finance;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives;

/// <summary>
/// Describes a packaged reference dataset — its name, source, and the date it was last verified
/// against the upstream authority. All data in this library is compiled at build time; no external
/// calls are made at runtime.
/// </summary>
public sealed record ReferenceDataset(
    string Name,
    string Description,
    string Source,
    Uri SourceUrl,
    DateOnly LastVerified,
    int EntryCount);

/// <summary>
/// Central registry of all embedded reference datasets and the date each was last verified against
/// its upstream source. Use this to answer "how current is the data in this package?".
/// </summary>
/// <remarks>
/// All dates reflect when the dataset was reviewed against the authoritative source; the actual
/// content may not have changed on that date.
/// </remarks>
public static class ReferenceData
{
    private static readonly DateOnly DataDate = new(2026, 3, 24);

    /// <summary>21 Swedish counties (län) with codes and names.</summary>
    public static ReferenceDataset SwedishCounties { get; } = new(
        "SwedishCounties",
        "21 Swedish counties (län) with 2-digit codes, Swedish and English names",
        "SCB",
        new Uri("https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/"),
        DataDate,
        21);

    /// <summary>290 Swedish municipalities (kommuner) with codes and names.</summary>
    public static ReferenceDataset SwedishMunicipalities { get; } = new(
        "SwedishMunicipalities",
        "290 Swedish municipalities (kommuner) with 4-digit codes and names",
        "SCB",
        new Uri("https://www.scb.se/en/finding-statistics/regional-statistics/regional-divisions/counties-and-municipalities/counties-and-municipalities-in-numerical-order/"),
        DataDate,
        290);

    /// <summary>ISO 3166-1 countries with names, codes, calling codes, TLDs, and classifications.</summary>
    public static ReferenceDataset Countries { get; } = new(
        "Countries",
        "Country and territory data aligned with ISO 3166-1, with English, Swedish and local names, alpha-2/alpha-3/numeric codes, calling code, ccTLD, continent, EU/EEA/Schengen/Nordic/SEPA membership, and currency",
        "Public reference sources aligned with ISO 3166-1",
        new Uri("https://www.iso.org/iso-3166-country-codes.html"),
        DataDate,
        Country.All.Count);

    /// <summary>EU member states list (27 countries).</summary>
    public static ReferenceDataset EuropeanUnionMembers { get; } = new(
        "EuropeanUnionMembers",
        "Current European Union member states used by Country.IsInEuropeanUnion",
        "European Council",
        new Uri("https://european-union.europa.eu/principles-countries-history/country-profiles_en"),
        DataDate,
        27);

    /// <summary>EEA member states list (EU + Iceland, Liechtenstein, Norway).</summary>
    public static ReferenceDataset EeaMembers { get; } = new(
        "EeaMembers",
        "European Economic Area member states used by Country.IsInEea",
        "EFTA",
        new Uri("https://www.efta.int/eea"),
        DataDate,
        30);

    /// <summary>Schengen area countries.</summary>
    public static ReferenceDataset SchengenMembers { get; } = new(
        "SchengenMembers",
        "Schengen area countries used by Country.IsInSchengen",
        "European Council",
        new Uri("https://www.consilium.europa.eu/en/policies/schengen-area/"),
        DataDate,
        29);

    /// <summary>SEPA geographical scope countries and territories represented by <see cref="Country"/>.</summary>
    public static ReferenceDataset SepaMembers { get; } = new(
        "SepaMembers",
        "SEPA geographical scope countries and crown dependencies represented by Country.IsInSepa",
        "ECB / EPC",
        new Uri("https://www.europeanpaymentscouncil.eu/document-library/other/epc-list-sepa-scheme-countries"),
        DataDate,
        44);

    /// <summary>ISO 4217 currencies with codes, names, symbols and decimal places.</summary>
    public static ReferenceDataset Currencies { get; } = new(
        "Currencies",
        "Currency data aligned with ISO 4217, with code, English/Swedish name, symbol and decimal places",
        "Public reference sources aligned with ISO 4217",
        new Uri("https://www.iso.org/iso-4217-currency-codes.html"),
        DataDate,
        Currency.All.Count);

    /// <summary>SNI 2025 code format validation (5-digit Swedish standard industrial classification, effective December 2024).</summary>
    public static ReferenceDataset SniCodes { get; } = new(
        "SniCodes",
        "SNI 2025 code format validation — 5-digit codes in XX.XXX format; no embedded code list",
        "SCB",
        new Uri("https://www.scb.se/en/documentation/classifications-and-standards/swedish-standard-industrial-classification-sni/"),
        DataDate,
        0);

    /// <summary>Swedish bank clearing number ranges and bank identification.</summary>
    public static ReferenceDataset SwedishBankClearingNumbers { get; } = new(
        "SwedishBankClearingNumbers",
        "Swedish bank clearing number ranges used to identify banks from 4–5 digit clearing numbers",
        "BSAB",
        new Uri("https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer#clearingTable"),
        DataDate,
        50);

    /// <summary>Known public email provider domains (Gmail, Outlook, Yahoo, Swedish providers, etc.).</summary>
    public static ReferenceDataset PublicEmailProviders { get; } = new(
        "PublicEmailProviders",
        "Known public/shared email provider domains used by EmailAddress.Provider",
        "Maintainer-curated public provider list",
        new Uri("https://en.wikipedia.org/wiki/Webmail"),
        DataDate,
        0);

    /// <summary>Swedish zip code format (5-digit NNN NN) and international postal patterns.</summary>
    public static ReferenceDataset PostalPatterns { get; } = new(
        "PostalPatterns",
        "Postal code format patterns for Sweden (5-digit) and international formats (DK, NL, GB, etc.)",
        "PostNord / various",
        new Uri("https://www.postnord.se/"),
        DataDate,
        0);

    /// <summary>GS1 company prefix ranges mapping barcode prefixes to issuing countries and organizations.</summary>
    public static ReferenceDataset Gs1Prefixes { get; } = new(
        "Gs1Prefixes",
        "GS1 company prefix ranges mapping 1–3 digit barcode prefixes to issuing country or organization, used by Gtin13",
        "GS1",
        new Uri("https://www.gs1.org/standards/id-keys/company-prefix"),
        DataDate,
        0);

    /// <summary>All packaged reference datasets.</summary>
    public static IReadOnlyList<ReferenceDataset> All { get; } =
    [
        SwedishCounties,
        SwedishMunicipalities,
        Countries,
        EuropeanUnionMembers,
        EeaMembers,
        SchengenMembers,
        SepaMembers,
        Currencies,
        SniCodes,
        SwedishBankClearingNumbers,
        PublicEmailProviders,
        PostalPatterns,
        Gs1Prefixes,
    ];
}
