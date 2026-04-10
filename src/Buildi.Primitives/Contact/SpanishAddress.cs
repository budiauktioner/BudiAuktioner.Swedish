using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Spanish address (<c>dirección postal</c>) — a complete postal address within Spain.
/// Requires a street, a 5-digit Spanish zip code, and a city.
/// Rejects addresses with an explicitly non-Spanish country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.correos.es/">Correos</see> — Spanish postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Spain">Wikipedia — Postal codes in Spain</see></description></item>
/// </list>
/// </remarks>
public sealed class SpanishAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Spanish Address", "Spansk adress", "🇪🇸", ["https://www.correos.es/", "https://en.wikipedia.org/wiki/Postal_codes_in_Spain"]);

    public SpanishAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private SpanishAddress(AddressStreet street, SpanishAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out SpanishAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out SpanishAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out SpanishAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static SpanishAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Spanish address.", nameof(input));
        return result!;
    }

    public static SpanishAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Spanish address.", nameof(street));
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

    private static bool TryWrap(Address address, out SpanishAddress? result)
        => TryWrapCore<SpanishAddress, SpanishAddressZipCode>(address, "ES", Country.Spain,
            SpanishAddressZipCode.TryParse,
            (s, z, c, a) => new SpanishAddress(s, z, c, a),
            out result);
}
