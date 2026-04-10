using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Cypriot address (<c>ταχυδρομική διεύθυνση</c>) — a complete postal address within Cyprus.
/// Requires a street, a 4-digit Cypriot zip code, and a city.
/// Rejects addresses with an explicitly non-Cypriot country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gov.cy/mtcw/en/department-of-postal-services/">Department of Postal Services</see> — Cypriot postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Cyprus">Wikipedia — Postal codes in Cyprus</see></description></item>
/// </list>
/// </remarks>
public sealed class CypriotAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Cypriot Address", "Cypriotisk adress", "🇨🇾", ["https://www.gov.cy/mtcw/en/department-of-postal-services/", "https://en.wikipedia.org/wiki/Postal_codes_in_Cyprus"]);

    public CypriotAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private CypriotAddress(AddressStreet street, CypriotAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out CypriotAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out CypriotAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out CypriotAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static CypriotAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Cypriot address.", nameof(input));
        return result!;
    }

    public static CypriotAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Cypriot address.", nameof(street));
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

    private static bool TryWrap(Address address, out CypriotAddress? result)
        => TryWrapCore<CypriotAddress, CypriotAddressZipCode>(address, "CY", Country.Cyprus,
            CypriotAddressZipCode.TryParse,
            (s, z, c, a) => new CypriotAddress(s, z, c, a),
            out result);
}
