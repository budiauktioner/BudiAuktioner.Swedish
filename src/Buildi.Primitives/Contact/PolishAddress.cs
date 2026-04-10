using Buildi.Primitives;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Polish address (<c>adres pocztowy</c>) — a complete postal address within Poland.
/// Requires a street, a 5-digit Polish zip code (<c>NN-NNN</c>), and a city.
/// Rejects addresses with an explicitly non-Polish country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.poczta-polska.pl/">Poczta Polska</see> — Polish postal service</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Postal_codes_in_Poland">Wikipedia — Postal codes in Poland</see> — format NN-NNN, 5 digits</description></item>
/// </list>
/// </remarks>
public sealed class PolishAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Polish Address", "Polsk adress", "🇵🇱", ["https://www.poczta-polska.pl/", "https://en.wikipedia.org/wiki/Postal_codes_in_Poland"]);

    public PolishAddressZipCode ZipCode { get; }
    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    private PolishAddress(AddressStreet street, PolishAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out PolishAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out PolishAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out PolishAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static PolishAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Polish address.", nameof(input));
        return result!;
    }

    public static PolishAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Polish address.", nameof(street));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the address as a single human-readable line,
    /// e.g. <c>Plac Defilad 1, 00-901 Warszawa</c>.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized address, e.g. <c>Plac Defilad 1, 00901, Warszawa, PL</c>.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    private static bool TryWrap(Address address, out PolishAddress? result)
        => TryWrapCore<PolishAddress, PolishAddressZipCode>(address, "PL", Country.Poland,
            PolishAddressZipCode.TryParse,
            (s, z, c, a) => new PolishAddress(s, z, c, a),
            out result);
}
