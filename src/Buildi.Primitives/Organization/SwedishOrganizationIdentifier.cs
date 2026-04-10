using Buildi.Primitives;
using Buildi.Primitives.Person;

namespace Buildi.Primitives.Organization;

/// <summary>
/// Parsed organization identifier result from the unified organization number parser.
/// The parser identifies and classifies organization identifiers across multiple formats:
/// Swedish organization numbers, personal identity numbers, coordination numbers, DUNS, LEI, and VAT numbers.
/// Personal identity numbers and coordination numbers are included because sole traders (enskild firma)
/// use their personal identity number as their organization number under Swedish law.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.dnb.com/duns.html">Dun &amp; Bradstreet — D-U-N-S Number</see></description></item>
/// <item><description><see href="https://www.gleif.org/en/about-lei/iso-17442-the-lei-code-structure/">GLEIF — Legal Entity Identifier (LEI)</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Data_Universal_Numbering_System">Wikipedia — Data Universal Numbering System</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Legal_Entity_Identifier">Wikipedia — Legal Entity Identifier</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishOrganizationIdentifier
{
    public SwedishOrganizationIdentifierType Type { get; init; }
    public SwedishOrganizationType OrganizationTypeHintCertain { get; init; }
    public SwedishOrganizationType OrganizationTypeHintBestGuess { get; init; }
    public string NormalizedValue { get; init; } = string.Empty;

    public SwedishOrganizationNumber? SwedishOrganizationNumber { get; init; }
    public EuVatNumber? EuVatNumber { get; init; }
    public DunsNumber? DunsNumber { get; init; }
    public LeiCode? LeiCode { get; init; }
}

/// <summary>
/// A unified parser that identifies and classifies organization identifiers across multiple formats:
/// Swedish organization numbers, personal identity numbers, coordination numbers, DUNS, LEI, and VAT numbers.
/// Personal identity numbers and coordination numbers are included because sole traders (enskild firma)
/// use their personal identity number as their organization number under Swedish law.
/// Useful when the input type is unknown.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.dnb.com/duns.html">Dun &amp; Bradstreet — D-U-N-S Number</see></description></item>
/// <item><description><see href="https://www.gleif.org/en/about-lei/iso-17442-the-lei-code-structure/">GLEIF — Legal Entity Identifier (LEI)</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Data_Universal_Numbering_System">Wikipedia — Data Universal Numbering System</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Legal_Entity_Identifier">Wikipedia — Legal Entity Identifier</see></description></item>
/// </list>
/// </remarks>
public static class SwedishOrganizationIdentifierParser
{
    private const int MaxInputLength = 30;

    /// <summary>
    /// Attempts to parse arbitrary input as a known number type.
    /// When <see cref="PrimitivesDefaults.CountryAlpha2Code"/> is <c>SE</c>, tries Swedish organization
    /// number first; otherwise tries VAT first.
    /// </summary>
    /// <param name="input">The raw string input.</param>
    /// <param name="result">The parsed result.</param>
    /// <param name="organizationName">Optional name to improve organization type hinting.</param>
    /// <param name="isPrivatePerson">Optional hint if the entity is a private person.</param>
    public static bool TryParse(string? input, out SwedishOrganizationIdentifier result, string? organizationName = null, bool? isPrivatePerson = null)
    {
        result = new SwedishOrganizationIdentifier
        {
            Type = SwedishOrganizationIdentifierType.Unknown,
            OrganizationTypeHintCertain = SwedishOrganizationType.Unknown,
            OrganizationTypeHintBestGuess = SwedishOrganizationType.Unknown,
            NormalizedValue = string.Empty
        };

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        input = InputSanitization.SanitizeInput(input!).Trim();
        if (input.Length > MaxInputLength) return false;

        var isSwedishDefault = PrimitivesDefaults.CountryAlpha2Code == "SE";

        if (isSwedishDefault)
        {
            if (TryParseSwedishOrg(input, organizationName, isPrivatePerson, out var r1)) { result = r1!; return true; }
            if (TryParseVat(input, organizationName, isPrivatePerson, out var r2)) { result = r2!; return true; }
        }
        else
        {
            if (TryParseVat(input, organizationName, isPrivatePerson, out var r3)) { result = r3!; return true; }
            if (TryParseSwedishOrg(input, organizationName, isPrivatePerson, out var r4)) { result = r4!; return true; }
        }

        if (DunsNumber.TryParse(input, out var duns))
        {
            result = new SwedishOrganizationIdentifier
            {
                Type = SwedishOrganizationIdentifierType.DunsNumber,
                OrganizationTypeHintCertain = SwedishOrganizationType.Unknown,
                OrganizationTypeHintBestGuess = SwedishOrganizationType.Unknown,
                NormalizedValue = duns!.Digits,
                DunsNumber = duns
            };
            return true;
        }

        if (LeiCode.TryParse(input, out var lei))
        {
            result = new SwedishOrganizationIdentifier
            {
                Type = SwedishOrganizationIdentifierType.LeiCode,
                OrganizationTypeHintCertain = SwedishOrganizationType.Unknown,
                OrganizationTypeHintBestGuess = SwedishOrganizationType.Unknown,
                NormalizedValue = lei!.Value,
                LeiCode = lei
            };
            return true;
        }

        return false;
    }

