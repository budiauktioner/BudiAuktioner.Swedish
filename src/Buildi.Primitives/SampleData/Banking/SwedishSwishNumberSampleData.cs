namespace Buildi.Primitives.SampleData.Banking;

/// <summary>
/// Valid <see cref="Primitives.Banking.SwedishSwishNumber"/> values from publicly known Swedish organizations.
/// </summary>
public static class SwedishSwishNumberSampleData
{
    /// <summary>Röda Korset (Swedish Red Cross) — climate disaster work. Source: rodakorset.se/skank-pengar/swisha-en-gava/</summary>
    public static Primitives.Banking.SwedishSwishNumber RodaKorset { get; } = Primitives.Banking.SwedishSwishNumber.Parse("1236652895");

    /// <summary>Rädda Barnen (Save the Children) — donations (90-number). Source: raddabarnen.se/stod-oss/swish/</summary>
    public static Primitives.Banking.SwedishSwishNumber RaddaBarnen { get; } = Primitives.Banking.SwedishSwishNumber.Parse("9020033");

    public static IReadOnlyList<Primitives.Banking.SwedishSwishNumber> All { get; } = [RodaKorset, RaddaBarnen];
}
