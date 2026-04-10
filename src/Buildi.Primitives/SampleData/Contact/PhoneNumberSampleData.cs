namespace Buildi.Primitives.SampleData.Contact;

/// <summary>
/// Valid <see cref="Primitives.Contact.PhoneNumber"/> values from publicly known Swedish organizations
/// and PTS-reserved test ranges.
/// </summary>
public static class PhoneNumberSampleData
{
    public static Primitives.Contact.PhoneNumber Systembolaget { get; } = Primitives.Contact.PhoneNumber.Parse("+46850330000");
    public static Primitives.Contact.PhoneNumber SJ { get; } = Primitives.Contact.PhoneNumber.Parse("+46107516000");
    public static Primitives.Contact.PhoneNumber Vattenfall { get; } = Primitives.Contact.PhoneNumber.Parse("+4687396000");
    public static Primitives.Contact.PhoneNumber Samhall { get; } = Primitives.Contact.PhoneNumber.Parse("+4620572572");
    public static Primitives.Contact.PhoneNumber StockholmsKommun { get; } = Primitives.Contact.PhoneNumber.Parse("+46850829000");
    public static Primitives.Contact.PhoneNumber SvenskaKyrkan { get; } = Primitives.Contact.PhoneNumber.Parse("+4618169500");
    public static Primitives.Contact.PhoneNumber Folksam { get; } = Primitives.Contact.PhoneNumber.Parse("+46771950950");
    public static Primitives.Contact.PhoneNumber KooperativaForbundet { get; } = Primitives.Contact.PhoneNumber.Parse("+46107400000");

    public static Primitives.Contact.PhoneNumber SwedishMobileTest { get; } = Primitives.Contact.PhoneNumber.Parse("+46701740605");
    public static Primitives.Contact.PhoneNumber SwedishFixedTest { get; } = Primitives.Contact.PhoneNumber.Parse("+4681234567");

    public static IReadOnlyList<Primitives.Contact.PhoneNumber> All { get; } =
    [
        Systembolaget, SJ, Vattenfall, Samhall, StockholmsKommun,
        SvenskaKyrkan, Folksam, KooperativaForbundet,
        SwedishMobileTest, SwedishFixedTest,
    ];
}
