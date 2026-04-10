using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// An Icelandic address (<c>póstfang</c>) — a complete postal address within Iceland.
/// Requires a street, a 3-digit Icelandic zip code, and a city.
/// Rejects addresses with an explicitly non-Icelandic country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postur.is/">Íslandspóstur</see> — Icelandic postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Iceland">Wikipedia — Postal codes in Iceland</see></description></item>
/// </list>
/// </remarks>
public sealed class IcelandicAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Icelandic Address", "Isländsk adress", "🇮🇸", ["https://www.postur.is/", "https://en.wikipedia.org/wiki/Postal_codes_in_Iceland"]);

    public IcelandicAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private IcelandicAddress(AddressStreet street, IcelandicAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out IcelandicAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out IcelandicAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out IcelandicAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static IcelandicAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Icelandic address.", nameof(input));
        return result!;
    }

    public static IcelandicAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Icelandic address.", nameof(street));
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

    private static bool TryWrap(Address address, out IcelandicAddress? result)
        => TryWrapCore<IcelandicAddress, IcelandicAddressZipCode>(address, "IS", Country.Iceland,
            IcelandicAddressZipCode.TryParse,
            (s, z, c, a) => new IcelandicAddress(s, z, c, a),
            out result);
}
