namespace Buildi.Primitives.SampleData.Property;

/// <summary>
/// Valid <see cref="Primitives.Property.SwedishPropertyTaxationCode"/> values for common
/// Swedish property taxation categories.
/// </summary>
public static class SwedishPropertyTaxationCodeSampleData
{
    public static Primitives.Property.SwedishPropertyTaxationCode SmahusBebyggd { get; } = Primitives.Property.SwedishPropertyTaxationCode.Parse("220");
    public static Primitives.Property.SwedishPropertyTaxationCode HyreshusBostad { get; } = Primitives.Property.SwedishPropertyTaxationCode.Parse("320");
    public static Primitives.Property.SwedishPropertyTaxationCode IndustriBebyggd { get; } = Primitives.Property.SwedishPropertyTaxationCode.Parse("420");

    public static IReadOnlyList<Primitives.Property.SwedishPropertyTaxationCode> All { get; } = [SmahusBebyggd, HyreshusBostad, IndustriBebyggd];
}
