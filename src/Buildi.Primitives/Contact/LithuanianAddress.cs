using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Lithuanian address (<c>pašto adresas</c>) — a complete postal address within Lithuania.
/// Requires a street, a 5-digit Lithuanian zip code (displayed as <c>LT-NNNNN</c>), and a city.
/// Rejects addresses with an explicitly non-Lithuanian country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.post.lt/">Lietuvos paštas</see> — Lithuanian postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Lithuania">Wikipedia — Postal codes in Lithuania</see> — format LT-NNNNN, 5 digits</description></item>
/// </list>
/// </remarks>
public sealed class LithuanianAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Lithuanian Address", "Litauisk adress", "🇱🇹", ["https://www.post.lt/", "https://en.wikipedia.org/wiki/Postal_codes_in_Lithuania"]);

    public LithuanianAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private LithuanianAddress(AddressStreet street, LithuanianAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out LithuanianAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out LithuanianAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out LithuanianAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static LithuanianAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Lithuanian address.", nameof(input));
        return result!;
    }

    public static LithuanianAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Lithuanian address.", nameof(street));
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

    private static bool TryWrap(Address address, out LithuanianAddress? result)
        => TryWrapCore<LithuanianAddress, LithuanianAddressZipCode>(address, "LT", Country.Lithuania,
            LithuanianAddressZipCode.TryParse,
            (s, z, c, a) => new LithuanianAddress(s, z, c, a),
            out result);
}
