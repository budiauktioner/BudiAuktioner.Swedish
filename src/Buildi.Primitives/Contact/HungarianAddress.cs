using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Hungarian address (<c>postai cím</c>) — a complete postal address within Hungary.
/// Requires a street, a 4-digit Hungarian zip code, and a city.
/// Rejects addresses with an explicitly non-Hungarian country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.posta.hu/">Magyar Posta</see> — Hungarian postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Hungary">Wikipedia — Postal codes in Hungary</see></description></item>
/// </list>
/// </remarks>
public sealed class HungarianAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Hungarian Address", "Ungersk adress", "🇭🇺", ["https://www.posta.hu/", "https://en.wikipedia.org/wiki/Postal_codes_in_Hungary"]);

    public HungarianAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private HungarianAddress(AddressStreet street, HungarianAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out HungarianAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out HungarianAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out HungarianAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static HungarianAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Hungarian address.", nameof(input));
        return result!;
    }

    public static HungarianAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Hungarian address.", nameof(street));
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

    private static bool TryWrap(Address address, out HungarianAddress? result)
        => TryWrapCore<HungarianAddress, HungarianAddressZipCode>(address, "HU", Country.Hungary,
            HungarianAddressZipCode.TryParse,
            (s, z, c, a) => new HungarianAddress(s, z, c, a),
            out result);
}
