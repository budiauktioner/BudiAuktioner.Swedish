namespace Buildi.Primitives.SampleData.Geography;

/// <summary>
/// Valid <see cref="Primitives.Geography.SwedishCounty"/> values used in the sample organization catalog
/// and additional representative counties.
/// </summary>
public static class SwedishCountySampleData
{
    public static Primitives.Geography.SwedishCounty Stockholm { get; } = Primitives.Geography.SwedishCounty.Parse("01");
    public static Primitives.Geography.SwedishCounty Uppsala { get; } = Primitives.Geography.SwedishCounty.Parse("03");
    public static Primitives.Geography.SwedishCounty VastraGotaland { get; } = Primitives.Geography.SwedishCounty.Parse("14");
    public static Primitives.Geography.SwedishCounty Skane { get; } = Primitives.Geography.SwedishCounty.Parse("12");
    public static Primitives.Geography.SwedishCounty Norrbotten { get; } = Primitives.Geography.SwedishCounty.Parse("25");

    public static IReadOnlyList<Primitives.Geography.SwedishCounty> All { get; } =
        [Stockholm, Uppsala, VastraGotaland, Skane, Norrbotten];
}
