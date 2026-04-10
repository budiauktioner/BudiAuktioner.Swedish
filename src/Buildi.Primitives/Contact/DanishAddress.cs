using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Danish address (<c>postadresse</c>) — a complete postal address within Denmark.
/// Requires a street, a 4-digit Danish zip code, and a city.
/// Rejects addresses with an explicitly non-Danish country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnord.dk/">PostNord Denmark</see> — Danish postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Denmark">Wikipedia — Postal codes in Denmark</see> — format NNNN, 4 digits</description></item>
/// </list>
/// </remarks>
public sealed class DanishAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Danish Address", "Dansk adress", "🇩🇰", ["https://www.postnord.dk/", "https://en.wikipedia.org/wiki/Postal_codes_in_Denmark"]);

    public DanishAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private DanishAddress(AddressStreet street, DanishAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out DanishAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out DanishAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out DanishAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static DanishAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Danish address.", nameof(input));
        return result!;
    }

    public static DanishAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Danish address.", nameof(street));
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

    private static bool TryWrap(Address address, out DanishAddress? result)
        => TryWrapCore<DanishAddress, DanishAddressZipCode>(address, "DK", Country.Denmark,
            DanishAddressZipCode.TryParse,
            (s, z, c, a) => new DanishAddress(s, z, c, a),
            out result);
}
