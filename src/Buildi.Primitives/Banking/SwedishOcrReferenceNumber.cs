using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Banking;

/// <summary>
/// An OCR reference number (<c>OCR-nummer</c> / <c>OCR-referens</c>) is the numeric reference commonly used in Swedish payment flows to reconcile incoming payments. Bankgirot supports MOD10 check-digit references as well as agreement-specific variants such as variable length digit and fixed-length control.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bankgirot.se/en/services/incoming-payments/bankgiro-receivables/ocr-reference-control/">Bankgirot — OCR reference control</see></description></item>
/// <item><description><see href="https://www.bankgirot.se/en/services/incoming-payments/bankgiro-receivables/technical-information/">Bankgirot — Technical information</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishOcrReferenceNumber : IEquatable<SwedishOcrReferenceNumber>, IComparable<SwedishOcrReferenceNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("OCR Reference Number", "OCR-referens", "📝", ["https://www.bankgirot.se/en/services/incoming-payments/bankgiro-receivables/ocr-reference-control/", "https://www.bankgirot.se/en/services/incoming-payments/bankgiro-receivables/technical-information/"]);

    public const string DisplayName = "OCR";
    public const string DisplayNameShort = "OCR";

    public string Value { get; }
    public string Formatted { get; }
    public int Length => Value.Length;
    public int CheckDigit => Value[^1] - '0';
    public int? LengthDigit { get; }

    private SwedishOcrReferenceNumber(string value, int? lengthDigit)
    {
        Value = value;
        Formatted = value;
        LengthDigit = lengthDigit;
    }

    public static bool TryParse(string? input, out SwedishOcrReferenceNumber? result)
        => TryParse(input, OcrReferenceOptions.Default, out result);

    public static bool TryParse(string? input, OcrReferenceOptions options, out SwedishOcrReferenceNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length < 2 || digits.Length > 25) return false;
        if (!Luhn.IsValid(digits)) return false;
        if (!ValidateByOptions(digits, options, out var lengthDigit)) return false;

        result = new SwedishOcrReferenceNumber(digits, lengthDigit);
        return true;
    }

    public static bool TryParseVariableLengthDigit(string? input, out SwedishOcrReferenceNumber? result)
        => TryParse(input, OcrReferenceOptions.VariableLengthDigit, out result);

    public static bool TryParseFixedLength(string? input, int fixedLength, out SwedishOcrReferenceNumber? result)
        => TryParse(input, OcrReferenceOptions.FixedLength(fixedLength), out result);

    public static bool TryParseFixedLength(string? input, int[] fixedLengths, out SwedishOcrReferenceNumber? result)
        => TryParse(input, OcrReferenceOptions.FixedLength(fixedLengths), out result);

    public static SwedishOcrReferenceNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException($"Invalid {DisplayName} reference number.", nameof(input));
        return result!;
    }

    public static SwedishOcrReferenceNumber Parse(string input, OcrReferenceOptions options)
    {
        if (!TryParse(input, options, out var result))
            throw new ArgumentException($"Invalid {DisplayName} reference number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    public static bool IsValid(string? input, OcrReferenceOptions options) => TryParse(input, options, out _);
    /// <summary>
    /// Returns the OCR reference as digits only, for example <c>123455</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized OCR reference as digits only, for example <c>123455</c>.
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
    /// Returns the normalized OCR reference as digits only, for example <c>123455</c>.
    /// </summary>
    public string ToNormalizedString() => Value;
    public string ToDisplayString() => $"{DisplayName} {Formatted}";
    public string ToShortDisplayString() => $"{DisplayNameShort} {Formatted}";
    /// <summary>
    /// Returns the OCR reference as digits only, for example <c>123455</c>.
    /// </summary>
    public override string ToString() => Formatted;

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a valid OCR reference number.
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));

        if (digits.Length < 2 || digits.Length > 25)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "OCR reference must be 2–25 digits.", "OCR-referens måste vara 2–25 siffror.");

        if (!Luhn.IsValid(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidCheckDigit,
                "Invalid Luhn check digit.", "Ogiltig kontrollsiffra.");

        return ValidationResult.Valid(input);
    }

    private static bool ValidateByOptions(string digits, OcrReferenceOptions options, out int? lengthDigit)
    {
        lengthDigit = null;

        switch (options.ControlType)
        {
            case OcrReferenceControlType.CheckDigitOnly:
                return true;

            case OcrReferenceControlType.VariableLengthDigit:
            {
                if (digits.Length < 3) return false;

                var expected = digits.Length % 10;
                var actual = digits[^2] - '0';
                if (actual != expected) return false;

                lengthDigit = actual;
                return true;
            }

            case OcrReferenceControlType.FixedLength:
                return options.AllowedLengths.Length > 0 && options.AllowedLengths.Contains(digits.Length);

            default:
                return false;
        }
    }

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{2,25}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential OCR reference numbers (digit sequences with valid Luhn).
    /// Results are heuristic-based candidates and have a high false-positive rate since OCR references
    /// are plain digit sequences. No guarantee is made that a candidate represents a real OCR reference
    /// in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishOcrReferenceNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishOcrReferenceNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var ocr)) continue;
            results.Add(new TextCandidate<SwedishOcrReferenceNumber>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishOcrReferenceNumber),
                TextCandidateCategory.Financial,
                ocr!.ToNormalizedString(),
                ocr.ToString(),
                ocr.ToMaskedString(),
                TextMatchConfidence.Low,
                ocr));
        }
        return results;
    }

    public bool Equals(SwedishOcrReferenceNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is SwedishOcrReferenceNumber other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishOcrReferenceNumber? a, SwedishOcrReferenceNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishOcrReferenceNumber? a, SwedishOcrReferenceNumber? b) => !(a == b);
    public int CompareTo(SwedishOcrReferenceNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishOcrReferenceNumber left, SwedishOcrReferenceNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishOcrReferenceNumber left, SwedishOcrReferenceNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishOcrReferenceNumber left, SwedishOcrReferenceNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishOcrReferenceNumber left, SwedishOcrReferenceNumber right) => left.CompareTo(right) >= 0;
}

public enum OcrReferenceControlType
{
    CheckDigitOnly = 0,
    VariableLengthDigit = 1,
    FixedLength = 2,
}

public sealed class OcrReferenceOptions
{
    public static OcrReferenceOptions Default { get; } = new();
    public static OcrReferenceOptions VariableLengthDigit { get; } = new(OcrReferenceControlType.VariableLengthDigit);

    public OcrReferenceControlType ControlType { get; }
    public int[] AllowedLengths { get; }

    public OcrReferenceOptions(OcrReferenceControlType controlType = OcrReferenceControlType.CheckDigitOnly, params int[] allowedLengths)
    {
        ControlType = controlType;
        AllowedLengths = allowedLengths ?? [];
    }

    public static OcrReferenceOptions FixedLength(params int[] allowedLengths)
        => new(OcrReferenceControlType.FixedLength, allowedLengths);
}
