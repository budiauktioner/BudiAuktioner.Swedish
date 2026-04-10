namespace Buildi.Primitives.SampleData.Finance;

/// <summary>
/// Valid <see cref="Primitives.Finance.Isin"/> values from publicly listed Swedish companies
/// and generic international examples.
/// </summary>
public static class IsinSampleData
{
    public static Primitives.Finance.Isin Ericsson { get; } = Primitives.Finance.Isin.Parse("SE0000108656");
    public static Primitives.Finance.Isin Telia { get; } = Primitives.Finance.Isin.Parse("SE0000667891");
    public static Primitives.Finance.Isin AppleInc { get; } = Primitives.Finance.Isin.Parse("US0378331005");

    public static IReadOnlyList<Primitives.Finance.Isin> All { get; } = [Ericsson, Telia, AppleInc];
}
