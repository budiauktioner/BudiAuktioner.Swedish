using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// Extension methods that produce actionable or lookup URLs for contact types.
/// </summary>
public static class ContactLookupExtensions
{
    /// <summary>
    /// Returns a <c>tel:</c> URI, e.g. <c>tel:+46701234567</c>.
    /// </summary>
    public static string ToTelUri(this PhoneNumber phone) =>
        $"tel:{phone.ToNormalizedString()}";

    /// <summary>
    /// Returns a Google Maps search URL for the address, e.g.
    /// <c>https://www.google.com/maps/search/?api=1&amp;query=Storgatan+12%2C+114+53+Stockholm</c>.
    /// </summary>
    public static string ToGoogleMapsUrl(this Address address) =>
        $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(address.ToString())}";

    /// <summary>
    /// Returns a Google Maps search URL for the Swedish address, e.g.
    /// <c>https://www.google.com/maps/search/?api=1&amp;query=Storgatan+1%2C+114+53+Stockholm</c>.
    /// </summary>
    public static string ToGoogleMapsUrl(this SwedishAddress address) =>
        $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(address.ToString())}";

    /// <summary>
    /// Returns a Swedish Wikipedia URL for the country, e.g.
    /// <c>https://sv.wikipedia.org/wiki/Sverige</c>.
    /// </summary>
    public static string ToWikipediaUrl(this Country country) =>
        $"https://sv.wikipedia.org/wiki/{Uri.EscapeDataString(country.LocalizedName)}";
}
