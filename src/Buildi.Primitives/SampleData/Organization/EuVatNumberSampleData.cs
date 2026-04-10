namespace Buildi.Primitives.SampleData.Organization;

/// <summary>
/// Valid <see cref="Primitives.Organization.EuVatNumber"/> values from publicly known Swedish organizations.
/// </summary>
public static class EuVatNumberSampleData
{
    public static Primitives.Organization.EuVatNumber BudiAB { get; } = Primitives.Organization.EuVatNumber.Parse("SE559246042101");
    public static Primitives.Organization.EuVatNumber Systembolaget { get; } = Primitives.Organization.EuVatNumber.Parse("SE556059947301");
    public static Primitives.Organization.EuVatNumber SJ { get; } = Primitives.Organization.EuVatNumber.Parse("SE556196159901");
    public static Primitives.Organization.EuVatNumber PostNord { get; } = Primitives.Organization.EuVatNumber.Parse("SE556711569501");
    public static Primitives.Organization.EuVatNumber Vattenfall { get; } = Primitives.Organization.EuVatNumber.Parse("SE556036213801");
    public static Primitives.Organization.EuVatNumber Samhall { get; } = Primitives.Organization.EuVatNumber.Parse("SE556448139701");
    public static Primitives.Organization.EuVatNumber LKAB { get; } = Primitives.Organization.EuVatNumber.Parse("SE556001583501");
    public static Primitives.Organization.EuVatNumber SVT { get; } = Primitives.Organization.EuVatNumber.Parse("SE556033428501");
    public static Primitives.Organization.EuVatNumber SverigesRadio { get; } = Primitives.Organization.EuVatNumber.Parse("SE556419323201");
    public static Primitives.Organization.EuVatNumber Telia { get; } = Primitives.Organization.EuVatNumber.Parse("SE556430014201");

    public static IReadOnlyList<Primitives.Organization.EuVatNumber> All { get; } =
        [BudiAB, Systembolaget, SJ, PostNord, Vattenfall, Samhall, LKAB, SVT, SverigesRadio, Telia];
}
