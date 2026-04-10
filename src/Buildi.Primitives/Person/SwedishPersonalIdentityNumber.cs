using System.Text.RegularExpressions;
using ActiveLogin.Identity.Swedish;
using Buildi.Primitives;
using ActiveLogin.Identity.Swedish.Extensions;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;
using APin = ActiveLogin.Identity.Swedish.PersonalIdentityNumber;

namespace Buildi.Primitives.Person;

/// <summary>
/// A Swedish personal identity number (<c>personnummer</c>) is a national identification number assigned by Skatteverket at birth or immigration. The format is <c>YYYYMMDD-NNNC</c> where the last digit is a Luhn check digit.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://skatteverket.se/privat/folkbokforing/personnummer.4.3810a01c150939e893f18c29.html">Skatteverket — Personnummer</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Personnummer_i_Sverige">Wikipedia — Personnummer i Sverige</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Luhn_algorithm">Wikipedia — Luhn algorithm</see></description></item>
/// <item><description><see href="https://github.com/ActiveLogin/ActiveLogin.Identity">ActiveLogin.Identity</see> — underlying parsing and validation</description></item>
/// </list>
/// </remarks>
public sealed class SwedishPersonalIdentityNumber : IEquatable<SwedishPersonalIdentityNumber>, IComparable<SwedishPersonalIdentityNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Personal Identity Number", "Personnummer", "🪪", ["https://skatteverket.se/privat/folkbokforing/personnummer.4.3810a01c150939e893f18c29.html", "https://sv.wikipedia.org/wiki/Personnummer_i_Sverige", "https://en.wikipedia.org/wiki/Luhn_algorithm", "https://github.com/ActiveLogin/ActiveLogin.Identity"]);

    private const int MaxInputLength = 20;

    private readonly APin _inner;

    private SwedishPersonalIdentityNumber(APin inner) => _inner = inner;

    /// <summary>10-digit display form with separator (e.g. "990807-2391").</summary>
    public string Formatted => _inner.To10DigitString();

    /// <summary>12-digit canonical form (e.g. "199908072391").</summary>
    public string Value => _inner.To12DigitString();

    /// <summary>Estimated date of birth. Not guaranteed to be exact.</summary>
    public DateTime DateOfBirthHint => _inner.GetDateOfBirthHint();

    /// <summary>Estimated age in years. Not guaranteed to be exact.</summary>
    public int AgeHint => _inner.GetAgeHint();

    /// <summary>Estimated gender. Not guaranteed to be exact.</summary>
    public Gender GenderHint => _inner.GetGenderHint();

    public string To10DigitString() => _inner.To10DigitString();
    public string To12DigitString() => _inner.To12DigitString();

    public static bool TryParse(string? input, out SwedishPersonalIdentityNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        input = InputSanitization.SanitizeInput(input!).Trim();
        if (input.Length > MaxInputLength) return false;
        if (!APin.TryParse(input, out var pin)) return false;
        result = new SwedishPersonalIdentityNumber(pin);
        return true;
    }

    public static SwedishPersonalIdentityNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish personal identity number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the personal identity number in display format, for example <c>19990807-2391</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized personal identity number in 12-digit form, for example <c>199908072391</c>.
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
    /// Returns the normalized personal identity number in 12-digit form, for example <c>199908072391</c>.
    /// </summary>
    public string ToNormalizedString() => Value;
    /// <summary>
    /// Returns the personal identity number in display format, for example <c>19990807-2391</c>.
    /// </summary>
    public override string ToString() => Formatted;

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

        if (!APin.TryParse(sanitized, out _))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "Invalid personal identity number format.", "Ogiltigt format för personnummer.");

        return ValidationResult.Valid(input);
    }

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)(\d{6,8})([-+])(\d{4})(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swedish personal identity numbers.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real personal identity number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishPersonalIdentityNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishPersonalIdentityNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var pin)) continue;
            results.Add(new TextCandidate<SwedishPersonalIdentityNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishPersonalIdentityNumber),
                TextCandidateCategory.PersonalIdentifier,
                pin!.ToNormalizedString(),
                pin.ToString(),
                pin.ToMaskedString(),
                TextMatchConfidence.High,
                pin));
        }
        return results;
    }

    public bool Equals(SwedishPersonalIdentityNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is SwedishPersonalIdentityNumber other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishPersonalIdentityNumber? a, SwedishPersonalIdentityNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishPersonalIdentityNumber? a, SwedishPersonalIdentityNumber? b) => !(a == b);
    public int CompareTo(SwedishPersonalIdentityNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishPersonalIdentityNumber left, SwedishPersonalIdentityNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishPersonalIdentityNumber left, SwedishPersonalIdentityNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishPersonalIdentityNumber left, SwedishPersonalIdentityNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishPersonalIdentityNumber left, SwedishPersonalIdentityNumber right) => left.CompareTo(right) >= 0;
}
