namespace Buildi.Primitives.Organization;

/// <summary>
/// Hint about the legal form of a Swedish organization.
/// Derived from the organization number and/or name.
/// </summary>
public enum SwedishOrganizationType
{
    Unknown = 0,

    Aktiebolag = 1,
    Dodsbo = 2,

    HandelsbolagEllerKommanditbolag = 10,
    Handelsbolag = 11,
    Kommanditbolag = 12,

    EkonomiskForening = 20,
    Bostadsrattsforening = 21,
    Samfallighetsforening = 22,

    IdeellForening = 30,
    Stiftelse = 31,

    OffentligSektor = 40,
    Kommun = 41,
    Region = 42,
    Forsamling = 43,

    EnkeltBolag = 50,
    Filial = 51,
    Europabolag = 52,
    EuropeiskEkonomiskIntressegruppering = 53,
    SCEForening = 54,

    EnskildFirmaEllerPrivatperson = 60,
    EnskildFirma = 61,
    Privatperson = 62,

    Other = 99
}
