using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Czech address (<c>poštovní adresa</c>) — a complete postal address within Czechia.
/// Requires a street, a 5-digit Czech zip code (displayed as <c>NNN NN</c>), and a city.
/// Rejects addresses with an explicitly non-Czech country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.ceskaposta.cz/">Česká pošta</see> — Czech postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_the_Czech_Republic">Wikipedia — Postal codes in the Czech Republic</see></description></item>
/// </list>
/// </remarks>
public sealed class CzechAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Czech Address", "Tjeckisk adress", "🇨🇿", ["https://www.ceskaposta.cz/", "https://en.wikipedia.org/wiki/Postal_codes_in_the_Czech_Republic"]);

    public CzechAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private CzechAddress(AddressStreet street, CzechAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out CzechAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out CzechAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out CzechAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static CzechAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Czech address.", nameof(input));
        return result!;
    }

    public static CzechAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Czech address.", nameof(street));
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

    private static bool TryWrap(Address address, out CzechAddress? result)
        => TryWrapCore<CzechAddress, CzechAddressZipCode>(address, "CZ", Country.CzechRepublic,
            CzechAddressZipCode.TryParse,
            (s, z, c, a) => new CzechAddress(s, z, c, a),
            out result);
}
