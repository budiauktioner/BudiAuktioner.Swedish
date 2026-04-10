using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Liechtenstein address (<c>Postanschrift</c>) — a complete postal address within Liechtenstein.
/// Requires a street, a 4-digit Liechtenstein zip code (94xx range), and a city.
/// Rejects addresses with an explicitly non-Liechtenstein country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.post.li/">Liechtensteinische Post</see> — Liechtenstein postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Switzerland_and_Liechtenstein">Wikipedia — Postal codes in Switzerland and Liechtenstein</see></description></item>
/// </list>
/// </remarks>
public sealed class LiechtensteinAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Liechtenstein Address", "Liechtensteinsk adress", "🇱🇮", ["https://www.post.li/", "https://en.wikipedia.org/wiki/Postal_codes_in_Switzerland_and_Liechtenstein"]);

    public LiechtensteinAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private LiechtensteinAddress(AddressStreet street, LiechtensteinAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out LiechtensteinAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out LiechtensteinAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out LiechtensteinAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static LiechtensteinAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Liechtenstein address.", nameof(input));
        return result!;
    }

    public static LiechtensteinAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Liechtenstein address.", nameof(street));
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

    private static bool TryWrap(Address address, out LiechtensteinAddress? result)
        => TryWrapCore<LiechtensteinAddress, LiechtensteinAddressZipCode>(address, "LI", Country.Liechtenstein,
            LiechtensteinAddressZipCode.TryParse,
            (s, z, c, a) => new LiechtensteinAddress(s, z, c, a),
            out result);
}
