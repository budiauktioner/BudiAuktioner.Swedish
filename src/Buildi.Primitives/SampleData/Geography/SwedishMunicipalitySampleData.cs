namespace Buildi.Primitives.SampleData.Geography;

/// <summary>
/// Valid <see cref="Primitives.Geography.SwedishMunicipality"/> values used in the sample organization catalog
/// and additional representative municipalities.
/// </summary>
public static class SwedishMunicipalitySampleData
{
    public static Primitives.Geography.SwedishMunicipality Stockholm { get; } = Primitives.Geography.SwedishMunicipality.Parse("Stockholm");
    public static Primitives.Geography.SwedishMunicipality Solna { get; } = Primitives.Geography.SwedishMunicipality.Parse("Solna");
    public static Primitives.Geography.SwedishMunicipality Uppsala { get; } = Primitives.Geography.SwedishMunicipality.Parse("Uppsala");
    public static Primitives.Geography.SwedishMunicipality Lulea { get; } = Primitives.Geography.SwedishMunicipality.Parse("Luleå");
    public static Primitives.Geography.SwedishMunicipality Goteborg { get; } = Primitives.Geography.SwedishMunicipality.Parse("Göteborg");
    public static Primitives.Geography.SwedishMunicipality Malmo { get; } = Primitives.Geography.SwedishMunicipality.Parse("Malmö");

    public static IReadOnlyList<Primitives.Geography.SwedishMunicipality> All { get; } =
        [Stockholm, Solna, Uppsala, Lulea, Goteborg, Malmo];
}
