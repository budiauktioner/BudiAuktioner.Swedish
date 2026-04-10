namespace Buildi.Primitives.SampleData.Banking;

/// <summary>
/// Valid <see cref="Primitives.Banking.Iban"/> values from publicly known Swedish organizations
/// and generic examples.
/// </summary>
public static class IbanSampleData
{
    public static Primitives.Banking.Iban Vattenfall { get; } = Primitives.Banking.Iban.Parse("SE7495000099604203849767");

    public static Primitives.Banking.Iban SwedishGeneric { get; } = Primitives.Banking.Iban.Parse("SE4550000000058398257466");
    public static Primitives.Banking.Iban GermanGeneric { get; } = Primitives.Banking.Iban.Parse("DE89370400440532013000");
    public static Primitives.Banking.Iban BritishGeneric { get; } = Primitives.Banking.Iban.Parse("GB29NWBK60161331926819");

    public static IReadOnlyList<Primitives.Banking.Iban> All { get; } =
        [Vattenfall, SwedishGeneric, GermanGeneric, BritishGeneric];
}
