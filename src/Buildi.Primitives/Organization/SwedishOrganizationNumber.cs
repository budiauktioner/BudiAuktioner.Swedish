using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Person;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Organization;

/// <summary>
/// A Swedish organization number (<c>organisationsnummer</c>) is a unique 10-digit identifier regulated by
/// lagen om identitetsbeteckning för juridiska personer m.fl. (SFS 1974:174). Organization numbers are
/// assigned by the authority that registers the entity — Bolagsverket for companies and associations,
/// Skatteverket for sole traders and estates, Länsstyrelsen for foundations, and others. For sole traders
/// (<c>enskild firma</c>), the personal identity number itself serves as the organization number.
/// This library also supports a 12-digit convenience form (<c>16NNNNNNNNNN</c> for legal entities,
/// <c>YYYYMMDDXXXX</c> for person-based numbers) for uniform storage, but the statutory form is always 10 digits.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://bolagsverket.se">Bolagsverket</see> — Swedish Companies Registration Office</description></item>
/// <item><description><see href="https://skatteverket.se/foretag/drivaforetag/startaforetag/organisationsnummer.html">Skatteverket — Organisationsnummer</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Organisationsnummer">Wikipedia — Organisationsnummer</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishOrganizationNumber : IEquatable<SwedishOrganizationNumber>, IComparable<SwedishOrganizationNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Organization Number", "Organisationsnummer", "🏢", ["https://bolagsverket.se", "https://skatteverket.se/foretag/drivaforetag/startaforetag/organisationsnummer.html", "https://sv.wikipedia.org/wiki/Organisationsnummer"]);

    private const int MaxInputLength = 30;
    private const string LegalEntityPrefix = "16";

    private readonly string _tenDigitsWithDash;
    private readonly string _twelveDigits;
    private readonly bool _isPerson;

    /// <summary>
    /// Result of organization type hint logic.
    /// </summary>
    public sealed class SwedishOrganizationTypeHintResult
    {
        /// <summary>
        /// The organization type we are certain of based on the number pattern alone.
        /// </summary>
        public SwedishOrganizationType Certain { get; init; }

        /// <summary>
        /// Our best guess for the organization type, refined using name and other hints.
        /// </summary>
        public SwedishOrganizationType BestGuess { get; init; }
    }

    /// <summary>
    /// Returns true if this is a person-based number (Personal Identity Number or Coordination Number).
    /// </summary>
    public bool IsPerson => _isPerson;

    private SwedishOrganizationNumber(string tenDigitsWithDash, string twelveDigits, bool isPerson)
    {
        _tenDigitsWithDash = tenDigitsWithDash;
        _twelveDigits = twelveDigits;
        _isPerson = isPerson;
    }

    /// <summary>
    /// Attempts to parse the input as a Swedish Organization Number.
    /// Accepts 10 digits, 12 digits (starting with 16 for legal entities, or century for persons), with or without separators.
    /// </summary>
    public static bool TryParse(string? input, out SwedishOrganizationNumber? organizationNumber)
    {
        organizationNumber = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        input = InputSanitization.SanitizeInput(input!).Trim();
        if (input.Length > MaxInputLength) return false;

        // 1. Try Personal Identity Number (has built-in validation and cleaning)
        if (SwedishPersonalIdentityNumber.TryParse(input, out var pin))
        {
            var formatted = pin!.To10DigitString();
            var twelve = pin.To12DigitString();
            organizationNumber = new SwedishOrganizationNumber(formatted, twelve, true);
            return true;
        }

        // 2. Try Coordination Number (has built-in validation and cleaning)
        if (SwedishCoordinationNumber.TryParse(input, out var cn))
        {
            var formatted = cn!.To10DigitString();
            var twelve = cn.To12DigitString();
            organizationNumber = new SwedishOrganizationNumber(formatted, twelve, true);
            return true;
        }

        // 3. Not a PIN/CN, try as Legal Entity (including Dödsbo)
        var digits = InputSanitization.KeepDigits(input!);
        if (string.IsNullOrEmpty(digits)) return false;

        string tenDigits;

        if (digits.Length == 10)
        {
            tenDigits = digits;
        }
        else if (digits.Length == 12 && digits.StartsWith(LegalEntityPrefix))
        {
            tenDigits = digits.Substring(2);
        }
        else
        {
            return false;
        }

        // Check if it's a valid legal entity pattern (includes Dödsbo)
        if (!IsLegalEntityPattern(tenDigits)) return false;

        // Validate Luhn checksum
        if (!Luhn.IsValid(tenDigits))
        {
            return false;
        }

        var tenWithDash = $"{tenDigits.Substring(0, 6)}-{tenDigits.Substring(6, 4)}";
        var twelveDigits = LegalEntityPrefix + tenDigits;

        organizationNumber = new SwedishOrganizationNumber(tenWithDash, twelveDigits, false);
        return true;
    }

    public static SwedishOrganizationNumber Parse(string input)
    {
        if (!TryParse(input, out var org))
        {
            throw new ArgumentException("Invalid Swedish organization number.", nameof(input));
        }

        return org!;
    }

    /// <summary>
    /// Returns the 10 raw digits without any separator (NNNNNNNNNN).
    /// </summary>
    public string To10DigitsOnly() => _tenDigitsWithDash.Replace("-", "").Replace("+", "");

    /// <summary>
    /// Returns the 10-digit string with separator (NNNNNN-NNNN or NNNNNN+XXXX for old PINs).
    /// Legal entities and Dödsbo always use dash.
    /// Personal identity numbers use + if over 100 years old, otherwise -.
    /// </summary>
    public string To10DigitString() => _tenDigitsWithDash;

    /// <summary>
    /// Returns a 12-digit convenience form for uniform storage and comparison.
    /// Legal entities are prefixed with <c>16</c> (e.g. <c>165592460421</c>),
    /// person-based numbers use the full birth date (e.g. <c>199001011234</c>).
    /// Note: the statutory form is always 10 digits; this is a library normalization.
    /// </summary>
    public string To12DigitString() => _twelveDigits;

    /// <summary>
    /// Convenience: returns true if input is a valid Swedish organization number (legal entity, PIN or coordination).
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the Swedish organization number in 10-digit display format, for example <c>559246-0421</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.To10DigitString() : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized Swedish organization number in 12-digit form, for example <c>165592460421</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.To12DigitString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized Swedish organization number in 12-digit form, for example <c>165592460421</c>.
    /// </summary>
    public string ToNormalizedString() => To12DigitString();
    /// <summary>
    /// Returns the Swedish organization number in 10-digit display format, for example <c>559246-0421</c>.
    /// </summary>
    public override string ToString() => To10DigitString();

    /// <summary>
    /// Determines the likely organization type.
    /// Returns both what we're certain of based on the number pattern alone,
    /// and our best guess refined using name and other hints via
    /// <see cref="SwedishOrganizationName.InferSwedishOrganizationType"/>.
    /// </summary>
    /// <param name="name">Optional name to refine the guess.</param>
    /// <param name="isPrivatePerson">Optional hint if the entity is known to be a private individual.</param>
    public SwedishOrganizationTypeHintResult GetSwedishOrganizationTypeHint(string? name = null, bool? isPrivatePerson = null)
    {
        SwedishOrganizationType certain;
        SwedishOrganizationType bestGuess;

        if (_isPerson)
        {
            certain = SwedishOrganizationType.EnskildFirmaEllerPrivatperson;

            if (isPrivatePerson == true)
                bestGuess = SwedishOrganizationType.Privatperson;
            else if (isPrivatePerson == false)
                bestGuess = SwedishOrganizationType.EnskildFirma;
            else
            {
                var nameType = SwedishOrganizationName.InferSwedishOrganizationType(name);
                bestGuess = nameType is SwedishOrganizationType.EnskildFirma or SwedishOrganizationType.Privatperson
                    ? nameType
                    : certain;
            }

            return new SwedishOrganizationTypeHintResult { Certain = certain, BestGuess = bestGuess };
        }

        var first = To10DigitsOnly()[0];
        var nameType2 = SwedishOrganizationName.InferSwedishOrganizationType(name);

        switch (first)
        {
            case '1':
                certain = SwedishOrganizationType.Dodsbo;
                bestGuess = certain;
                break;

            case '2':
                certain = SwedishOrganizationType.OffentligSektor;
                bestGuess = nameType2 is SwedishOrganizationType.Kommun
                    or SwedishOrganizationType.Region or SwedishOrganizationType.Forsamling
                    ? nameType2 : certain;
                break;

            case '5':
                certain = SwedishOrganizationType.Aktiebolag;
                bestGuess = nameType2 == SwedishOrganizationType.Kommanditbolag ? nameType2 : certain;
                break;

            case '6':
                certain = SwedishOrganizationType.HandelsbolagEllerKommanditbolag;
                bestGuess = nameType2 is SwedishOrganizationType.Kommanditbolag or SwedishOrganizationType.Handelsbolag
                    ? nameType2 : certain;
                break;

            case '7':
                certain = SwedishOrganizationType.EkonomiskForening;
                bestGuess = nameType2 is SwedishOrganizationType.Bostadsrattsforening
                    or SwedishOrganizationType.Samfallighetsforening
                    ? nameType2 : certain;
                break;

            case '8':
                certain = SwedishOrganizationType.IdeellForening;
                bestGuess = nameType2 == SwedishOrganizationType.Stiftelse ? nameType2 : certain;
                break;

            case '9':
                certain = SwedishOrganizationType.HandelsbolagEllerKommanditbolag;
                bestGuess = nameType2 is SwedishOrganizationType.Kommanditbolag or SwedishOrganizationType.Handelsbolag
                    ? nameType2 : certain;
                break;

            default:
                certain = SwedishOrganizationType.Other;
                bestGuess = nameType2 is SwedishOrganizationType.EuropeiskEkonomiskIntressegruppering
                    or SwedishOrganizationType.Europabolag or SwedishOrganizationType.SCEForening
                    or SwedishOrganizationType.Filial
                    ? nameType2 : certain;
                break;
        }

        return new SwedishOrganizationTypeHintResult { Certain = certain, BestGuess = bestGuess };
    }

    /// <summary>
    /// Validates the input and returns a <see cref="ValidationResult"/> with detailed reasons on failure.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var sanitized = InputSanitization.SanitizeInput(input!).Trim();

        if (sanitized.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Input contains too many characters.", "Värdet innehåller för många tecken.");

        if (SwedishPersonalIdentityNumber.TryParse(sanitized, out _))
            return ValidationResult.Valid(input);

        if (SwedishCoordinationNumber.TryParse(sanitized, out _))
            return ValidationResult.Valid(input);

        var digits = InputSanitization.KeepDigits(sanitized);
        if (string.IsNullOrEmpty(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "Input contains no digits.", "Värdet innehåller inga siffror.");

        if (digits.Length != 10 && !(digits.Length == 12 && digits.StartsWith(LegalEntityPrefix)))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "Organization number must be 10 or 12 digits (12-digit form must start with 16).",
                "Organisationsnummer måste vara 10 eller 12 siffror (12-siffrig form måste börja med 16).");

        var tenDigits = digits.Length == 12 ? digits.Substring(2) : digits;

        if (!IsLegalEntityPattern(tenDigits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidEntityPattern,
                "Number does not match a valid organization type pattern.",
                "Numret matchar inte ett giltigt organisationstypsmönster.");

        if (!Luhn.IsValid(tenDigits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidCheckDigit,
                "Invalid check digit.", "Ogiltig kontrollsiffra.");

        return ValidationResult.Valid(input);
    }

    // --- Helpers ---

    private static bool IsLegalEntityPattern(string tenDigits)
    {
        if (tenDigits.Length != 10) return false;
        
        // Dödsbo: starts with 1
        if (tenDigits[0] == '1') return true;
        
        // First digit must be a known category.
        var first = tenDigits[0];
        if (first is not ('1' or '2' or '3' or '5' or '6' or '7' or '8' or '9'))
        {
            return false;
        }
        
        // Other legal entities: month >= 20
        var m1 = tenDigits[2] - '0';
        var m2 = tenDigits[3] - '0';
        var mm = m1 * 10 + m2;

        return mm >= 20;
    }

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)(\d{6})-?(\d{4})(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swedish organization numbers (legal entities only;
    /// person-based numbers are found by <see cref="SwedishPersonalIdentityNumber.FindCandidatesInText"/>).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real organization number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishOrganizationNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishOrganizationNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var org)) continue;
            if (org!.IsPerson) continue;

            results.Add(new TextCandidate<SwedishOrganizationNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishOrganizationNumber),
                TextCandidateCategory.OrganizationIdentifier,
                org.ToNormalizedString(),
                org.ToString(),
                org.ToMaskedString(),
                TextMatchConfidence.High,
                org));
        }
        return results;
    }

    public bool Equals(SwedishOrganizationNumber? other) => other is not null && _twelveDigits == other._twelveDigits;
    public override bool Equals(object? obj) => obj is SwedishOrganizationNumber other && Equals(other);
    public override int GetHashCode() => _twelveDigits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishOrganizationNumber? a, SwedishOrganizationNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishOrganizationNumber? a, SwedishOrganizationNumber? b) => !(a == b);
    public int CompareTo(SwedishOrganizationNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishOrganizationNumber left, SwedishOrganizationNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishOrganizationNumber left, SwedishOrganizationNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishOrganizationNumber left, SwedishOrganizationNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishOrganizationNumber left, SwedishOrganizationNumber right) => left.CompareTo(right) >= 0;
}
