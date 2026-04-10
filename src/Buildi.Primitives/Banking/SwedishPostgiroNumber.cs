using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Banking;

/// <summary>
/// A Plusgiro number (<c>plusgironummer</c>) is a payment identifier in the Swedish Plusgiro system, originally operated by the postal service and now managed by Bankgirot. Numbers are 2–8 digits with a Luhn check digit.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bankgirot.se/">Bankgirot</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Plusgirot">Wikipedia — Plusgirot</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishPostgiroNumber : IEquatable<SwedishPostgiroNumber>, IComparable<SwedishPostgiroNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Plusgiro Number", "Plusgironummer", "📮", ["https://www.bankgirot.se/", "https://sv.wikipedia.org/wiki/Plusgirot"]);

    private const int MaxInputLength = 20;

    public const string DisplayName = "Plusgiro";
    public const string DisplayNameShort = "PG";

    private static readonly Regex Digits2To8 = new(@"^\d{2,8}$", RegexOptions.Compiled);

    public string Digits { get; }
    public string Formatted { get; }

    private SwedishPostgiroNumber(string digits, string formatted)
    {
        Digits = digits;
        Formatted = formatted;
    }

    public static bool TryParse(string? input, out SwedishPostgiroNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length > MaxInputLength) return false;
        if (!Digits2To8.IsMatch(digits)) return false;
        if (!Luhn.IsValid(digits)) return false;

        var basePart = digits.Substring(0, digits.Length - 1);
        var control = digits.Substring(digits.Length - 1, 1);
        var formatted = $"{basePart}-{control}";

        result = new SwedishPostgiroNumber(digits, formatted);
        return true;
    }

    public static SwedishPostgiroNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException($"Invalid {DisplayName} number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the Plusgiro in display format with a hyphen before the control digit, for example <c>1234567-9</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the Plusgiro number in canonical form with hyphen before the check digit, for example <c>1234567-9</c>.
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
    /// Returns the Plusgiro number in canonical form with hyphen before the check digit, for example <c>1234567-9</c>.
    /// </summary>
    public string ToNormalizedString() => Formatted;
    public string ToDisplayString() => $"{DisplayName} {Formatted}";
    public string ToShortDisplayString() => $"{DisplayNameShort} {Formatted}";
    /// <summary>
    /// Returns the Plusgiro in display format with a hyphen before the control digit, for example <c>1234567-9</c>.
    /// </summary>
    public override string ToString() => Formatted;

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a valid Plusgiro number.
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

        if (!Digits2To8.IsMatch(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "Plusgiro number must be 2–8 digits.", "Plusgironummer måste vara 2–8 siffror.");

        if (!Luhn.IsValid(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidCheckDigit,
                "Invalid Luhn check digit.", "Ogiltig kontrollsiffra.");

        return ValidationResult.Valid(input);
    }

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{1,7}-\d(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Plusgiro numbers (hyphenated format only, e.g. <c>1234567-9</c>).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real Plusgiro number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishPostgiroNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishPostgiroNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var pg)) continue;
            results.Add(new TextCandidate<SwedishPostgiroNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishPostgiroNumber),
                TextCandidateCategory.Financial,
                pg!.ToNormalizedString(),
                pg.ToString(),
                pg.ToMaskedString(),
                TextMatchConfidence.Medium,
                pg));
        }
        return results;
    }

    public bool Equals(SwedishPostgiroNumber? other) => other is not null && Formatted == other.Formatted;
    public override bool Equals(object? obj) => obj is SwedishPostgiroNumber other && Equals(other);
    public override int GetHashCode() => Formatted.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishPostgiroNumber? a, SwedishPostgiroNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishPostgiroNumber? a, SwedishPostgiroNumber? b) => !(a == b);
    public int CompareTo(SwedishPostgiroNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishPostgiroNumber left, SwedishPostgiroNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishPostgiroNumber left, SwedishPostgiroNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishPostgiroNumber left, SwedishPostgiroNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishPostgiroNumber left, SwedishPostgiroNumber right) => left.CompareTo(right) >= 0;
}
