using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Banking;

/// <summary>
/// A Bankgiro number (<c>bankgironummer</c>) is a payment routing identifier in the Swedish Bankgiro system, operated by Bankgirot. It directs incoming payments to a bank account without exposing the account number itself.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bankgirot.se/en/services/incoming-payments/bankgiro-number/">Bankgirot — Bankgiro number</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Bankgirot">Wikipedia — Bankgirot</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishBankgiroNumber : IEquatable<SwedishBankgiroNumber>, IComparable<SwedishBankgiroNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Bankgiro Number", "Bankgironummer", "💳", ["https://www.bankgirot.se/en/services/incoming-payments/bankgiro-number/", "https://sv.wikipedia.org/wiki/Bankgirot"]);

    private const int MaxInputLength = 20;

    public const string DisplayName = "Bankgiro";
    public const string DisplayNameShort = "BG";

    private static readonly Regex Digits7Or8 = new(@"^\d{7,8}$", RegexOptions.Compiled);

    public string Digits { get; }
    public string Formatted { get; }

    private SwedishBankgiroNumber(string digits, string formatted)
    {
        Digits = digits;
        Formatted = formatted;
    }

    public static bool TryParse(string? input, out SwedishBankgiroNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length > MaxInputLength) return false;
        if (!Digits7Or8.IsMatch(digits)) return false;
        if (!Luhn.IsValid(digits)) return false;

        var formatted = digits.Length == 7
            ? $"{digits.Substring(0, 3)}-{digits.Substring(3, 4)}"
            : $"{digits.Substring(0, 4)}-{digits.Substring(4, 4)}";

        result = new SwedishBankgiroNumber(digits, formatted);
        return true;
    }

    public static SwedishBankgiroNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException($"Invalid {DisplayName} number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the Bankgiro in display format with a hyphen, for example <c>5805-6201</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the Bankgiro number in canonical hyphenated form, for example <c>5805-6201</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Formatted;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the Bankgiro number in canonical hyphenated form, for example <c>5805-6201</c>.
    /// </summary>
    public string ToNormalizedString() => Formatted;
    public string ToDisplayString() => $"{DisplayName} {Formatted}";
    public string ToShortDisplayString() => $"{DisplayNameShort} {Formatted}";
    /// <summary>
    /// Returns the Bankgiro in display format with a hyphen, for example <c>5805-6201</c>.
    /// </summary>
    public override string ToString() => Formatted;

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a valid Bankgiro number.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));

        if (digits.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Input contains too many characters.", "Värdet innehåller för många tecken.");

        if (!Digits7Or8.IsMatch(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "Bankgiro number must be 7 or 8 digits.", "Bankgironummer måste vara 7 eller 8 siffror.");

        if (!Luhn.IsValid(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidCheckDigit,
                "Invalid Luhn check digit.", "Ogiltig kontrollsiffra.");

        return ValidationResult.Valid(input);
    }

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{3,4}-\d{4}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Bankgiro numbers (hyphenated format only, e.g. <c>5805-6201</c>).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real Bankgiro number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishBankgiroNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishBankgiroNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var bg)) continue;
            results.Add(new TextCandidate<SwedishBankgiroNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishBankgiroNumber),
                TextCandidateCategory.Financial,
                bg!.ToNormalizedString(),
                bg.ToString(),
                bg.ToMaskedString(),
                TextMatchConfidence.High,
                bg));
        }
        return results;
    }

    public bool Equals(SwedishBankgiroNumber? other) => other is not null && Formatted == other.Formatted;
    public override bool Equals(object? obj) => obj is SwedishBankgiroNumber other && Equals(other);
    public override int GetHashCode() => Formatted.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishBankgiroNumber? a, SwedishBankgiroNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishBankgiroNumber? a, SwedishBankgiroNumber? b) => !(a == b);
    public int CompareTo(SwedishBankgiroNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishBankgiroNumber left, SwedishBankgiroNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishBankgiroNumber left, SwedishBankgiroNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishBankgiroNumber left, SwedishBankgiroNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishBankgiroNumber left, SwedishBankgiroNumber right) => left.CompareTo(right) >= 0;
}
