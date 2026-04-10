using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A German address (<c>Postanschrift</c>) — a complete postal address within Germany.
/// Requires a street, a 5-digit German zip code (<c>Postleitzahl</c>), and a city.
/// Rejects addresses with an explicitly non-German country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.deutschepost.de/">Deutsche Post</see> — German postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Germany">Wikipedia — Postal codes in Germany</see> — format NNNNN, 5 digits</description></item>
/// </list>
/// </remarks>
public sealed class GermanAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("German Address", "Tysk adress", "🇩🇪", ["https://www.deutschepost.de/", "https://en.wikipedia.org/wiki/Postal_codes_in_Germany"]);

    public GermanAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private GermanAddress(AddressStreet street, GermanAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out GermanAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out GermanAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out GermanAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static GermanAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid German address.", nameof(input));
        return result!;
    }

    public static GermanAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid German address.", nameof(street));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    private static bool TryWrap(Address address, out GermanAddress? result)
        => TryWrapCore<GermanAddress, GermanAddressZipCode>(address, "DE", Country.Germany,
            GermanAddressZipCode.TryParse,
            (s, z, c, a) => new GermanAddress(s, z, c, a),
            out result);
}
