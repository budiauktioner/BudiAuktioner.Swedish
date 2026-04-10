using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// Abstract base class for country-specific address types. Provides common properties,
/// formatting methods, and the shared <see cref="TryWrapCore{TAddr,TZip}"/> helper
/// used by all implementations. Subclasses provide the country-specific zip code,
/// parsing logic, and static API.
/// </summary>
public abstract class CountryAddressBase : ICountryAddress
{
    /// <inheritdoc />
    public AddressStreet Street { get; }

    /// <inheritdoc />
    public AddressCity City { get; }

    /// <inheritdoc />
    public Address Address { get; }

    /// <inheritdoc />
    public Country Country => Address.Country!;

    /// <summary>The country-specific zip code, accessible through the common interface.</summary>
    public abstract ICountryAddressZipCode CountryZipCode { get; }

    /// <inheritdoc />
    public string? CareOf => Street.CareOf;

    /// <inheritdoc />
    public string? PostBox => Street.PostBox;

    /// <inheritdoc />
    public bool IsPostBox => Street.IsPostBox;

    /// <inheritdoc />
    public string Value => ToNormalizedString();

    /// <inheritdoc />
    public string Formatted => ToString();

    protected CountryAddressBase(AddressStreet street, AddressCity city, Address address)
    {
        Street = street;
        City = city;
        Address = address;
    }

    /// <inheritdoc />
    public string ToNormalizedString() => Address.ToNormalizedString();

    /// <summary>
    /// Returns the address as a single human-readable line in domestic format (country omitted).
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(CareOf)) parts.Add($"c/o {CareOf}");
        parts.Add(Street.Street);
        parts.Add($"{CountryZipCode.Formatted} {City.Value}");
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Returns the address as multiple lines suitable for postal labels.
    /// Country is omitted (domestic format).
    /// </summary>
    public virtual string ToMultilineString()
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(CareOf)) lines.Add($"c/o {CareOf}");
        lines.Add(Street.Street);
        lines.Add($"{CountryZipCode.Formatted} {City.Value}");
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Shared helper for the TryWrap pattern used by all country-specific address types.
    /// Validates country code, requires zip and city, parses the zip with the country-specific
    /// parser, and creates the typed instance.
    /// </summary>
    protected delegate bool TryParseZipFunc<TZip>(string value, out TZip? result);

    protected static bool TryWrapCore<TAddr, TZip>(
        Address address,
        string alpha2,
        Country defaultCountry,
        TryParseZipFunc<TZip> tryParseZip,
        Func<AddressStreet, TZip, AddressCity, Address, TAddr> factory,
        out TAddr? result)
        where TAddr : CountryAddressBase
        where TZip : class
    {
        result = null;
        if (address.Country != null &&
            !address.Country.Alpha2Code.Equals(alpha2, StringComparison.OrdinalIgnoreCase))
            return false;
        if (address.ZipCode == null || address.City == null) return false;
        if (!tryParseZip(address.ZipCode.Value, out var zip) || zip == null) return false;

        var withCountry = address.Country != null
            ? address
            : new Address(address.Street, address.ZipCode, address.City, defaultCountry);

        result = factory(address.Street, zip, address.City, withCountry);
        return true;
    }
}
