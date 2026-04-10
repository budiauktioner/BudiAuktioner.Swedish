namespace Buildi.Primitives.SampleData.Organization;

/// <summary>
/// Valid <see cref="Primitives.Organization.ElfCode"/> values for common Swedish entity legal forms.
/// </summary>
public static class ElfCodeSampleData
{
    public static Primitives.Organization.ElfCode Aktiebolag { get; } = Primitives.Organization.ElfCode.Parse("XTIQ");
    public static Primitives.Organization.ElfCode Handelsbolag { get; } = Primitives.Organization.ElfCode.Parse("N2GY");
    public static Primitives.Organization.ElfCode EnskildFirma { get; } = Primitives.Organization.ElfCode.Parse("WJEL");
    public static Primitives.Organization.ElfCode Stiftelse { get; } = Primitives.Organization.ElfCode.Parse("CLBQ");

    public static IReadOnlyList<Primitives.Organization.ElfCode> All { get; } = [Aktiebolag, Handelsbolag, EnskildFirma, Stiftelse];
}
