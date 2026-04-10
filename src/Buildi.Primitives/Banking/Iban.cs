using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Banking;

/// <summary>
/// The International Bank Account Number (IBAN) is an internationally agreed system for identifying bank accounts across borders, defined by ISO 13616. Validation uses MOD-97 check digits as specified in the standard.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/standard/81090.html">ISO 13616</see> — IBAN standard</description></item>
/// <item><description><see href="https://www.iban.com/">IBAN.com</see> — country formats and validation</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/International_Bank_Account_Number">Wikipedia — International Bank Account Number</see></description></item>
/// </list>
/// </remarks>
public sealed class Iban : IEquatable<Iban>, IComparable<Iban>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("IBAN", "IBAN", "🏦", ["https://www.iso.org/standard/81090.html", "https://www.iban.com/", "https://en.wikipedia.org/wiki/International_Bank_Account_Number"]);

    private const int MaxInputLength = 50;

    public const string DisplayName = "IBAN";
    public const string DisplayNameShort = "IBAN";

    private static readonly Regex NormalizedPattern = new(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$", RegexOptions.Compiled);

    private static readonly Dictionary<string, int> CountryIbanLengths = new(StringComparer.Ordinal)
    {
        ["AL"] = 28, ["AD"] = 24, ["AT"] = 20, ["AZ"] = 28, ["BE"] = 16, ["BA"] = 20,
        ["BG"] = 22, ["BH"] = 22, ["BR"] = 29, ["CH"] = 21, ["CY"] = 28, ["CZ"] = 24,
        ["DE"] = 22, ["DK"] = 18, ["DO"] = 28, ["EE"] = 20, ["ES"] = 24, ["FI"] = 18, ["FO"] = 18, ["FR"] = 27,
        ["GB"] = 22, ["GE"] = 22, ["GI"] = 23, ["GL"] = 18, ["GR"] = 27, ["GT"] = 28, ["HR"] = 21,
        ["HU"] = 28, ["IE"] = 22, ["IL"] = 23, ["IS"] = 26, ["IT"] = 27, ["IQ"] = 23, ["JO"] = 30, ["KW"] = 30, ["KZ"] = 20,
        ["LB"] = 28, ["LC"] = 32, ["LI"] = 21, ["LT"] = 20, ["LU"] = 20, ["LV"] = 21, ["MC"] = 27, ["MD"] = 24, ["ME"] = 22, ["MK"] = 19,
        ["MR"] = 27, ["MT"] = 31, ["NL"] = 18, ["NO"] = 15, ["PK"] = 24, ["PL"] = 28, ["PS"] = 29, ["PT"] = 25, ["QA"] = 29,
        ["RO"] = 24, ["RS"] = 22, ["SA"] = 24, ["SC"] = 31, ["SE"] = 24, ["SI"] = 19, ["SK"] = 24, ["SM"] = 27,
        ["ST"] = 25, ["SV"] = 28, ["TL"] = 23, ["TN"] = 24, ["TR"] = 26, ["UA"] = 29, ["VA"] = 22, ["VG"] = 24, ["XK"] = 20
    };

    public string Value { get; }
    public string CountryCode { get; }
    public string Formatted { get; }

    private Iban(string value, string countryCode, string formatted)
    {
        Value = value;
        CountryCode = countryCode;
        Formatted = formatted;
    }

    public static bool TryParse(string? input, out Iban? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalized = InputSanitization.KeepAsciiAlphanumericUppercase(InputSanitization.SanitizeInput(input!));
        if (normalized.Length > MaxInputLength) return false;
        if (!IsStructurallyValid(normalized)) return false;
        if (!HasValidCheckDigits(normalized)) return false;

        var countryCode = normalized.Substring(0, 2);
        var formatted = FormatValue(normalized);
        result = new Iban(normalized, countryCode, formatted);
        return true;
    }

    public static bool TryParse(string? input, string countryCode, out Iban? result)
    {
        result = null;
        if (!TryParse(input, out var parsed)) return false;
        if (!string.Equals(parsed!.CountryCode, countryCode, StringComparison.OrdinalIgnoreCase)) return false;
        result = parsed;
        return true;
    }

    public static Iban Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException($"Invalid {DisplayName}.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the IBAN grouped in blocks of four characters, for example <c>SE45 5000 0000 0583 9825 7466</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized IBAN without spaces, for example <c>SE4550000000058398257466</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized IBAN without spaces, for example <c>SE4550000000058398257466</c>.
    /// </summary>
    public string ToNormalizedString() => Value;
    public string ToDisplayString() => $"{DisplayName} {Formatted}";
    public string ToShortDisplayString() => $"{DisplayNameShort} {Formatted}";
    /// <summary>
    /// Returns the IBAN grouped in blocks of four characters, for example <c>SE45 5000 0000 0583 9825 7466</c>.
    /// </summary>
    public override string ToString() => Formatted;

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a well-formed IBAN.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var normalized = InputSanitization.KeepAsciiAlphanumericUppercase(InputSanitization.SanitizeInput(input!));

        if (normalized.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Input contains too many characters.", "Värdet innehåller för många tecken.");

        if (normalized.Length < 15 || normalized.Length > 34)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "IBAN must be 15–34 characters.", "IBAN måste vara 15–34 tecken.");

        if (!NormalizedPattern.IsMatch(normalized))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "IBAN has an invalid format.", "IBAN har ett ogiltigt format.");

        var countryCode = normalized.Substring(0, 2);
        if (!CountryIbanLengths.TryGetValue(countryCode, out var expectedLength))
            return ValidationResult.Invalid(input, ValidationErrorReason.UnknownCountryCode,
                "Unknown IBAN country code.", "Okänd IBAN-landskod.");

        if (normalized.Length != expectedLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLengthForCountry,
                "IBAN length is wrong for the specified country.", "IBAN-längden stämmer inte för det angivna landet.");

        if (!HasValidCheckDigits(normalized))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidCheckDigit,
                "Invalid IBAN check digits.", "Ogiltiga IBAN-kontrollsiffror.");

        return ValidationResult.Valid(input);
    }

    private static bool IsStructurallyValid(string normalized)
    {
        if (normalized.Length < 15 || normalized.Length > 34) return false;
        if (!NormalizedPattern.IsMatch(normalized)) return false;

        var countryCode = normalized.Substring(0, 2);
        if (!CountryIbanLengths.TryGetValue(countryCode, out var expectedLength)) return false;
        if (normalized.Length != expectedLength) return false;

        return true;
    }

    /// <summary>
    /// ISO 7064 MOD-97-10: move first 4 chars to end, convert A=10..Z=35, verify remainder == 1.
    /// </summary>
    private static bool HasValidCheckDigits(string normalized)
    {
        int remainder = 0;
        int len = normalized.Length;

        for (int i = 4; i < len + 4; i++)
        {
            char c = normalized[i % len];
            if (c >= '0' && c <= '9')
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
            }
            else
            {
                int value = c - 'A' + 10;
                int tens = value / 10;
                int ones = value % 10;
                remainder = (remainder * 10 + tens) % 97;
                remainder = (remainder * 10 + ones) % 97;
            }
        }

        return remainder == 1;
    }

    private static readonly Regex ScanPattern = new(
        @"\b[A-Z]{2}\d{2}\s?[A-Z0-9]{4}[\sA-Z0-9]{6,30}\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential IBANs.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real IBAN in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Iban>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Iban>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var iban)) continue;
            results.Add(new TextCandidate<Iban>(
                match.Index,
                match.Length,
                match.Value,
                nameof(Iban),
                TextCandidateCategory.Financial,
                iban!.ToNormalizedString(),
                iban.ToString(),
                iban.ToMaskedString(),
                TextMatchConfidence.High,
                iban));
        }
        return results;
    }

    private static string FormatValue(string normalized)
    {
        var len = normalized.Length;
        var groups = (len + 3) / 4;
        var resultLen = len + (groups - 1);

        var result = new char[resultLen];
        var src = 0;
        var dst = 0;
        var remainingInGroup = 4;

        while (src < len)
        {
            if (remainingInGroup == 0)
            {
                result[dst++] = ' ';
                remainingInGroup = 4;
            }

            result[dst++] = normalized[src++];
            remainingInGroup--;
        }

        return new string(result);
    }

    public bool Equals(Iban? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Iban other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Iban? a, Iban? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Iban? a, Iban? b) => !(a == b);
    public int CompareTo(Iban? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Iban left, Iban right) => left.CompareTo(right) < 0;
    public static bool operator >(Iban left, Iban right) => left.CompareTo(right) > 0;
    public static bool operator <=(Iban left, Iban right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Iban left, Iban right) => left.CompareTo(right) >= 0;
}
