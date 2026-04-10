namespace Buildi.Primitives.Geography;

/// <summary>
/// Extension methods that generate lookup URLs for geography-related types.
/// </summary>
public static class GeographyExternalLinkExtensions
{
    /// <summary>
    /// Returns an SCB regional statistics URL for this county,
    /// e.g. <c>https://www.scb.se/hitta-statistik/regional-statistik-och-kartor/regionala-indelningar/lan-och-kommuner/</c>.
    /// </summary>
    public static Uri GetScbUrl(this SwedishCounty county)
        => new("https://www.scb.se/hitta-statistik/regional-statistik-och-kartor/regionala-indelningar/lan-och-kommuner/");

    /// <summary>
    /// Returns a Swedish Wikipedia URL for this county,
    /// e.g. <c>https://sv.wikipedia.org/wiki/Stockholms_län</c>.
    /// </summary>
    public static Uri GetWikipediaUrl(this SwedishCounty county)
        => new($"https://sv.wikipedia.org/wiki/{Uri.EscapeDataString(county.LocalizedName.Replace(' ', '_'))}");

    /// <summary>
    /// Returns a Google Maps search URL for this county,
    /// e.g. <c>https://www.google.com/maps/search/?api=1&amp;query=Stockholms+län</c>.
    /// </summary>
    public static Uri GetGoogleMapsUrl(this SwedishCounty county)
        => new($"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(county.LocalizedName)}");

    /// <summary>
    /// Returns an SCB regional statistics URL for this municipality,
    /// e.g. <c>https://www.scb.se/hitta-statistik/regional-statistik-och-kartor/regionala-indelningar/lan-och-kommuner/</c>.
    /// </summary>
    public static Uri GetScbUrl(this SwedishMunicipality municipality)
        => new("https://www.scb.se/hitta-statistik/regional-statistik-och-kartor/regionala-indelningar/lan-och-kommuner/");

    /// <summary>
    /// Returns a Swedish Wikipedia URL for this municipality,
    /// e.g. <c>https://sv.wikipedia.org/wiki/Stockholms_kommun</c>.
    /// </summary>
    public static Uri GetWikipediaUrl(this SwedishMunicipality municipality)
        => new($"https://sv.wikipedia.org/wiki/{Uri.EscapeDataString($"{municipality.LocalizedName}s kommun".Replace(' ', '_'))}");

    /// <summary>
    /// Returns a Google Maps search URL for this municipality,
    /// e.g. <c>https://www.google.com/maps/search/?api=1&amp;query=Stockholm+kommun</c>.
    /// </summary>
    public static Uri GetGoogleMapsUrl(this SwedishMunicipality municipality)
        => new($"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString($"{municipality.LocalizedName} kommun")}");
}