    private static bool TryParseSwedishOrg(string input, string? organizationName, bool? isPrivatePerson, out SwedishOrganizationIdentifier? result)
    {
        result = null;
        if (!SwedishOrganizationNumber.TryParse(input, out var seOrg)) return false;

        var type = SwedishOrganizationIdentifierType.SwedishOrganizationNumber;
        if (seOrg!.IsPerson)
        {
            var twelve = seOrg.To12DigitString();
            if (SwedishPersonalIdentityNumber.TryParse(twelve, out _))
                type = SwedishOrganizationIdentifierType.SwedishPersonalIdentityNumber;
            else if (SwedishCoordinationNumber.TryParse(twelve, out _))
                type = SwedishOrganizationIdentifierType.SwedishCoordinationNumber;
        }

        var hint = seOrg.GetSwedishOrganizationTypeHint(organizationName, isPrivatePerson);
        result = new SwedishOrganizationIdentifier
        {
            Type = type,
            OrganizationTypeHintCertain = hint.Certain,
            OrganizationTypeHintBestGuess = hint.BestGuess,
            NormalizedValue = seOrg.To12DigitString(),
            SwedishOrganizationNumber = seOrg
        };
        return true;
    }

    private static bool TryParseVat(string input, string? organizationName, bool? isPrivatePerson, out SwedishOrganizationIdentifier? result)
    {
        result = null;
        if (!EuVatNumber.TryParse(input, out var vat) || vat == null) return false;

        var hintCertain = SwedishOrganizationType.Unknown;
        var hintBestGuess = SwedishOrganizationType.Unknown;

        var seOrgFromVat = vat.ToSwedishOrganizationNumber();
        if (seOrgFromVat != null)
        {
            var hintResult = seOrgFromVat.GetSwedishOrganizationTypeHint(organizationName, isPrivatePerson);
            hintCertain = hintResult.Certain;
            hintBestGuess = hintResult.BestGuess;
        }

        result = new SwedishOrganizationIdentifier
        {
            Type = SwedishOrganizationIdentifierType.EuVatNumber,
            OrganizationTypeHintCertain = hintCertain,
            OrganizationTypeHintBestGuess = hintBestGuess,
            NormalizedValue = vat.CountryCode + vat.Body,
            EuVatNumber = vat
        };
        return true;
    }

    public static SwedishOrganizationIdentifier Parse(string input, string? organizationName = null)
    {
        if (!TryParse(input, out var res, organizationName))
        {
            throw new ArgumentException("Could not parse input as a known organization/identity number.", nameof(input));
        }
        return res!;
    }

