using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Luxembourgish address (<c>adresse postale</c>) — a complete postal address within Luxembourg.
/// Requires a street, a 4-digit Luxembourgish zip code, and a city.
/// Rejects addresses with an explicitly non-Luxembourgish country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.post.lu/">POST Luxembourg</see> — Luxembourgish postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Luxembourg">Wikipedia — Postal codes in Luxembourg</see></description></item>
/// </list>
/// </remarks>
public sealed class LuxembourgishAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Luxembourgish Address", "Luxemburgsk adress", "🇱🇺", ["https://www.post.lu/", "https://en.wikipedia.org/wiki/Postal_codes_in_Luxembourg"]);

    public LuxembourgishAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private LuxembourgishAddress(AddressStreet street, LuxembourgishAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out LuxembourgishAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out LuxembourgishAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out LuxembourgishAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static LuxembourgishAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Luxembourgish address.", nameof(input));
        return result!;
    }

    public static LuxembourgishAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Luxembourgish address.", nameof(street));
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

    private static bool TryWrap(Address address, out LuxembourgishAddress? result)
        => TryWrapCore<LuxembourgishAddress, LuxembourgishAddressZipCode>(address, "LU", Country.Luxembourg,
            LuxembourgishAddressZipCode.TryParse,
            (s, z, c, a) => new LuxembourgishAddress(s, z, c, a),
            out result);
}
