using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Contact;

/// <summary>
/// Common interface for all country-specific postal/zip code types.
/// Each implementation wraps an <see cref="AddressZipCode"/> and enforces
/// the formatting rules of a specific country.
/// </summary>
public interface ICountryAddressZipCode
{
    /// <summary>The country this zip code type belongs to.</summary>
    Country Country { get; }

    /// <summary>The normalized/compact form of the zip code.</summary>
    string Value { get; }

    /// <summary>The human-readable display form of the zip code.</summary>
    string Formatted { get; }

    /// <summary>The underlying generic <see cref="AddressZipCode"/> instance.</summary>
    AddressZipCode ZipCode { get; }

    /// <summary>Returns the normalized form of the zip code.</summary>
    string ToNormalizedString();
}
