using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A Swedish vehicle registration number (<c>registreringsnummer</c>) assigned by Transportstyrelsen.
/// Classic format is three letters followed by three digits (ABC 123). Since 2019 the last digit
/// may be replaced by a letter (ABC 12A) to expand capacity. Letters I, Q, V, Å, Ä, Ö are excluded
/// from all positions; O is additionally excluded from the final position to avoid confusion with zero.
/// The last numeric digit is used to derive the standard vehicle tax (<c>fordonsskatt</c>) payment
/// month according to Transportstyrelsen's mapping — this is a structural derivation, not a live
/// tax status lookup.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/en/road/vehicles/licence-plates/">Transportstyrelsen — Licence plates</see></description></item>
/// <item><description><see href="https://www.transportstyrelsen.se/en/road/vehicles/taxes-and-fees/vehicle-tax/">Transportstyrelsen — Vehicle tax</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Vehicle_registration_plates_of_Sweden">Wikipedia — Vehicle registration plates of Sweden</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishVehicleRegistrationNumber : IEquatable<SwedishVehicleRegistrationNumber>, IComparable<SwedishVehicleRegistrationNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Registration Number", "Registreringsnummer", "🚗", ["https://www.transportstyrelsen.se/en/road/vehicles/licence-plates/", "https://www.transportstyrelsen.se/en/road/vehicles/taxes-and-fees/vehicle-tax/", "https://en.wikipedia.org/wiki/Vehicle_registration_plates_of_Sweden"]);

    private const int MaxInputLength = 20;

    private static readonly Regex Pattern = new(
        @"^[A-HJ-NOPR-UW-Z]{3}\d{2}[\dA-HJ-NPR-UW-Z]$",
        RegexOptions.Compiled);

    private static readonly int[] TaxMonthByDigit = [3, 4, 5, 6, 8, 10, 11, 12, 1, 2];

    /// <summary>The registration number without spaces, e.g. <c>ABC123</c> or <c>ABC12A</c>.</summary>
    public string Value { get; }

    /// <summary>Display format with a space, e.g. <c>ABC 123</c> or <c>ABC 12A</c>.</summary>
    public string Formatted { get; }

    /// <summary>The three-letter prefix, e.g. <c>ABC</c>.</summary>
    public string Letters { get; }

    /// <summary>The trailing part after the letter prefix, e.g. <c>123</c> or <c>12A</c>.</summary>
    public string Suffix { get; }

    /// <summary><see langword="true"/> if the registration uses the 2019+ format where the last character is a letter.</summary>
    public bool IsNewFormat { get; }

    /// <summary>
    /// The numeric digit used to derive the standard tax payment month, according to
    /// Transportstyrelsen's payment-month mapping. For classic format (ABC 123) this is the last
    /// character; for new format (ABC 12A) it is the second-to-last character.
    /// This is a structural derivation from the registration number — it does not reflect
    /// the actual current tax status of any specific vehicle.
    /// </summary>
    public int TaxPaymentDigit { get; }

    /// <summary>
    /// The primary month (1–12) when vehicle tax (<c>fordonsskatt</c>) is typically due,
    /// derived from <see cref="TaxPaymentDigit"/> according to Transportstyrelsen's standard
    /// payment-month mapping. For example, digit 0 → March (3), digit 1 → April (4),
    /// digit 8 → January (1). If the annual tax exceeds 3 600 SEK, it is split into three
    /// payments four months apart. This is a structural derivation from the registration
    /// number — it does not reflect the actual current tax status of any specific vehicle.
    /// </summary>
    public int TaxPaymentMonth { get; }

    private SwedishVehicleRegistrationNumber(string value, string formatted, string letters, string suffix, bool isNewFormat, int taxDigit)
    {
        Value = value;
        Formatted = formatted;
        Letters = letters;
        Suffix = suffix;
        IsNewFormat = isNewFormat;
        TaxPaymentDigit = taxDigit;
        TaxPaymentMonth = TaxMonthByDigit[taxDigit];
    }

    public static bool TryParse(string? input, out SwedishVehicleRegistrationNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = InputSanitization.SanitizeInput(input!).Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        if (cleaned.Length > MaxInputLength) return false;
        if (cleaned.Length != 6) return false;
        if (!Pattern.IsMatch(cleaned)) return false;

        var letters = cleaned[..3];
        var suffix = cleaned[3..];
        var isNewFormat = char.IsLetter(cleaned[5]);
        var formatted = $"{letters} {suffix}";
        var taxDigit = isNewFormat ? cleaned[4] - '0' : cleaned[5] - '0';

        result = new SwedishVehicleRegistrationNumber(cleaned, formatted, letters, suffix, isNewFormat, taxDigit);
        return true;
    }

    public static SwedishVehicleRegistrationNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish vehicle registration number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the registration number in display format with a space, e.g. <c>ABC 123</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the registration number without spaces, e.g. <c>ABC123</c>.
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
    /// Validates the input and returns a <see cref="ValidationResult"/> describing why it is invalid,
    /// or a valid result when the input represents a correct Swedish vehicle registration number.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var cleaned = InputSanitization.SanitizeInput(input!).Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");

        if (cleaned.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Input contains too many characters.", "Värdet innehåller för många tecken.");

        if (cleaned.Length != 6)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "Registration number must be exactly 6 characters.", "Registreringsnumret måste vara exakt 6 tecken.");

        if (!Pattern.IsMatch(cleaned))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "Registration number must be three letters followed by two digits and one digit or letter (I, Q, V, Å, Ä, Ö excluded).",
                "Registreringsnumret måste vara tre bokstäver följt av två siffror och en siffra eller bokstav (I, Q, V, Å, Ä, Ö exkluderade).");

        return ValidationResult.Valid(input);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the registration number without spaces, e.g. <c>ABC123</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the registration number in display format with a space, e.g. <c>ABC 123</c>.</summary>
    public override string ToString() => Formatted;

    private static readonly Regex ScanPattern = new(
        @"\b[A-HJ-NOPR-UW-Z]{3}\s?\d{2}[\dA-HJ-NPR-UW-Z]\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swedish vehicle registration numbers.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real registration number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishVehicleRegistrationNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var upper = text.ToUpperInvariant();
        var results = new List<TextCandidate<SwedishVehicleRegistrationNumber>>();
        foreach (Match match in ScanPattern.Matches(upper))
        {
            if (!TryParse(match.Value, out var reg)) continue;
            results.Add(new TextCandidate<SwedishVehicleRegistrationNumber>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(SwedishVehicleRegistrationNumber),
                TextCandidateCategory.Vehicle,
                reg!.ToNormalizedString(),
                reg.ToString(),
                reg.ToMaskedString(),
                TextMatchConfidence.Medium,
                reg));
        }
        return results;
    }

    public bool Equals(SwedishVehicleRegistrationNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is SwedishVehicleRegistrationNumber other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishVehicleRegistrationNumber? a, SwedishVehicleRegistrationNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishVehicleRegistrationNumber? a, SwedishVehicleRegistrationNumber? b) => !(a == b);
    public int CompareTo(SwedishVehicleRegistrationNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishVehicleRegistrationNumber left, SwedishVehicleRegistrationNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishVehicleRegistrationNumber left, SwedishVehicleRegistrationNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishVehicleRegistrationNumber left, SwedishVehicleRegistrationNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishVehicleRegistrationNumber left, SwedishVehicleRegistrationNumber right) => left.CompareTo(right) >= 0;
}
