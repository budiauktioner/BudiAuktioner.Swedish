namespace Buildi.Primitives.SampleData.Organization;

/// <summary>
/// Valid <see cref="Primitives.Organization.LeiCode"/> values from publicly known Swedish organizations.
/// </summary>
public static class LeiCodeSampleData
{
    public static Primitives.Organization.LeiCode Vattenfall { get; } = Primitives.Organization.LeiCode.Parse("549300T5RZ1HA5HZ3109");
    public static Primitives.Organization.LeiCode LKAB { get; } = Primitives.Organization.LeiCode.Parse("549300ONBUTV20237K19");
    public static Primitives.Organization.LeiCode Folksam { get; } = Primitives.Organization.LeiCode.Parse("5493003384H0SVUD4J19");

    public static IReadOnlyList<Primitives.Organization.LeiCode> All { get; } = [Vattenfall, LKAB, Folksam];
}
