using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A French address (<c>adresse postale</c>) — a complete postal address within France.
/// Requires a street, a 5-digit French zip code, and a city.
/// Rejects addresses with an explicitly non-French country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.laposte.fr/">La Poste</see> — French postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_France">Wikipedia — Postal codes in France</see></description></item>
/// </list>
/// </remarks>
public sealed class FrenchAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("French Address", "Fransk adress", "🇫🇷", ["https://www.laposte.fr/", "https://en.wikipedia.org/wiki/Postal_codes_in_France"]);

    public FrenchAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private FrenchAddress(AddressStreet street, FrenchAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out FrenchAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out FrenchAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out FrenchAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static FrenchAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid French address.", nameof(input));
        return result!;
    }

    public static FrenchAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid French address.", nameof(street));
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

    private static bool TryWrap(Address address, out FrenchAddress? result)
        => TryWrapCore<FrenchAddress, FrenchAddressZipCode>(address, "FR", Country.France,
            FrenchAddressZipCode.TryParse,
            (s, z, c, a) => new FrenchAddress(s, z, c, a),
            out result);
}
