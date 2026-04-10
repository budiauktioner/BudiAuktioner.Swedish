using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Swiss address (<c>Postanschrift</c> / <c>adresse postale</c> / <c>indirizzo postale</c>) —
/// a complete postal address within Switzerland.
/// Requires a street, a 4-digit Swiss zip code, and a city.
/// Rejects addresses with an explicitly non-Swiss country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.post.ch/">Swiss Post</see> — Swiss postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Switzerland_and_Liechtenstein">Wikipedia — Postal codes in Switzerland</see></description></item>
/// </list>
/// </remarks>
public sealed class SwissAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Swiss Address", "Schweizisk adress", "🇨🇭", ["https://www.post.ch/", "https://en.wikipedia.org/wiki/Postal_codes_in_Switzerland_and_Liechtenstein"]);

    public SwissAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private SwissAddress(AddressStreet street, SwissAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out SwissAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out SwissAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out SwissAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static SwissAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swiss address.", nameof(input));
        return result!;
    }

    public static SwissAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Swiss address.", nameof(street));
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

    private static bool TryWrap(Address address, out SwissAddress? result)
        => TryWrapCore<SwissAddress, SwissAddressZipCode>(address, "CH", Country.Switzerland,
            SwissAddressZipCode.TryParse,
            (s, z, c, a) => new SwissAddress(s, z, c, a),
            out result);
}
