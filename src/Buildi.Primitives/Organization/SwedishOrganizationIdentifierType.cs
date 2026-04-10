namespace Buildi.Primitives.Organization;

/// <summary>
/// Type of identifier/number parsed.
/// </summary>
public enum SwedishOrganizationIdentifierType
{
    Unknown = 0,

    SwedishOrganizationNumber = 1,
    SwedishPersonalIdentityNumber = 2,
    SwedishCoordinationNumber = 3,

    EuVatNumber = 10,
    DunsNumber = 20,
    LeiCode = 30
}
