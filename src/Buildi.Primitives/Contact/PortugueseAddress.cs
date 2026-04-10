using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Portuguese address (<c>morada</c>) — a complete postal address within Portugal.
/// Requires a street, a 7-digit Portuguese zip code (<c>NNNN-NNN</c>), and a city.
/// Rejects addresses with an explicitly non-Portuguese country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.ctt.pt/">CTT — Correios de Portugal</see> — Portuguese postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Portugal">Wikipedia — Postal codes in Portugal</see></description></item>
/// </list>
/// </remarks>
public sealed class PortugueseAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Portuguese Address", "Portugisisk adress", "🇵🇹", ["https://www.ctt.pt/", "https://en.wikipedia.org/wiki/Postal_codes_in_Portugal"]);

    public PortugueseAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private PortugueseAddress(AddressStreet street, PortugueseAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out PortugueseAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out PortugueseAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out PortugueseAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static PortugueseAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Portuguese address.", nameof(input));
        return result!;
    }

    public static PortugueseAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Portuguese address.", nameof(street));
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

    private static bool TryWrap(Address address, out PortugueseAddress? result)
        => TryWrapCore<PortugueseAddress, PortugueseAddressZipCode>(address, "PT", Country.Portugal,
            PortugueseAddressZipCode.TryParse,
            (s, z, c, a) => new PortugueseAddress(s, z, c, a),
            out result);
}
