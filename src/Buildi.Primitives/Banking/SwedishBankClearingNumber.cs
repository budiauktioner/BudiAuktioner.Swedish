using Buildi.Primitives;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Banking;

/// <summary>
/// A Swedish clearing number (<c>clearingnummer</c>) identifies a bank and branch in the Swedish
/// payments infrastructure. Most clearing numbers are 4 digits; Swedbank's series 8 uses 5 digits.
/// Parsing resolves the associated bank when the clearing number falls within a known range.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer#clearingTable">BSAB — Clearingnummer</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Clearingnummer">Wikipedia — Clearingnummer</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishBankClearingNumber : IEquatable<SwedishBankClearingNumber>, IComparable<SwedishBankClearingNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Bank Clearing Number", "Clearingnummer", "🔑", ["https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer#clearingTable", "https://sv.wikipedia.org/wiki/Clearingnummer"]);

    private const int MaxInputLength = 20;

    public string Digits { get; }
    public SwedishBank Bank { get; }
    public string BankName { get; }

    private SwedishBankClearingNumber(string digits, SwedishBank bank, string bankName)
    {
        Digits = digits;
        Bank = bank;
        BankName = bankName;
    }

    public static bool TryParse(string? input, out SwedishBankClearingNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length > MaxInputLength) return false;
        if (digits.Length < 4 || digits.Length > 5) return false;

        if (digits.Length == 5 && digits[0] != '8') return false;
        if (digits.Length == 4 && digits[0] == '8') return false;

        var clearing4 = ParseInt4(digits);
        var (bank, bankName) = BankResolver.Resolve(clearing4);
        if (bank == null || bankName == null) return false;

        result = new SwedishBankClearingNumber(digits, bank.Value, bankName);
        return true;
    }

    public static SwedishBankClearingNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid clearing number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the clearing number digits, for example <c>5100</c> or <c>8327-9</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString() : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the clearing number as digits only, for example <c>5100</c> or <c>83279</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Digits;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the clearing number as digits only, for example <c>5100</c> or <c>83279</c>.
    /// </summary>
    public string ToNormalizedString() => Digits;

    /// <summary>
    /// Returns the clearing number in display format, for example <c>5100</c> or <c>8327-9</c>.
    /// </summary>
    public override string ToString() => Digits.Length == 5 ? $"{Digits[..4]}-{Digits[4]}" : Digits;

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a recognized clearing number.
    /// </summary>
    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a recognized clearing number.
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

        if (digits.Length < 4 || digits.Length > 5)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "Clearing number must be 4 or 5 digits.", "Clearingnummer måste vara 4 eller 5 siffror.");

        if (digits.Length == 5 && digits[0] != '8')
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidSwedbankFormat,
                "A 5-digit clearing number must start with 8 (Swedbank).", "Ett 5-siffrigt clearingnummer måste börja med 8 (Swedbank).");

        if (digits.Length == 4 && digits[0] == '8')
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidSwedbankFormat,
                "Clearing numbers starting with 8 must be 5 digits (Swedbank).", "Clearingnummer som börjar med 8 måste vara 5 siffror (Swedbank).");

        var clearing4 = ParseInt4(digits);
        var (bank, bankName) = BankResolver.Resolve(clearing4);
        if (bank == null || bankName == null)
            return ValidationResult.Invalid(input, ValidationErrorReason.UnknownClearingRange,
                "Clearing number does not match any known bank.", "Clearingnumret matchar ingen känd bank.");

        return ValidationResult.Valid(input);
    }

    private static int ParseInt4(string s) =>
        (s[0] - '0') * 1000 + (s[1] - '0') * 100 + (s[2] - '0') * 10 + (s[3] - '0');

    public bool Equals(SwedishBankClearingNumber? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is SwedishBankClearingNumber other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishBankClearingNumber? a, SwedishBankClearingNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishBankClearingNumber? a, SwedishBankClearingNumber? b) => !(a == b);
    public int CompareTo(SwedishBankClearingNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishBankClearingNumber left, SwedishBankClearingNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishBankClearingNumber left, SwedishBankClearingNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishBankClearingNumber left, SwedishBankClearingNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishBankClearingNumber left, SwedishBankClearingNumber right) => left.CompareTo(right) >= 0;
}
