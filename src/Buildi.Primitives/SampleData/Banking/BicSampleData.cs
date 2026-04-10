namespace Buildi.Primitives.SampleData.Banking;

/// <summary>
/// Valid <see cref="Primitives.Banking.Bic"/> values for well-known Swedish and international banks.
/// </summary>
public static class BicSampleData
{
    public static Primitives.Banking.Bic Nordea { get; } = Primitives.Banking.Bic.Parse("NDEASESS");
    public static Primitives.Banking.Bic SEB { get; } = Primitives.Banking.Bic.Parse("ESSESESS");
    public static Primitives.Banking.Bic Swedbank { get; } = Primitives.Banking.Bic.Parse("SWEDSESS");
    public static Primitives.Banking.Bic Handelsbanken { get; } = Primitives.Banking.Bic.Parse("HANDSESS");

    public static IReadOnlyList<Primitives.Banking.Bic> All { get; } = [Nordea, SEB, Swedbank, Handelsbanken];
}
