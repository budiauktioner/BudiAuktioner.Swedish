namespace Buildi.Primitives.SampleData.Organization;

/// <summary>
/// Valid <see cref="Primitives.Organization.SwedishSniCode"/> values from publicly known Swedish organizations.
/// </summary>
public static class SwedishSniCodeSampleData
{
    public static Primitives.Organization.SwedishSniCode BudiAB { get; } = Primitives.Organization.SwedishSniCode.Parse("47917");
    public static Primitives.Organization.SwedishSniCode Systembolaget { get; } = Primitives.Organization.SwedishSniCode.Parse("47250");
    public static Primitives.Organization.SwedishSniCode SJ { get; } = Primitives.Organization.SwedishSniCode.Parse("49100");
    public static Primitives.Organization.SwedishSniCode PostNord { get; } = Primitives.Organization.SwedishSniCode.Parse("53100");
    public static Primitives.Organization.SwedishSniCode Vattenfall { get; } = Primitives.Organization.SwedishSniCode.Parse("35120");
    public static Primitives.Organization.SwedishSniCode Samhall { get; } = Primitives.Organization.SwedishSniCode.Parse("81210");
    public static Primitives.Organization.SwedishSniCode LKAB { get; } = Primitives.Organization.SwedishSniCode.Parse("07100");
    public static Primitives.Organization.SwedishSniCode Telia { get; } = Primitives.Organization.SwedishSniCode.Parse("61100");
    public static Primitives.Organization.SwedishSniCode SKR { get; } = Primitives.Organization.SwedishSniCode.Parse("94112");
    public static Primitives.Organization.SwedishSniCode SvenskaKyrkan { get; } = Primitives.Organization.SwedishSniCode.Parse("94910");
    public static Primitives.Organization.SwedishSniCode Folksam { get; } = Primitives.Organization.SwedishSniCode.Parse("65120");
    public static Primitives.Organization.SwedishSniCode Lantmannen { get; } = Primitives.Organization.SwedishSniCode.Parse("46210");
    public static Primitives.Organization.SwedishSniCode KooperativaForbundet { get; } = Primitives.Organization.SwedishSniCode.Parse("70100");

    public static IReadOnlyList<Primitives.Organization.SwedishSniCode> All { get; } =
    [
        BudiAB, Systembolaget, SJ, PostNord, Vattenfall, Samhall, LKAB, Telia,
        SKR, SvenskaKyrkan, Folksam, Lantmannen, KooperativaForbundet,
    ];
}
