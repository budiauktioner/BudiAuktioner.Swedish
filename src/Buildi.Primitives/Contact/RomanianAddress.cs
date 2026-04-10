using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Romanian address (<c>adresă poștală</c>) — a complete postal address within Romania.
/// Requires a street, a 6-digit Romanian zip code, and a city.
/// Rejects addresses with an explicitly non-Romanian country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.posta-romana.ro/">Poșta Română</see> — Romanian postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Romania">Wikipedia — Postal codes in Romania</see> — format NNNNNN, 6 digits</description></item>
/// </list>
/// </remarks>
public sealed class RomanianAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Romanian Address", "Rumänsk adress", "🇷🇴", ["https://www.posta-romana.ro/", "https://en.wikipedia.org/wiki/Postal_codes_in_Romania"]);

    public RomanianAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private RomanianAddress(AddressStreet street, RomanianAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out RomanianAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out RomanianAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out RomanianAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static RomanianAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Romanian address.", nameof(input));
        return result!;
    }

    public static RomanianAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Romanian address.", nameof(street));
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

    private static bool TryWrap(Address address, out RomanianAddress? result)
        => TryWrapCore<RomanianAddress, RomanianAddressZipCode>(address, "RO", Country.Romania,
            RomanianAddressZipCode.TryParse,
            (s, z, c, a) => new RomanianAddress(s, z, c, a),
            out result);
}
