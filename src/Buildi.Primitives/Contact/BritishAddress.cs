using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A British address — a complete postal address within the United Kingdom.
/// Requires a street, a valid UK postcode, and a city.
/// Rejects addresses with an explicitly non-UK country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.royalmail.com/">Royal Mail</see> — UK postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postcodes_in_the_United_Kingdom">Wikipedia — Postcodes in the United Kingdom</see></description></item>
/// </list>
/// </remarks>
public sealed class BritishAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("British Address", "Brittisk adress", "🇬🇧", ["https://www.royalmail.com/", "https://en.wikipedia.org/wiki/Postcodes_in_the_United_Kingdom"]);

    public BritishAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private BritishAddress(AddressStreet street, BritishAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out BritishAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out BritishAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out BritishAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static BritishAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid British address.", nameof(input));
        return result!;
    }

    public static BritishAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid British address.", nameof(street));
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

    private static bool TryWrap(Address address, out BritishAddress? result)
        => TryWrapCore<BritishAddress, BritishAddressZipCode>(address, "GB", Country.UnitedKingdom,
            BritishAddressZipCode.TryParse,
            (s, z, c, a) => new BritishAddress(s, z, c, a),
            out result);
}