    /// <summary>
    /// Attempts to parse input as a known number type, but only accepts numbers from the specified country.
    /// DUNS and LEI numbers are rejected as they don't have country information and cannot be validated.
    /// </summary>
    /// <param name="input">The raw string input.</param>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code to validate against (e.g., "SE").</param>
    /// <param name="result">The parsed result.</param>
    /// <param name="organizationName">Optional name to improve organization type hinting.</param>
    /// <param name="isPrivatePerson">Optional hint if the entity is a private person.</param>
    public static bool TryParseForCountry(string? input, string countryCode, out SwedishOrganizationIdentifier result, string? organizationName = null, bool? isPrivatePerson = null)
    {
        result = new SwedishOrganizationIdentifier
        {
            Type = SwedishOrganizationIdentifierType.Unknown,
            OrganizationTypeHintCertain = SwedishOrganizationType.Unknown,
            OrganizationTypeHintBestGuess = SwedishOrganizationType.Unknown,
            NormalizedValue = string.Empty
        };

        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(countryCode))
        {
            return false;
        }

        var normalizedCountryCode = countryCode.ToUpperInvariant();

        // 1. Swedish Organization Number - only accept if country is SE
        if (normalizedCountryCode == "SE" && SwedishOrganizationNumber.TryParse(input, out var seOrg))
        {
            // Determine specific subtype
            var type = SwedishOrganizationIdentifierType.SwedishOrganizationNumber;
            
            // If it's a person-based number, determine if PIN or Coord
            if (seOrg!.IsPerson)
            {
                var twelve = seOrg.To12DigitString();
                if (SwedishPersonalIdentityNumber.TryParse(twelve, out _))
                {
                    type = SwedishOrganizationIdentifierType.SwedishPersonalIdentityNumber;
                }
                else if (SwedishCoordinationNumber.TryParse(twelve, out _))
                {
                    type = SwedishOrganizationIdentifierType.SwedishCoordinationNumber;
                }
            }

            var hint = seOrg.GetSwedishOrganizationTypeHint(organizationName, isPrivatePerson);
            result = new SwedishOrganizationIdentifier
            {
                Type = type,
                OrganizationTypeHintCertain = hint.Certain,
                OrganizationTypeHintBestGuess = hint.BestGuess,
                NormalizedValue = seOrg.To12DigitString(),
                SwedishOrganizationNumber = seOrg
            };
            return true;
        }

        // 2. VAT - use country-specific validation
        if (EuVatNumber.TryParseForCountry(input, normalizedCountryCode, out var vat) && vat != null)
        {
            var hintCertain = SwedishOrganizationType.Unknown;
            var hintBestGuess = SwedishOrganizationType.Unknown;
            
            // If SE VAT, use the extension method to extract org number for hinting
            var seOrgFromVat = vat.ToSwedishOrganizationNumber();
            if (seOrgFromVat != null)
            {
                var hintResult = seOrgFromVat.GetSwedishOrganizationTypeHint(organizationName, isPrivatePerson);
                hintCertain = hintResult.Certain;
                hintBestGuess = hintResult.BestGuess;
            }
            
            result = new SwedishOrganizationIdentifier
            {
                Type = SwedishOrganizationIdentifierType.EuVatNumber,
                OrganizationTypeHintCertain = hintCertain,
                OrganizationTypeHintBestGuess = hintBestGuess,
                NormalizedValue = vat.CountryCode + vat.Body,
                EuVatNumber = vat
            };
            return true;
        }

        // DUNS and LEI are NOT checked when filtering by country
        // as they don't have country information and cannot be validated

        return false;
    }

    /// <summary>
    /// Parses input as a known number type, but only accepts numbers from the specified country.
    /// DUNS and LEI numbers are rejected as they don't have country information and cannot be validated.
    /// Throws an exception if the input cannot be parsed or is from a different country.
    /// </summary>
    /// <param name="input">The raw string input.</param>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code to validate against (e.g., "SE").</param>
    /// <param name="organizationName">Optional name to improve organization type hinting.</param>
    /// <returns>The parsed result.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is not a valid number for the specified country.</exception>
    public static SwedishOrganizationIdentifier ParseForCountry(string input, string countryCode, string? organizationName = null)
    {
        if (!TryParseForCountry(input, countryCode, out var res, organizationName))
        {
            throw new ArgumentException($"Could not parse input as a known organization/identity number for country {countryCode}.", nameof(input));
        }
        return res!;
    }
}
