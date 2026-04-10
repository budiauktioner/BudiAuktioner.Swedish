using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// An Estonian address (<c>postiaadress</c>) — a complete postal address within Estonia.
/// Requires a street, a 5-digit Estonian zip code, and a city.
/// Rejects addresses with an explicitly non-Estonian country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.omniva.ee/">Omniva</see> — Estonian postal service</description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Postnummer_i_Estland">Wikipedia — Postnummer i Estland</see> — format NNNNN, 5 digits</description></item>
/// </list>
/// </remarks>
public sealed class EstonianAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Estonian Address", "Estnisk adress", "🇪🇪", ["https://www.omniva.ee/", "https://sv.wikipedia.org/wiki/Postnummer_i_Estland"]);

    public EstonianAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private EstonianAddress(AddressStreet street, EstonianAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out EstonianAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out EstonianAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out EstonianAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static EstonianAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Estonian address.", nameof(input));
        return result!;
    }

    public static EstonianAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Estonian address.", nameof(street));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim()
        : null;

    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    private static bool TryWrap(Address address, out EstonianAddress? result)
        => TryWrapCore<EstonianAddress, EstonianAddressZipCode>(address, "EE", Country.Estonia,
            EstonianAddressZipCode.TryParse,
            (s, z, c, a) => new EstonianAddress(s, z, c, a),
            out result);
}
