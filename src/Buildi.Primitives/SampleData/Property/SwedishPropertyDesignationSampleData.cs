namespace Buildi.Primitives.SampleData.Property;

/// <summary>
/// Valid <see cref="Primitives.Property.SwedishPropertyDesignation"/> values from publicly known
/// Swedish properties and generic examples.
/// </summary>
public static class SwedishPropertyDesignationSampleData
{
    public static Primitives.Property.SwedishPropertyDesignation UppsalaDomkyrka { get; } = Primitives.Property.SwedishPropertyDesignation.Parse("Uppsala Fjärdingen 22:1");

    public static Primitives.Property.SwedishPropertyDesignation GenericUrban { get; } = Primitives.Property.SwedishPropertyDesignation.Parse("Stockholm Norrmalm 3:12");
    public static Primitives.Property.SwedishPropertyDesignation GenericRural { get; } = Primitives.Property.SwedishPropertyDesignation.Parse("Gävle Olsbacka 11:1");

    public static IReadOnlyList<Primitives.Property.SwedishPropertyDesignation> All { get; } =
        [UppsalaDomkyrka, GenericUrban, GenericRural];
}
