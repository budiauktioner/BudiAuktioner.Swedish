using System.Text;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Organization;

/// <summary>
/// A VAT identification number (<c>momsnummer</c> / <c>momsregistreringsnummer</c>) uniquely identifies businesses registered for value-added tax within the EU. Format and validation rules are country-specific — this library supports all EU member states.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://ec.europa.eu/taxation_customs/vies/#/vat-validation">EU VIES</see> — VAT Information Exchange System</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/VAT_identification_number">Wikipedia — VAT identification number</see></description></item>
/// </list>
/// </remarks>
public sealed class EuVatNumber : IEquatable<EuVatNumber>, IComparable<EuVatNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("VAT Number", "Momsnummer", "💰", ["https://ec.europa.eu/taxation_customs/vies/#/vat-validation", "https://en.wikipedia.org/wiki/VAT_identification_number"]);

    private const int MaxInputLength = 30;

    private static readonly Dictionary<string, (string CountryCode, string CountryName)> CountryInfo = new(StringComparer.OrdinalIgnoreCase)
    {
        // European Union VAT identification numbers
        ["AT"] = ("AT", "Austria"),
        ["BE"] = ("BE", "Belgium"),
        ["BG"] = ("BG", "Bulgaria"),
        ["HR"] = ("HR", "Croatia"),
        ["CY"] = ("CY", "Cyprus"),
        ["CZ"] = ("CZ", "Czech Republic"),
        ["DK"] = ("DK", "Denmark"),
        ["EE"] = ("EE", "Estonia"),
        ["FI"] = ("FI", "Finland"),
        ["FR"] = ("FR", "France"),
        ["DE"] = ("DE", "Germany"),
        ["EL"] = ("GR", "Greece"),  // EL is VAT prefix, GR is ISO code
        ["GR"] = ("GR", "Greece"),  // Both accepted
        ["HU"] = ("HU", "Hungary"),
        ["IE"] = ("IE", "Ireland"),
        ["IT"] = ("IT", "Italy"),
        ["LV"] = ("LV", "Latvia"),
        ["LT"] = ("LT", "Lithuania"),
        ["LU"] = ("LU", "Luxembourg"),
        ["MT"] = ("MT", "Malta"),
        ["NL"] = ("NL", "Netherlands"),
        ["PL"] = ("PL", "Poland"),
        ["PT"] = ("PT", "Portugal"),
        ["RO"] = ("RO", "Romania"),
        ["SK"] = ("SK", "Slovakia"),
        ["SI"] = ("SI", "Slovenia"),
        ["ES"] = ("ES", "Spain"),
        ["SE"] = ("SE", "Sweden"),
        
        // Non-EU European countries
        ["GB"] = ("GB", "United Kingdom"),
        ["NO"] = ("NO", "Norway"),
        ["IS"] = ("IS", "Iceland"),
        ["LI"] = ("LI", "Liechtenstein"),
        ["CH"] = ("CH", "Switzerland"),
        ["AL"] = ("AL", "Albania"),
        ["BA"] = ("BA", "Bosnia and Herzegovina"),
        ["BY"] = ("BY", "Belarus"),
        ["MK"] = ("MK", "North Macedonia"),
        ["RS"] = ("RS", "Serbia"),
        ["SM"] = ("SM", "San Marino"),
        ["TR"] = ("TR", "Turkey"),
        ["UA"] = ("UA", "Ukraine"),
        ["RU"] = ("RU", "Russia"),
        ["UZ"] = ("UZ", "Uzbekistan"),
        
        // Asia-Pacific
        ["AU"] = ("AU", "Australia"),
        ["CN"] = ("CN", "China"),
        ["HK"] = ("HK", "Hong Kong"),
        ["IN"] = ("IN", "India"),
        ["ID"] = ("ID", "Indonesia"),
        ["IL"] = ("IL", "Israel"),
        ["JP"] = ("JP", "Japan"),
        ["KZ"] = ("KZ", "Kazakhstan"),
        ["NZ"] = ("NZ", "New Zealand"),
        ["PH"] = ("PH", "Philippines"),
        ["SA"] = ("SA", "Saudi Arabia"),
        ["SG"] = ("SG", "Singapore"),
        ["TW"] = ("TW", "Taiwan"),
        
        // Americas
        ["AR"] = ("AR", "Argentina"),
        ["BZ"] = ("BZ", "Belize"),
        ["BO"] = ("BO", "Bolivia"),
        ["BR"] = ("BR", "Brazil"),
        ["CA"] = ("CA", "Canada"),
        ["CL"] = ("CL", "Chile"),
        ["CO"] = ("CO", "Colombia"),
        ["CR"] = ("CR", "Costa Rica"),
        ["DO"] = ("DO", "Dominican Republic"),
        ["EC"] = ("EC", "Ecuador"),
        ["SV"] = ("SV", "El Salvador"),
        ["GT"] = ("GT", "Guatemala"),
        ["HN"] = ("HN", "Honduras"),
        ["MX"] = ("MX", "Mexico"),
        ["NI"] = ("NI", "Nicaragua"),
        ["PA"] = ("PA", "Panama"),
        ["PY"] = ("PY", "Paraguay"),
        ["PE"] = ("PE", "Peru"),
        ["UY"] = ("UY", "Uruguay"),
        ["US"] = ("US", "United States"),
        ["VE"] = ("VE", "Venezuela"),
        
        // Africa
        ["NG"] = ("NG", "Nigeria")
    };

    private static readonly HashSet<string> ValidCountryCodes = new(CountryInfo.Keys, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The VAT prefix (2-letter country code from VAT number, e.g., "SE", "EL").
    /// Note: EL is used for Greece in VAT numbers, though ISO code is GR.
    /// </summary>
    public string VatPrefix { get; }
    
    /// <summary>
    /// The ISO 3166-1 alpha-2 country code (e.g., "SE", "GR").
    /// </summary>
    public string CountryCode { get; }
    
    /// <summary>
    /// The country name in English (e.g., "Sweden", "Greece").
    /// </summary>
    public string CountryName { get; }
    
    /// <summary>
    /// The body of the VAT number (everything after the country prefix).
    /// </summary>
    public string Body { get; }

    private EuVatNumber(string vatPrefix, string countryCode, string countryName, string body)
    {
        VatPrefix = vatPrefix;
        CountryCode = countryCode;
        CountryName = countryName;
        Body = body;
    }

    public static bool TryParse(string? input, out EuVatNumber? vat)
    {
        vat = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // Clean common separators
        var cleaned = OrganizationValidationUtils.CleanSeparators(InputSanitization.SanitizeInput(input!));
        if (cleaned.Length > MaxInputLength) return false;

        if (cleaned.Length < 3)
        {
            return false;
        }

        // Country code = first two letters
        var cc1 = cleaned[0];
        var cc2 = cleaned[1];
        if (!char.IsLetter(cc1) || !char.IsLetter(cc2))
        {
            return false;
        }
        var cc = new string(new[] { char.ToUpperInvariant(cc1), char.ToUpperInvariant(cc2) });
        if (!ValidCountryCodes.Contains(cc))
        {
            return false;
        }

        // Get country info
        var (countryCode, countryName) = CountryInfo[cc];

        // Body = remaining
        var bodySubstring = cleaned.Substring(2);
        var bodyBuilder = new StringBuilder(bodySubstring.Length);
        for (var i = 0; i < bodySubstring.Length; i++)
        {
            var c = bodySubstring[i];
            if (!char.IsLetterOrDigit(c))
            {
                return false;
            }
            bodyBuilder.Append(char.ToUpperInvariant(c));
        }
        var body = bodyBuilder.ToString();
        if (body.Length == 0)
        {
            return false;
        }

        // Validate country-specific format
        if (!ValidateCountryFormat(countryCode, body))
        {
            return false;
        }

        vat = new EuVatNumber(cc, countryCode, countryName, body);
        return true;
    }

    public static EuVatNumber Parse(string input)
    {
        if (!TryParse(input, out var vat))
        {
            throw new ArgumentException("Invalid VAT number.", nameof(input));
        }
        return vat!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the VAT number as uppercase country prefix plus body, for example <c>SE559246042101</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.VatPrefix + r.Body : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized VAT number as uppercase country prefix plus body, for example <c>SE559246042101</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.VatPrefix + r.Body;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized VAT number as uppercase country prefix plus body, for example <c>SE559246042101</c>.
    /// </summary>
    public string ToNormalizedString() => VatPrefix + Body;
    /// <summary>
    /// Returns the VAT number as uppercase country prefix plus body, for example <c>SE559246042101</c>.
    /// </summary>
    public override string ToString() => VatPrefix + Body;

    /// <summary>
    /// Attempts to parse a VAT number, but only accepts VAT numbers from the specified country.
    /// </summary>
    /// <param name="input">The VAT number to parse.</param>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code to validate against (e.g., "SE", "DE").</param>
    /// <param name="vat">The parsed VAT number if successful.</param>
    /// <returns>True if the input is a valid VAT number for the specified country.</returns>
    public static bool TryParseForCountry(string? input, string countryCode, out EuVatNumber? vat)
    {
        vat = null;
        
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return false;
        }

        // Try to parse as a regular VAT number
        if (!TryParse(input, out vat))
        {
            return false;
        }

        // Check if it matches the expected country
        // Handle special case: EL (VAT prefix) vs GR (ISO code) for Greece
        var normalizedCountryCode = countryCode.ToUpperInvariant();
        if (IsGreekCountryCode(normalizedCountryCode) && vat!.CountryCode == "GR")
        {
            // Accept both EL and GR for Greece
            return true;
        }

        if (!vat!.CountryCode.Equals(normalizedCountryCode, StringComparison.OrdinalIgnoreCase))
        {
            vat = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Parses a VAT number, but only accepts VAT numbers from the specified country.
    /// Throws an exception if the input is not a valid VAT number for the specified country.
    /// </summary>
    /// <param name="input">The VAT number to parse.</param>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code to validate against (e.g., "SE", "DE").</param>
    /// <returns>The parsed VAT number.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is not a valid VAT number for the specified country.</exception>
    public static EuVatNumber ParseForCountry(string input, string countryCode)
    {
        if (!TryParseForCountry(input, countryCode, out var vat))
        {
            throw new ArgumentException($"Invalid VAT number for country {countryCode}.", nameof(input));
        }
        return vat!;
    }

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a well-formed VAT number.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var cleaned = OrganizationValidationUtils.CleanSeparators(InputSanitization.SanitizeInput(input!));

        if (cleaned.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Input contains too many characters.", "Värdet innehåller för många tecken.");

        if (cleaned.Length < 3)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooShort,
                "VAT number is too short.", "Momsnumret är för kort.");

        var cc1 = cleaned[0];
        var cc2 = cleaned[1];
        if (!char.IsLetter(cc1) || !char.IsLetter(cc2))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidCountryPrefix,
                "VAT number must start with a two-letter country code.",
                "Momsnumret måste börja med en tvåbokstavig landskod.");

        var cc = new string(new[] { char.ToUpperInvariant(cc1), char.ToUpperInvariant(cc2) });
        if (!ValidCountryCodes.Contains(cc))
            return ValidationResult.Invalid(input, ValidationErrorReason.UnknownCountryCode,
                "Unknown VAT country code.", "Okänd momsnummer-landskod.");

        var (countryCode, _) = CountryInfo[cc];
        var bodySubstring = cleaned.Substring(2);
        var bodyBuilder = new StringBuilder(bodySubstring.Length);
        for (var i = 0; i < bodySubstring.Length; i++)
        {
            var c = bodySubstring[i];
            if (!char.IsLetterOrDigit(c))
                return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                    "VAT number body contains invalid characters.",
                    "Momsnumrets innehåll har ogiltiga tecken.");
            bodyBuilder.Append(char.ToUpperInvariant(c));
        }
        var body = bodyBuilder.ToString();
        if (body.Length == 0)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "VAT number body contains invalid characters.",
                "Momsnumrets innehåll har ogiltiga tecken.");

        if (!ValidateCountryFormat(countryCode, body))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "VAT number format is invalid for the specified country.",
                "Momsnummerformatet är ogiltigt för det angivna landet.");

        return ValidationResult.Valid(input);
    }

    // --- Country-specific validation methods ---

    private static bool ValidateCountryFormat(string countryCode, string body)
    {
        return countryCode.ToUpperInvariant() switch
        {
            // EU Countries
            "AT" => ValidateAustrianVat(body),
            "BE" => ValidateBelgianVat(body),
            "BG" => ValidateBulgarianVat(body),
            "HR" => ValidateCroatianVat(body),
            "CY" => ValidateCypriotVat(body),
            "CZ" => ValidateCzechVat(body),
            "DK" => ValidateDanishVat(body),
            "EE" => ValidateEstonianVat(body),
            "FI" => ValidateFinnishVat(body),
            "FR" => ValidateFrenchVat(body),
            "DE" => ValidateGermanVat(body),
            "GR" => ValidateGreekVat(body),
            "HU" => ValidateHungarianVat(body),
            "IE" => ValidateIrishVat(body),
            "IT" => ValidateItalianVat(body),
            "LV" => ValidateLatvianVat(body),
            "LT" => ValidateLithuanianVat(body),
            "LU" => ValidateLuxembourgVat(body),
            "MT" => ValidateMalteseVat(body),
            "NL" => ValidateDutchVat(body),
            "PL" => ValidatePolishVat(body),
            "PT" => ValidatePortugueseVat(body),
            "RO" => ValidateRomanianVat(body),
            "SK" => ValidateSlovakVat(body),
            "SI" => ValidateSlovenianVat(body),
            "ES" => ValidateSpanishVat(body),
            "SE" => ValidateSwedishVat(body),
            
            // Non-EU but validated
            "GB" => ValidateUKVat(body),
            "NO" => ValidateNorwegianVat(body),
            "CH" => ValidateSwissVat(body),
            
            // Others - basic format validation: must contain at least one digit
            _ => body.Any(char.IsDigit)
        };
    }

    // EU Country Validations

    private static bool ValidateSwedishVat(string body)
    {
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 12 || !digits.EndsWith("01", StringComparison.Ordinal))
        {
            return false;
        }
        
        var orgNumber = digits.Substring(0, 10);
        return SwedishOrganizationNumber.TryParse(orgNumber, out _);
    }

    private static bool ValidateAustrianVat(string body)
    {
        // AT + U + 8 digits
        if (body.Length != 9 || body[0] != 'U')
        {
            return false;
        }
        
        var digits = body.Substring(1);
        return digits.Length == 8 && digits.All(char.IsDigit);
    }

    private static bool ValidateBelgianVat(string body)
    {
        // 8 digits + 2 check digits (10 total)
        // Check digits = 97 - (number MOD 97)
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 10)
        {
            return false;
        }

        if (!long.TryParse(digits.Substring(0, 8), out var baseNumber))
        {
            return false;
        }

        var checkDigits = 97 - (baseNumber % 97);
        var actualCheck = int.Parse(digits.Substring(8, 2));
        
        return checkDigits == actualCheck;
    }

    private static bool ValidateBulgarianVat(string body)
    {
        // 9-10 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length >= 9 && digits.Length <= 10;
    }

    private static bool ValidateCroatianVat(string body)
    {
        // 11 digits, ISO 7064 MOD 11-10
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 11)
        {
            return false;
        }

        return ValidateMod1110(digits);
    }

    private static bool ValidateCypriotVat(string body)
    {
        // 9 characters (8 digits + 1 letter)
        if (body.Length != 9)
        {
            return false;
        }

        var digits = body.Substring(0, 8);
        var letter = body[8];
        
        return digits.All(char.IsDigit) && char.IsLetter(letter);
    }

    private static bool ValidateCzechVat(string body)
    {
        // 8-10 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length >= 8 && digits.Length <= 10;
    }

    private static bool ValidateDanishVat(string body)
    {
        // 8 digits, last is check digit
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 8;
    }

    private static bool ValidateEstonianVat(string body)
    {
        // 9 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 9;
    }

    private static bool ValidateFinnishVat(string body)
    {
        // 8 digits (7 + 1 check digit), MOD 11-2
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 8)
        {
            return false;
        }

        // MOD 11-2 validation
        int[] weights = { 7, 9, 10, 5, 8, 4, 2 };
        var sum = 0;
        for (var i = 0; i < 7; i++)
        {
            sum += (digits[i] - '0') * weights[i];
        }

        var remainder = sum % 11;
        if (remainder == 1)
        {
            return false;
        }

        var checkDigit = remainder == 0 ? 0 : 11 - remainder;

        return checkDigit == (digits[7] - '0');
    }

    private static bool ValidateFrenchVat(string body)
    {
        // 2 chars (digits or letters) + 9 digits
        if (body.Length != 11)
        {
            return false;
        }

        var key = body.Substring(0, 2);
        var siren = body.Substring(2, 9);
        
        return siren.All(char.IsDigit);
    }

    private static bool ValidateGermanVat(string body)
    {
        // 9 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 9;
    }

    private static bool ValidateGreekVat(string body)
    {
        // 9 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 9;
    }

    private static bool ValidateHungarianVat(string body)
    {
        // 8 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 8;
    }

    private static bool ValidateIrishVat(string body)
    {
        // Multiple formats:
        // 7 digits + 1 letter
        // 7 digits + 2 letters
        // 1 digit + 1 letter/+/* + 5 digits + 1 letter
        if (body.Length < 8 || body.Length > 9)
        {
            return false;
        }

        // Check common patterns
        if (body.Length == 8)
        {
            return body.Substring(0, 7).All(char.IsDigit) && char.IsLetter(body[7]);
        }

        if (body.Length == 9)
        {
            var firstSevenDigits = body.Substring(0, 7);
            if (firstSevenDigits.All(char.IsDigit) && char.IsLetter(body[7]) && char.IsLetter(body[8]))
            {
                return true;
            }

            return char.IsDigit(body[0])
                && char.IsLetter(body[1])
                && body.Substring(2, 5).All(char.IsDigit)
                && char.IsLetter(body[8]);
        }

        return false;
    }

    private static bool ValidateItalianVat(string body)
    {
        // 11 digits, last is Luhn check digit
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 11)
        {
            return false;
        }

        return Luhn.IsValid(digits);
    }

    private static bool ValidateLatvianVat(string body)
    {
        // 11 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 11;
    }

    private static bool ValidateLithuanianVat(string body)
    {
        // 9 or 12 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 9 || digits.Length == 12;
    }

    private static bool ValidateLuxembourgVat(string body)
    {
        // 8 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 8;
    }

    private static bool ValidateMalteseVat(string body)
    {
        // 8 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 8;
    }

    private static bool ValidateDutchVat(string body)
    {
        // 9 digits + B + 2 digits (company index)
        if (body.Length != 12)
        {
            return false;
        }

        var digits = body.Substring(0, 9);
        var b = body[9];
        var companyIndex = body.Substring(10, 2);
        
        return digits.All(char.IsDigit) && b == 'B' && companyIndex.All(char.IsDigit);
    }

    private static bool ValidatePolishVat(string body)
    {
        // 10 digits, checksum on first 9 digits
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 10)
        {
            return false;
        }

        int[] weights = { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            sum += (digits[i] - '0') * weights[i];
        }

        var checkDigit = sum % 11;
        if (checkDigit == 10)
        {
            return false;
        }

        return checkDigit == (digits[9] - '0');
    }

    private static bool ValidatePortugueseVat(string body)
    {
        // 9 digits, last is check digit
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 9;
    }

    private static bool ValidateRomanianVat(string body)
    {
        // 2-10 digits
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length >= 2 && digits.Length <= 10;
    }

    private static bool ValidateSlovakVat(string body)
    {
        // 10 digits, must be divisible by 11
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 10)
        {
            return false;
        }

        if (!long.TryParse(digits, out var number))
        {
            return false;
        }

        return number % 11 == 0;
    }

    private static bool ValidateSlovenianVat(string body)
    {
        // 8 digits, last is check digit
        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 8;
    }

    private static bool ValidateSpanishVat(string body)
    {
        // Multiple formats:
        // Letter + 8 digits
        // Letter + 7 digits + letter
        // 8 digits + letter
        // Letter + 7 digits + letter
        if (body.Length < 9 || body.Length > 9)
        {
            return false;
        }

        // Accept various Spanish formats
        return true;
    }

    private static bool ValidateUKVat(string body)
    {
        // Multiple formats:
        // 9 digits (3-4-2)
        // 12 digits (3-4-2-3)
        // GD + 3 digits (000-499)
        // HA + 3 digits (500-999)
        var digits = InputSanitization.KeepDigits(body);
        
        if (body.StartsWith("GD") || body.StartsWith("HA"))
        {
            return body.Length >= 5;
        }

        return digits.Length == 9 || digits.Length == 12;
    }

    private static bool ValidateNorwegianVat(string body)
    {
        // 9 digits + MVA, last digit is MOD 11 check
        var digits = InputSanitization.KeepDigits(body);
        if (digits.Length != 9)
        {
            return false;
        }

        int[] weights = { 3, 2, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            sum += (digits[i] - '0') * weights[i];
        }

        var remainder = 11 - (sum % 11);
        if (remainder == 10)
        {
            return false;
        }

        var checkDigit = remainder == 11 ? 0 : remainder;
        return checkDigit == (digits[8] - '0');
    }

    private static bool ValidateSwissVat(string body)
    {
        // CHE + 9 digits, last is MOD 11 check
        if (body.Length < 9)
        {
            return false;
        }

        var digits = InputSanitization.KeepDigits(body);
        return digits.Length == 9;
    }

    // Helper method for MOD 11-10 validation (ISO 7064)
    private static bool ValidateMod1110(string digits)
    {
        var checksum = 10;
        for (var i = 0; i < digits.Length - 1; i++)
        {
            var digit = digits[i] - '0';
            checksum = (checksum + digit) % 10;
            if (checksum == 0)
            {
                checksum = 10;
            }
            checksum = (checksum * 2) % 11;
        }

        var expectedCheck = (11 - checksum) % 10;
        var actualCheck = digits[digits.Length - 1] - '0';
        
        return expectedCheck == actualCheck;
    }

    private static bool IsGreekCountryCode(string countryCode)
        => countryCode is "EL" or "GR";

    private static readonly Regex ScanPattern = new(
        @"\b[A-Z]{2}[A-Z0-9]{5,14}\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential VAT numbers.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real VAT number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<EuVatNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<EuVatNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var vat)) continue;
            results.Add(new TextCandidate<EuVatNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(EuVatNumber),
                TextCandidateCategory.OrganizationIdentifier,
                vat!.ToNormalizedString(),
                vat.ToString(),
                vat.ToMaskedString(),
                TextMatchConfidence.High,
                vat));
        }
        return results;
    }

    public bool Equals(EuVatNumber? other) => other is not null && VatPrefix == other.VatPrefix && Body == other.Body;
    public override bool Equals(object? obj) => obj is EuVatNumber other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(VatPrefix.GetHashCode(StringComparison.Ordinal), Body.GetHashCode(StringComparison.Ordinal));
    public static bool operator ==(EuVatNumber? a, EuVatNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EuVatNumber? a, EuVatNumber? b) => !(a == b);
    public int CompareTo(EuVatNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(EuVatNumber left, EuVatNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(EuVatNumber left, EuVatNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(EuVatNumber left, EuVatNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(EuVatNumber left, EuVatNumber right) => left.CompareTo(right) >= 0;
}
