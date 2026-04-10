namespace Buildi.Primitives.Organization;

/// <summary>
/// Extension methods that produce public lookup URLs for organization identifiers.
/// </summary>
public static class OrganizationLookupExtensions
{
    /// <summary>
    /// Returns an Allabolag.se lookup URL, e.g.
    /// <c>https://www.allabolag.se/5560125790</c>.
    /// </summary>
    public static string ToAllabolagUrl(this SwedishOrganizationNumber orgNumber) =>
        $"https://www.allabolag.se/{orgNumber.ToNormalizedString()}";

    /// <summary>
    /// Returns an EU VIES validation URL for the VAT number, e.g.
    /// <c>https://ec.europa.eu/taxation_customs/vies/#/vat-validation/SE556012579001</c>.
    /// </summary>
    public static string ToViesUrl(this EuVatNumber vat) =>
        $"https://ec.europa.eu/taxation_customs/vies/#/vat-validation/{vat.ToNormalizedString()}";

    /// <summary>
    /// Returns a GLEIF record lookup URL, e.g.
    /// <c>https://search.gleif.org/#/record/549300MLUDYVRQOOXS22</c>.
    /// </summary>
    public static string ToGleifUrl(this LeiCode lei) =>
        $"https://search.gleif.org/#/record/{lei.ToNormalizedString()}";
}
