namespace Buildi.Primitives.Web;

/// <summary>
/// Extension methods that produce actionable or lookup URLs for web-related types.
/// </summary>
public static class WebLookupExtensions
{
    /// <summary>
    /// Returns a <c>mailto:</c> URI, e.g. <c>mailto:info@example.com</c>.
    /// </summary>
    public static string ToMailtoUri(this EmailAddress email) =>
        $"mailto:{email.ToNormalizedString()}";

    /// <summary>
    /// Returns the normalized URL itself as its own lookup target.
    /// </summary>
    public static string ToNormalizedUrl(this Url url) =>
        url.ToNormalizedString();
}
