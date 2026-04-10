namespace Buildi.Primitives.Contact;

/// <summary>
/// Extension methods that generate lookup URLs for contact-related types.
/// </summary>
public static class ContactExternalLinkExtensions
{
    /// <summary>
    /// Returns a Google Maps search URL for this address,
    /// e.g. <c>https://www.google.com/maps/search/?api=1&amp;query=Storgatan+12,+114+53+Stockholm</c>.
    /// </summary>
    public static Uri GetGoogleMapsUrl(this Address address)
        => new($"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(address.ToString())}");

    /// <summary>
    /// Returns a Bing Maps search URL for this address,
    /// e.g. <c>https://www.bing.com/maps?q=Storgatan+12,+114+53+Stockholm</c>.
    /// </summary>
    public static Uri GetBingMapsUrl(this Address address)
        => new($"https://www.bing.com/maps?q={Uri.EscapeDataString(address.ToString())}");

    /// <summary>
    /// Returns a Hitta.se search URL for this phone number,
    /// e.g. <c>https://www.hitta.se/sök?vad=0701740633</c>.
    /// </summary>
    public static Uri GetHittaUrl(this PhoneNumber phone)
        => new($"https://www.hitta.se/sök?vad={Uri.EscapeDataString(phone.ToInternationalString())}");
}
