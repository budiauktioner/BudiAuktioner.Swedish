using BankingTypes = Buildi.Primitives.Banking;
using ContactTypes = Buildi.Primitives.Contact;
using GeoTypes = Buildi.Primitives.Geography;
using WebTypes = Buildi.Primitives.Web;
using OrgTypes = Buildi.Primitives.Organization;
using PropTypes = Buildi.Primitives.Property;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.SampleData;

/// <summary>
/// A publicly known Swedish organization with parsed sample data.
/// All fields except <see cref="Name"/> and <see cref="Sources"/> are nullable —
/// only populated when explicitly published by the organization.
/// See <c>TEST_AND_SAMPLE_DATA.md</c> for the data strategy and source links.
/// </summary>
public sealed record SampleOrganization
{
    public required OrgTypes.SwedishOrganizationName Name { get; init; }
    public required string[] Sources { get; init; }

    public OrgTypes.SwedishOrganizationNumber? OrganizationNumber { get; init; }
    public OrgTypes.EuVatNumber? EuVatNumber { get; init; }
    public OrgTypes.DunsNumber? DunsNumber { get; init; }
    public OrgTypes.LeiCode? LeiCode { get; init; }
    public OrgTypes.SwedishSniCode? SniCode { get; init; }

    public ContactTypes.PhoneNumber? PhoneNumber { get; init; }
    public WebTypes.EmailAddress? EmailAddress { get; init; }

    public ContactTypes.AddressStreet? AddressStreet { get; init; }
    public string? PostalAddressBox { get; init; }
    public ContactTypes.AddressZipCode? AddressZipCode { get; init; }
    public ContactTypes.AddressCity? AddressCity { get; init; }
    public GeoTypes.Country? Country { get; init; }
    public GeoTypes.SwedishMunicipality? Municipality { get; init; }
    public GeoTypes.SwedishCounty? County { get; init; }

    public PropTypes.SwedishPropertyDesignation? PropertyDesignation { get; init; }

    public BankingTypes.SwedishBankgiroNumber? Bankgiro { get; init; }
    public BankingTypes.SwedishPostgiroNumber? Plusgiro { get; init; }
    public BankingTypes.Iban? Iban { get; init; }
    public BankingTypes.Bic? Bic { get; init; }
    public BankingTypes.SwedishSwishNumber? SwedishSwishNumber { get; init; }
}
