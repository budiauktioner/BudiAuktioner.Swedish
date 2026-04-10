namespace Buildi.Primitives.Geography;

/// <summary>
/// Extension methods that produce public lookup URLs for geographic types.
/// </summary>
public static class GeographyLookupExtensions
{
    /// <summary>
    /// Returns a Swedish Wikipedia URL for the municipality, e.g.
    /// <c>https://sv.wikipedia.org/wiki/Stockholms_kommun</c>.
    /// </summary>
    public static string ToWikipediaUrl(this SwedishMunicipality municipality) =>
        $"https://sv.wikipedia.org/wiki/{Uri.EscapeDataString(municipality.LocalizedName + " kommun")}";

    /// <summary>
    /// Returns a Swedish Wikipedia URL for the county, e.g.
    /// <c>https://sv.wikipedia.org/wiki/Stockholms_l%C3%A4n</c>.
    /// </summary>
    public static string ToWikipediaUrl(this SwedishCounty county) =>
        $"https://sv.wikipedia.org/wiki/{Uri.EscapeDataString(county.LocalizedName)}";
}
