using System.Text;
using Buildi.Primitives;
using Buildi.Primitives.Organization;
using Buildi.Primitives.Person;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A contact (<c>kontaktuppgift</c>) that combines an optional person name, organization name,
/// and address into a single model. At least one component must be present.
/// Use <see cref="Create"/> to construct from already-parsed primitives, or <see cref="Builder"/>
/// to build from raw strings.
/// </summary>
public sealed class ContactAddress
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Contact Address", "Kontaktuppgift", "📬", []);

    /// <summary>The person's full name, or <see langword="null"/> if not provided.</summary>
    public PersonFullName? PersonName { get; }

    /// <summary>The organization name, or <see langword="null"/> if not provided.</summary>
    public SwedishOrganizationName? OrganizationName { get; }

    /// <summary>The postal address, or <see langword="null"/> if not provided.</summary>
    public Address? Address { get; }

    /// <summary><see langword="true"/> when a person name is present.</summary>
    public bool HasPersonName => PersonName is not null;

    /// <summary><see langword="true"/> when an organization name is present.</summary>
    public bool HasOrganizationName => OrganizationName is not null;

    /// <summary><see langword="true"/> when an address is present.</summary>
    public bool HasAddress => Address is not null;

    private ContactAddress(PersonFullName? personName, SwedishOrganizationName? organizationName, Address? address)
    {
        PersonName = personName;
        OrganizationName = organizationName;
        Address = address;
    }

    /// <summary>
    /// Creates a contact from already-parsed primitives. At least one parameter must be non-null.
    /// </summary>
    public static ContactAddress Create(
        PersonFullName? personName = null,
        SwedishOrganizationName? organizationName = null,
        Address? address = null)
    {
        if (personName is null && organizationName is null && address is null)
            throw new ArgumentException("At least one of personName, organizationName, or address must be provided.");

        return new ContactAddress(personName, organizationName, address);
    }

    /// <summary>
    /// Returns a builder that accepts raw strings and parses them into a contact.
    /// </summary>
    public static ContactAddressBuilder Builder() => new();

    /// <summary>
    /// Returns the contact as multiple lines suitable for a postal label, for example:
    /// <code>
    /// Anna Andersson
    /// Budi AB
    /// Storgatan 12
    /// 114 53 Stockholm
    /// Sverige
    /// </code>
    /// </summary>
    public string ToMultilineString()
    {
        var lines = new List<string>();

        if (PersonName is not null)
            lines.Add(PersonName.Value);

        if (OrganizationName is not null)
            lines.Add(OrganizationName.Value);

        if (Address is not null)
            lines.Add(Address.ToMultilineString());

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns the contact as a single line in the current display language,
    /// for example <c>Anna Andersson, Budi AB, Storgatan 12, 114 53 Stockholm, Sverige</c>.
    /// </summary>
    public string ToDisplayString() => BuildSingleLine(useEnglish: !PrimitivesDefaults.UseLocalizedDisplayNames);

    /// <summary>
    /// Returns the contact as a single line with the country name in English,
    /// for example <c>Anna Andersson, Budi AB, Storgatan 12, 114 53 Stockholm, Sweden</c>.
    /// </summary>
    public string ToEnglishString() => BuildSingleLine(useEnglish: true);

    /// <summary>
    /// Returns the contact as a single line with the country name in the country's own native language (endonym).
    /// </summary>
    public string ToNativeString()
    {
        var parts = new List<string>();

        if (PersonName is not null)
            parts.Add(PersonName.Value);

        if (OrganizationName is not null)
            parts.Add(OrganizationName.Value);

        if (Address is not null)
            parts.Add(Address.ToNativeString());

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Returns the contact as a single line in the current display language.
    /// </summary>
    public override string ToString() => ToDisplayString();

    private string BuildSingleLine(bool useEnglish)
    {
        var parts = new List<string>();

        if (PersonName is not null)
            parts.Add(PersonName.Value);

        if (OrganizationName is not null)
            parts.Add(OrganizationName.Value);

        if (Address is not null)
            parts.Add(useEnglish ? Address.ToEnglishString() : Address.ToDisplayString());

        return string.Join(", ", parts);
    }
}

/// <summary>
/// Fluent builder for creating a <see cref="ContactAddress"/> from raw strings.
/// </summary>
public sealed class ContactAddressBuilder
{
    private PersonFullName? _personName;
    private SwedishOrganizationName? _organizationName;
    private Address? _address;

    internal ContactAddressBuilder() { }

    /// <summary>Parses and sets the person name. Ignored if the value cannot be parsed.</summary>
    public ContactAddressBuilder WithPersonName(string? fullName)
    {
        if (PersonFullName.TryParse(fullName, out var parsed))
            _personName = parsed;
        return this;
    }

    /// <summary>Sets an already-parsed person name.</summary>
    public ContactAddressBuilder WithPersonName(PersonFullName? personName)
    {
        _personName = personName;
        return this;
    }

    /// <summary>Parses and sets the organization name. Ignored if the value cannot be parsed.</summary>
    public ContactAddressBuilder WithOrganizationName(string? organizationName)
    {
        if (SwedishOrganizationName.TryParse(organizationName, out var parsed))
            _organizationName = parsed;
        return this;
    }

    /// <summary>Sets an already-parsed organization name.</summary>
    public ContactAddressBuilder WithOrganizationName(SwedishOrganizationName? organizationName)
    {
        _organizationName = organizationName;
        return this;
    }

    /// <summary>Parses and sets the address from a free-text string. Ignored if the value cannot be parsed.</summary>
    public ContactAddressBuilder WithAddress(string? address)
    {
        if (Address.TryParse(address, out var parsed))
            _address = parsed;
        return this;
    }

    /// <summary>Parses and sets the address from individual components. Ignored if parsing fails.</summary>
    public ContactAddressBuilder WithAddress(string? street, string? zipCode, string? city, string? country = null)
    {
        if (Address.TryParse(street, zipCode, city, country, out var parsed))
            _address = parsed;
        return this;
    }

    /// <summary>Sets an already-parsed address.</summary>
    public ContactAddressBuilder WithAddress(Address? address)
    {
        _address = address;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="ContactAddress"/>. Returns <see langword="false"/> if no components were
    /// successfully set.
    /// </summary>
    public bool TryBuild(out ContactAddress? result)
    {
        result = null;
        if (_personName is null && _organizationName is null && _address is null)
            return false;

        result = ContactAddress.Create(_personName, _organizationName, _address);
        return true;
    }

    /// <summary>
    /// Builds the <see cref="ContactAddress"/>. Throws if no components were successfully set.
    /// </summary>
    public ContactAddress Build()
    {
        if (!TryBuild(out var result))
            throw new InvalidOperationException("At least one of person name, organization name, or address must be provided.");
        return result!;
    }
}
