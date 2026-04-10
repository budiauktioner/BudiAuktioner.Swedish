using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Greek address (<c>ταχυδρομική διεύθυνση</c>) — a complete postal address within Greece.
/// Requires a street, a 5-digit Greek zip code (displayed as <c>NNN NN</c>), and a city.
/// Rejects addresses with an explicitly non-Greek country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.elta.gr/">ELTA</see> — Hellenic Post</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Greece">Wikipedia — Postal codes in Greece</see></description></item>
/// </list>
/// </remarks>
public sealed class GreekAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Greek Address", "Grekisk adress", "🇬🇷", ["https://www.elta.gr/", "https://en.wikipedia.org/wiki/Postal_codes_in_Greece"]);

    public GreekAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private GreekAddress(AddressStreet street, GreekAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out GreekAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out GreekAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out GreekAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static GreekAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Greek address.", nameof(input));
        return result!;
    }

    public static GreekAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Greek address.", nameof(street));
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

    private static bool TryWrap(Address address, out GreekAddress? result)
        => TryWrapCore<GreekAddress, GreekAddressZipCode>(address, "GR", Country.Greece,
            GreekAddressZipCode.TryParse,
            (s, z, c, a) => new GreekAddress(s, z, c, a),
            out result);
}
