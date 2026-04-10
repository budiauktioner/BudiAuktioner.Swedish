namespace Buildi.Primitives.SampleData.Web;

/// <summary>
/// Valid <see cref="Primitives.Web.EmailAddress"/> values from publicly known Swedish organizations
/// and RFC 2606 reserved domains.
/// </summary>
public static class EmailSampleData
{
    public static Primitives.Web.EmailAddress Samhall { get; } = Primitives.Web.EmailAddress.Parse("kontakt@samhall.se");
    public static Primitives.Web.EmailAddress LKAB { get; } = Primitives.Web.EmailAddress.Parse("redovisning@lkab.com");

    public static Primitives.Web.EmailAddress Example { get; } = Primitives.Web.EmailAddress.Parse("user@example.com");
    public static Primitives.Web.EmailAddress ExampleOrg { get; } = Primitives.Web.EmailAddress.Parse("info@example.org");
    public static IReadOnlyList<Primitives.Web.EmailAddress> All { get; } = [Samhall, LKAB, Example, ExampleOrg];
}
