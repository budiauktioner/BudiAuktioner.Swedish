using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Dutch address (<c>postadres</c>) — a complete postal address within the Netherlands.
/// Requires a street, a Dutch zip code (<c>NNNN AA</c>), and a city.
/// Rejects addresses with an explicitly non-Dutch country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnl.nl/">PostNL</see> — Dutch postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_the_Netherlands">Wikipedia — Postal codes in the Netherlands</see></description></item>
/// </list>
/// </remarks>
public sealed class DutchAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Dutch Address", "Nederländsk adress", "🇳🇱", ["https://www.postnl.nl/", "https://en.wikipedia.org/wiki/Postal_codes_in_the_Netherlands"]);

    public DutchAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private DutchAddress(AddressStreet street, DutchAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out DutchAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out DutchAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out DutchAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static DutchAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Dutch address.", nameof(input));
        return result!;
    }

    public static DutchAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Dutch address.", nameof(street));
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

    private static bool TryWrap(Address address, out DutchAddress? result)
        => TryWrapCore<DutchAddress, DutchAddressZipCode>(address, "NL", Country.Netherlands,
            DutchAddressZipCode.TryParse,
            (s, z, c, a) => new DutchAddress(s, z, c, a),
            out result);
}
