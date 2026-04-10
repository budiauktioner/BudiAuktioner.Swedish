using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// Common interface for all country-specific address types.
/// Each implementation wraps an <see cref="Address"/> and enforces
/// the validation and formatting rules of a specific country.
/// Use <see cref="Address"/> directly when international addresses need to be accepted.
/// </summary>
public interface ICountryAddress
{
    /// <summary>The country this address belongs to.</summary>
    Country Country { get; }

    /// <summary>The street component.</summary>
    AddressStreet Street { get; }

    /// <summary>The country-specific zip code, accessible through the common interface.</summary>
    ICountryAddressZipCode CountryZipCode { get; }

    /// <summary>The city/postal locality.</summary>
    AddressCity City { get; }

    /// <summary>The underlying generic <see cref="Address"/> instance for interop.</summary>
    Address Address { get; }

    /// <summary>Care-of name, if present.</summary>
    string? CareOf { get; }

    /// <summary>Post box identifier, if present.</summary>
    string? PostBox { get; }

    /// <summary>Whether this address uses a post box instead of a street.</summary>
    bool IsPostBox { get; }

    /// <summary>The normalized form of the address.</summary>
    string Value { get; }

    /// <summary>The human-readable display form of the address.</summary>
    string Formatted { get; }

    /// <summary>Returns the normalized address string.</summary>
    string ToNormalizedString();

    /// <summary>Returns the address as multiple lines suitable for postal labels.</summary>
    string ToMultilineString();
}
