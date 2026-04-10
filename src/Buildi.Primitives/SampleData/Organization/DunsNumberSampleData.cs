namespace Buildi.Primitives.SampleData.Organization;

/// <summary>
/// Valid <see cref="Primitives.Organization.DunsNumber"/> values from publicly known Swedish organizations.
/// </summary>
public static class DunsNumberSampleData
{
    public static Primitives.Organization.DunsNumber BudiAB { get; } = Primitives.Organization.DunsNumber.Parse("350827673");

    public static IReadOnlyList<Primitives.Organization.DunsNumber> All { get; } = [BudiAB];
}
