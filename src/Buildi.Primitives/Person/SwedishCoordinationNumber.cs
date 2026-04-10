using System.Text.RegularExpressions;
using ActiveLogin.Identity.Swedish;
using Buildi.Primitives;
using ActiveLogin.Identity.Swedish.Extensions;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;
using ACn = ActiveLogin.Identity.Swedish.CoordinationNumber;

namespace Buildi.Primitives.Person;

/// <summary>
/// A Swedish coordination number (<c>samordningsnummer</c>) is assigned by Skatteverket to individuals not registered in the Swedish population register. It uses the same format as a personal identity number but with 60 added to the birth day.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://skatteverket.se/privat/folkbokforing/samordningsnummer.4.5c281c7015abecc2e201130b.html">Skatteverket — Samordningsnummer</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Samordningsnummer">Wikipedia — Samordningsnummer</see></description></item>
/// <item><description><see href="https://github.com/ActiveLogin/ActiveLogin.Identity">ActiveLogin.Identity</see> — underlying parsing and validation</description></item>
/// </list>
/// </remarks>
public sealed class SwedishCoordinationNumber : IEquatable<SwedishCoordinationNumber>, IComparable<SwedishCoordinationNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Coordination Number", "Samordningsnummer", "🆔", ["https://skatteverket.se/privat/folkbokforing/samordningsnummer.4.5c281c7015abecc2e201130b.html", "https://sv.wikipedia.org/wiki/Samordningsnummer", "https://github.com/ActiveLogin/ActiveLogin.Identity"]);

    private const int MaxInputLength = 20;

    private readonly ACn _inner;

    private SwedishCoordinationNumber(ACn inner) => _inner = inner;

    /// <summary>10-digit display form with separator (e.g. "680164-2395").</summary>
    public string Formatted => _inner.To10DigitString();

    /// <summary>12-digit canonical form (e.g. "196801642395").</summary>
    public string Value => _inner.To12DigitString();

    /// <summary>
    /// The real day of date of birth (coordination day minus 60).
    /// Can be 0 when the day is unknown.
    /// </summary>
    public int RealDay => _inner.RealDay;

    /// <summary>Estimated date of birth, or null if unknown. Not guaranteed to be exact.</summary>
    public DateTime? DateOfBirthHint => _inner.GetDateOfBirthHint();

    /// <summary>Estimated age in years, or null if unknown. Not guaranteed to be exact.</summary>
    public int? AgeHint => _inner.GetAgeHint();

    /// <summary>Estimated gender. Not guaranteed to be exact.</summary>
    public Gender GenderHint => _inner.GetGenderHint();

    public string To10DigitString() => _inner.To10DigitString();
    public string To12DigitString() => _inner.To12DigitString();

    public static bool TryParse(string? input, out SwedishCoordinationNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        input = InputSanitization.SanitizeInput(input!).Trim();
        if (input.Length > MaxInputLength) return false;
        if (!ACn.TryParse(input, out var cn)) return false;
        result = new SwedishCoordinationNumber(cn);
        return true;
    }

    public static SwedishCoordinationNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish coordination number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the coordination number in display format, for example <c>19680164-2395</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized coordination number in 12-digit form, for example <c>196801642395</c>.
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
    /// Returns the normalized coordination number in 12-digit form, for example <c>196801642395</c>.
    /// </summary>
    public string ToNormalizedString() => Value;
    /// <summary>
    /// Returns the coordination number in display format, for example <c>19680164-2395</c>.
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

        if (!ACn.TryParse(sanitized, out _))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidFormat,
                "Invalid coordination number format.", "Ogiltigt format för samordningsnummer.");

        return ValidationResult.Valid(input);
    }

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)(\d{6,8})([-+])(\d{4})(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swedish coordination numbers.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real coordination number in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishCoordinationNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishCoordinationNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var cn)) continue;
            results.Add(new TextCandidate<SwedishCoordinationNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishCoordinationNumber),
                TextCandidateCategory.PersonalIdentifier,
                cn!.ToNormalizedString(),
                cn.ToString(),
                cn.ToMaskedString(),
                TextMatchConfidence.High,
                cn));
        }
        return results;
    }

    public bool Equals(SwedishCoordinationNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is SwedishCoordinationNumber other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishCoordinationNumber? a, SwedishCoordinationNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishCoordinationNumber? a, SwedishCoordinationNumber? b) => !(a == b);
    public int CompareTo(SwedishCoordinationNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishCoordinationNumber left, SwedishCoordinationNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishCoordinationNumber left, SwedishCoordinationNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishCoordinationNumber left, SwedishCoordinationNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishCoordinationNumber left, SwedishCoordinationNumber right) => left.CompareTo(right) >= 0;
}
