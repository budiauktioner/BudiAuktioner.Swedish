using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Maltese address — a complete postal address within Malta.
/// Requires a street, a Maltese zip code (<c>AAA NNNN</c>), and a city.
/// Rejects addresses with an explicitly non-Maltese country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.maltapost.com/">MaltaPost</see> — Maltese postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Malta">Wikipedia — Postal codes in Malta</see></description></item>
/// </list>
/// </remarks>
public sealed class MalteseAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Maltese Address", "Maltesisk adress", "🇲🇹", ["https://www.maltapost.com/", "https://en.wikipedia.org/wiki/Postal_codes_in_Malta"]);

    public MalteseAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private MalteseAddress(AddressStreet street, MalteseAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out MalteseAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out MalteseAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out MalteseAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static MalteseAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Maltese address.", nameof(input));
        return result!;
    }

    public static MalteseAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Maltese address.", nameof(street));
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

    private static bool TryWrap(Address address, out MalteseAddress? result)
        => TryWrapCore<MalteseAddress, MalteseAddressZipCode>(address, "MT", Country.Malta,
            MalteseAddressZipCode.TryParse,
            (s, z, c, a) => new MalteseAddress(s, z, c, a),
            out result);
}
