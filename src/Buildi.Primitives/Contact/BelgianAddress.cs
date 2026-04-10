using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Belgian address (<c>postadres</c> / <c>adresse postale</c>) — a complete postal address within Belgium.
/// Requires a street, a 4-digit Belgian zip code, and a city.
/// Rejects addresses with an explicitly non-Belgian country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bpost.be/">bpost</see> — Belgian postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Belgium">Wikipedia — Postal codes in Belgium</see></description></item>
/// </list>
/// </remarks>
public sealed class BelgianAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Belgian Address", "Belgisk adress", "🇧🇪", ["https://www.bpost.be/", "https://en.wikipedia.org/wiki/Postal_codes_in_Belgium"]);

    public BelgianAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private BelgianAddress(AddressStreet street, BelgianAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out BelgianAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out BelgianAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out BelgianAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static BelgianAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Belgian address.", nameof(input));
        return result!;
    }

    public static BelgianAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Belgian address.", nameof(street));
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

    private static bool TryWrap(Address address, out BelgianAddress? result)
        => TryWrapCore<BelgianAddress, BelgianAddressZipCode>(address, "BE", Country.Belgium,
            BelgianAddressZipCode.TryParse,
            (s, z, c, a) => new BelgianAddress(s, z, c, a),
            out result);
}
