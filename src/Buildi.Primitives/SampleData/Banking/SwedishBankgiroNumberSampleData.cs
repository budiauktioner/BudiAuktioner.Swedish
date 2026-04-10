namespace Buildi.Primitives.SampleData.Banking;

/// <summary>
/// Valid <see cref="Primitives.Banking.SwedishBankgiroNumber"/> values from publicly known Swedish organizations.
/// </summary>
public static class SwedishBankgiroNumberSampleData
{
    public static Primitives.Banking.SwedishBankgiroNumber BudiAB { get; } = Primitives.Banking.SwedishBankgiroNumber.Parse("235-9321");
    public static Primitives.Banking.SwedishBankgiroNumber Vattenfall { get; } = Primitives.Banking.SwedishBankgiroNumber.Parse("5110-8348");
    public static Primitives.Banking.SwedishBankgiroNumber Telia { get; } = Primitives.Banking.SwedishBankgiroNumber.Parse("5117-7913");
    public static Primitives.Banking.SwedishBankgiroNumber SvenskaKyrkan { get; } = Primitives.Banking.SwedishBankgiroNumber.Parse("900-1223");

    public static IReadOnlyList<Primitives.Banking.SwedishBankgiroNumber> All { get; } =
        [BudiAB, Vattenfall, Telia, SvenskaKyrkan];
}
