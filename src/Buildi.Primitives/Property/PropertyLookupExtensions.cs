namespace Buildi.Primitives.Property;

/// <summary>
/// Extension methods that produce public lookup URLs for property types.
/// </summary>
public static class PropertyLookupExtensions
{
    /// <summary>
    /// Returns a Hitta.se map search URL for the property designation, e.g.
    /// <c>https://www.hitta.se/kartan?s=Stockholm+S%C3%B6der+75%3A2</c>.
    /// </summary>
    public static string ToHittaUrl(this SwedishPropertyDesignation designation) =>
        $"https://www.hitta.se/kartan?s={Uri.EscapeDataString(designation.ToString())}";
}
