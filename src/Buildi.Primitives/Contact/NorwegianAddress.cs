using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Norwegian address (<c>postadresse</c>) — a complete postal address within Norway.
/// Requires a street, a 4-digit Norwegian zip code, and a city.
/// Rejects addresses with an explicitly non-Norwegian country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.posten.no/">Posten Norge</see> — Norwegian postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Norway">Wikipedia — Postal codes in Norway</see> — format NNNN, 4 digits</description></item>
/// </list>
/// </remarks>
public sealed class NorwegianAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Norwegian Address", "Norsk adress", "🇳🇴", ["https://www.posten.no/", "https://en.wikipedia.org/wiki/Postal_codes_in_Norway"]);

    public NorwegianAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private NorwegianAddress(AddressStreet street, NorwegianAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out NorwegianAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out NorwegianAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out NorwegianAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static NorwegianAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Norwegian address.", nameof(input));
        return result!;
    }

    public static NorwegianAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Norwegian address.", nameof(street));
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

    private static bool TryWrap(Address address, out NorwegianAddress? result)
        => TryWrapCore<NorwegianAddress, NorwegianAddressZipCode>(address, "NO", Country.Norway,
            NorwegianAddressZipCode.TryParse,
            (s, z, c, a) => new NorwegianAddress(s, z, c, a),
            out result);
}
