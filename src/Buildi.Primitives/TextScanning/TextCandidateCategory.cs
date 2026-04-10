namespace Buildi.Primitives.TextScanning;

/// <summary>
/// Broad classification of a <see cref="TextCandidate{T}"/> for filtering and selective masking.
/// </summary>
public enum TextCandidateCategory
{
    /// <summary>Personal identity numbers (personnummer, samordningsnummer).</summary>
    PersonalIdentifier = 0,

    /// <summary>Organization identifiers (organisationsnummer, VAT, LEI, DUNS).</summary>
    OrganizationIdentifier = 1,

    /// <summary>Financial identifiers (IBAN, BIC, bank accounts, Bankgiro, Plusgiro, OCR).</summary>
    Financial = 2,

    /// <summary>Contact information (email, phone, zip code).</summary>
    Contact = 3,

    /// <summary>Vehicle identifiers (registration number, VIN).</summary>
    Vehicle = 4,

    /// <summary>Property identifiers (fastighetsbeteckning).</summary>
    Property = 5,

    /// <summary>Product identifiers (GTIN / EAN barcodes).</summary>
    Product = 6,

    /// <summary>Geographic data (country names).</summary>
    Geography = 7,

    /// <summary>Measurement values (length, weight, volume, energy, etc.).</summary>
    Measurement = 8
}
