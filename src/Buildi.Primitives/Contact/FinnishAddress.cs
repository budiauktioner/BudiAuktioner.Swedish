using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Finnish address (<c>postiosoite</c>) — a complete postal address within Finland.
/// Requires a street, a 5-digit Finnish zip code, and a city.
/// Rejects addresses with an explicitly non-Finnish country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.posti.fi/">Posti</see> — Finnish postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Finland">Wikipedia — Postal codes in Finland</see> — format NNNNN, 5 digits</description></item>
/// </list>
/// </remarks>
public sealed class FinnishAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Finnish Address", "Finsk adress", "🇫🇮", ["https://www.posti.fi/", "https://en.wikipedia.org/wiki/Postal_codes_in_Finland"]);

    public FinnishAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private FinnishAddress(AddressStreet street, FinnishAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out FinnishAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out FinnishAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out FinnishAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static FinnishAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Finnish address.", nameof(input));
        return result!;
    }

    public static FinnishAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Finnish address.", nameof(street));
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

    private static bool TryWrap(Address address, out FinnishAddress? result)
        => TryWrapCore<FinnishAddress, FinnishAddressZipCode>(address, "FI", Country.Finland,
            FinnishAddressZipCode.TryParse,
            (s, z, c, a) => new FinnishAddress(s, z, c, a),
            out result);
}
