namespace Buildi.Primitives.SampleData.Banking;

/// <summary>
/// Valid <see cref="Primitives.Banking.SwedishPostgiroNumber"/> values from publicly known Swedish organizations.
/// </summary>
public static class SwedishPostgiroNumberSampleData
{
    public static Primitives.Banking.SwedishPostgiroNumber Vattenfall { get; } = Primitives.Banking.SwedishPostgiroNumber.Parse("4131300-8");
    public static Primitives.Banking.SwedishPostgiroNumber SvenskaKyrkan { get; } = Primitives.Banking.SwedishPostgiroNumber.Parse("900122-3");

    public static IReadOnlyList<Primitives.Banking.SwedishPostgiroNumber> All { get; } = [Vattenfall, SvenskaKyrkan];
}
