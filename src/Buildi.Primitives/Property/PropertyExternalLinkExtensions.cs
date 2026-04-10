namespace Buildi.Primitives.Property;

/// <summary>
/// Extension methods that generate lookup URLs for property-related types.
/// </summary>
public static class PropertyExternalLinkExtensions
{
    /// <summary>
    /// Returns a Lantmäteriet (Swedish mapping authority) search URL for this property designation,
    /// e.g. <c>https://minkarta.lantmateriet.se/?e=...&amp;search=Stockholm+Söder+75:2</c>.
    /// </summary>
    public static Uri GetLantmaterietUrl(this SwedishPropertyDesignation prop)
        => new($"https://minkarta.lantmateriet.se/?search={Uri.EscapeDataString(prop.Value)}");

    /// <summary>
    /// Returns a Hitta.se property search URL for this property designation,
    /// e.g. <c>https://www.hitta.se/sök?vad=Stockholm+Söder+75:2</c>.
    /// </summary>
    public static Uri GetHittaUrl(this SwedishPropertyDesignation prop)
        => new($"https://www.hitta.se/sök?vad={Uri.EscapeDataString(prop.Value)}");
}
