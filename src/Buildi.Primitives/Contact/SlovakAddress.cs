using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Slovak address (<c>poštová adresa</c>) — a complete postal address within Slovakia.
/// Requires a street, a 5-digit Slovak zip code (displayed as <c>NNN NN</c>), and a city.
/// Rejects addresses with an explicitly non-Slovak country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.posta.sk/">Slovenská pošta</see> — Slovak postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Slovakia">Wikipedia — Postal codes in Slovakia</see></description></item>
/// </list>
/// </remarks>
public sealed class SlovakAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Slovak Address", "Slovakisk adress", "🇸🇰", ["https://www.posta.sk/", "https://en.wikipedia.org/wiki/Postal_codes_in_Slovakia"]);

    public SlovakAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private SlovakAddress(AddressStreet street, SlovakAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out SlovakAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out SlovakAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out SlovakAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static SlovakAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Slovak address.", nameof(input));
        return result!;
    }

    public static SlovakAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Slovak address.", nameof(street));
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

    private static bool TryWrap(Address address, out SlovakAddress? result)
        => TryWrapCore<SlovakAddress, SlovakAddressZipCode>(address, "SK", Country.Slovakia,
            SlovakAddressZipCode.TryParse,
            (s, z, c, a) => new SlovakAddress(s, z, c, a),
            out result);
}
