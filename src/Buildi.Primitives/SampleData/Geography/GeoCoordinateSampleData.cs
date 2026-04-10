namespace Buildi.Primitives.SampleData.Geography;

/// <summary>
/// Valid <see cref="Primitives.Geography.GeoCoordinate"/> values for well-known Swedish cities
/// and other notable geographic locations.
/// </summary>
public static class GeoCoordinateSampleData
{
    /// <summary>Stockholm, Sweden (59.3293°N, 18.0686°E).</summary>
    public static Primitives.Geography.GeoCoordinate Stockholm { get; } = Primitives.Geography.GeoCoordinate.Parse("59.3293, 18.0686");

    /// <summary>Gothenburg, Sweden (57.7089°N, 11.9746°E).</summary>
    public static Primitives.Geography.GeoCoordinate Gothenburg { get; } = Primitives.Geography.GeoCoordinate.Parse("57.7089, 11.9746");

    /// <summary>Malmö, Sweden (55.6050°N, 13.0038°E).</summary>
    public static Primitives.Geography.GeoCoordinate Malmo { get; } = Primitives.Geography.GeoCoordinate.Parse("55.6050, 13.0038");

    /// <summary>Luleå, Sweden (65.5848°N, 22.1547°E).</summary>
    public static Primitives.Geography.GeoCoordinate Lulea { get; } = Primitives.Geography.GeoCoordinate.Parse("65.5848, 22.1547");

    /// <summary>Null Island — intersection of the equator and the prime meridian (0°N, 0°E).</summary>
    public static Primitives.Geography.GeoCoordinate NullIsland { get; } = Primitives.Geography.GeoCoordinate.Parse("0, 0");

    public static IReadOnlyList<Primitives.Geography.GeoCoordinate> All { get; } =
        [Stockholm, Gothenburg, Malmo, Lulea, NullIsland];
}
