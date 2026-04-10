namespace Buildi.Primitives.SampleData.Organization;

/// <summary>
/// Valid <see cref="Primitives.Organization.SwedishOrganizationNumber"/> values from publicly known Swedish organizations
/// and generic examples covering different organization types.
/// </summary>
public static class OrganizationNumberSampleData
{
    public static Primitives.Organization.SwedishOrganizationNumber BudiAB { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("559246-0421");
    public static Primitives.Organization.SwedishOrganizationNumber Systembolaget { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556059-9473");
    public static Primitives.Organization.SwedishOrganizationNumber SJ { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556196-1599");
    public static Primitives.Organization.SwedishOrganizationNumber PostNord { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556711-5695");
    public static Primitives.Organization.SwedishOrganizationNumber Vattenfall { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556036-2138");
    public static Primitives.Organization.SwedishOrganizationNumber Samhall { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556448-1397");
    public static Primitives.Organization.SwedishOrganizationNumber LKAB { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556001-5835");
    public static Primitives.Organization.SwedishOrganizationNumber SVT { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556033-4285");
    public static Primitives.Organization.SwedishOrganizationNumber SverigesRadio { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556419-3232");
    public static Primitives.Organization.SwedishOrganizationNumber Telia { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("556430-0142");
    public static Primitives.Organization.SwedishOrganizationNumber StockholmsKommun { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("212000-0142");
    public static Primitives.Organization.SwedishOrganizationNumber SKR { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("222000-0315");
    public static Primitives.Organization.SwedishOrganizationNumber SvenskaKyrkan { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("252002-6135");
    public static Primitives.Organization.SwedishOrganizationNumber Folksam { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("502006-1619");
    public static Primitives.Organization.SwedishOrganizationNumber Lantmannen { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("769605-2856");
    public static Primitives.Organization.SwedishOrganizationNumber KooperativaForbundet { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("702001-1693");

    public static Primitives.Organization.SwedishOrganizationNumber Skatteverket { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-5448");
    public static Primitives.Organization.SwedishOrganizationNumber Forsakringskassan { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-5521");
    public static Primitives.Organization.SwedishOrganizationNumber Arbetsformedlingen { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-2114");
    public static Primitives.Organization.SwedishOrganizationNumber Bolagsverket { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-5000");
    public static Primitives.Organization.SwedishOrganizationNumber Trafikverket { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-6297");
    public static Primitives.Organization.SwedishOrganizationNumber Lantmateriet { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-4888");
    public static Primitives.Organization.SwedishOrganizationNumber Kronofogden { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-2809");
    public static Primitives.Organization.SwedishOrganizationNumber Transportstyrelsen { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-6099");
    public static Primitives.Organization.SwedishOrganizationNumber Migrationsverket { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-2163");
    public static Primitives.Organization.SwedishOrganizationNumber Tullverket { get; } = Primitives.Organization.SwedishOrganizationNumber.Parse("202100-0969");

    public static IReadOnlyList<Primitives.Organization.SwedishOrganizationNumber> All { get; } =
    [
        BudiAB, Systembolaget, SJ, PostNord, Vattenfall, Samhall, LKAB, SVT,
        SverigesRadio, Telia,
        Skatteverket, Forsakringskassan, Arbetsformedlingen, Bolagsverket, Trafikverket,
        Lantmateriet, Kronofogden, Transportstyrelsen, Migrationsverket, Tullverket,
        StockholmsKommun, SKR, SvenskaKyrkan, Folksam,
        Lantmannen, KooperativaForbundet,
    ];
}
