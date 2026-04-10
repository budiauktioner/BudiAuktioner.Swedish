using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// Abstract base class for country-specific zip code types. Provides common properties
/// and instance methods shared by all implementations. Subclasses provide the country,
/// parsing logic, and static API.
/// </summary>
public abstract class CountryAddressZipCodeBase : ICountryAddressZipCode
{
    /// <inheritdoc />
    public string Value { get; }

    /// <inheritdoc />
    public string Formatted { get; }

    /// <inheritdoc />
    public AddressZipCode ZipCode { get; }

    /// <inheritdoc />
    public abstract Country Country { get; }

    protected CountryAddressZipCodeBase(string value, string formatted, AddressZipCode zipCode)
    {
        Value = value;
        Formatted = formatted;
        ZipCode = zipCode;
    }

    /// <inheritdoc />
    public string ToNormalizedString() => Value;

    /// <inheritdoc cref="object.ToString" />
    public override string ToString() => Formatted;
}
