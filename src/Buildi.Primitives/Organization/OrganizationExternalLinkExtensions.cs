namespace Buildi.Primitives.Organization;

/// <summary>
/// Extension methods that generate lookup URLs for organization-related types.
/// </summary>
public static class OrganizationExternalLinkExtensions
{
    /// <summary>
    /// Returns the Bolagsverket (Swedish Companies Registration Office) company information URL,
    /// e.g. <c>https://foretagsinfo.bolagsverket.se/sok-foretagsinformation-web/foretag/559246-0421</c>.
    /// </summary>
    public static Uri GetBolagsverketUrl(this SwedishOrganizationNumber org)
        => new($"https://foretagsinfo.bolagsverket.se/sok-foretagsinformation-web/foretag/{org.To10DigitString()}");

    /// <summary>
    /// Returns the Allabolag.se company information URL,
    /// e.g. <c>https://www.allabolag.se/5592460421</c>.
    /// </summary>
    public static Uri GetAllabolagUrl(this SwedishOrganizationNumber org)
        => new($"https://www.allabolag.se/{org.To10DigitsOnly()}");

    /// <summary>
    /// Returns the GLEIF (Global Legal Entity Identifier Foundation) record URL,
    /// e.g. <c>https://search.gleif.org/#/record/5493001KJTIIGC8Y1R12</c>.
    /// </summary>
    public static Uri GetGleifUrl(this LeiCode lei)
        => new($"https://search.gleif.org/#/record/{lei.Value}");
}
