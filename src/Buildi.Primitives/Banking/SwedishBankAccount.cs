using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Banking;

/// <summary>
/// A Swedish bank account (<c>bankkonto</c>) is identified by a clearing number (4–5 digits indicating bank and branch) followed by an account number. Clearing number ranges and account length rules are defined by BSAB.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer#clearingTable">BSAB — Clearingnummer</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Clearingnummer">Wikipedia — Clearingnummer</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishBankAccount : IEquatable<SwedishBankAccount>, IComparable<SwedishBankAccount>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Bank Account", "Bankkonto", "🏧", ["https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer#clearingTable", "https://sv.wikipedia.org/wiki/Clearingnummer"]);

    private const int MaxInputLength = 40;

    public const string DisplayName = "Bankkonto";

    private static readonly Regex Digits11To15 = new(@"^\d{11,15}$", RegexOptions.Compiled);

    public SwedishBankClearingNumber Clearing { get; }
    public string ClearingNumber => Clearing.Digits;
    public string AccountNumber { get; }
    public string? BankName => Clearing.BankName;
    public SwedishBank? Bank => Clearing.Bank;
    public string Formatted { get; }

    private SwedishBankAccount(SwedishBankClearingNumber clearing, string accountNumber, string formatted)
    {
        Clearing = clearing;
        AccountNumber = accountNumber;
        Formatted = formatted;
    }

    public static bool TryParse(string? input, out SwedishBankAccount? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length > MaxInputLength) return false;
        if (!Digits11To15.IsMatch(digits)) return false;
        if (!IsValidLength(digits)) return false;

        GetClearingAndAccount(digits, out var clearingDigits, out var account);
        if (!Banking.SwedishBankClearingNumber.TryParse(clearingDigits, out var clearing)) return false;
        var formatted = $"{clearingDigits}-{account}";

        result = new SwedishBankAccount(clearing!, account, formatted);
        return true;
    }

    public static SwedishBankAccount Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException($"Invalid {DisplayName} number.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the Swedish bank account in display format with a hyphen between clearing number and account number,
    /// for example <c>5100-0123456</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized Swedish bank account without separators, for example <c>51000123456</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.ClearingNumber + r.AccountNumber;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the normalized Swedish bank account without separators, for example <c>51000123456</c>.
    /// </summary>
    public string ToNormalizedString() => ClearingNumber + AccountNumber;
    public string ToDisplayString() => $"{DisplayName} {Formatted}";
    /// <summary>
    /// Returns the Swedish bank account in display format with a hyphen, for example <c>5100-0123456</c>.
    /// </summary>
    public override string ToString() => Formatted;

    /// <summary>
    /// Returns a <see cref="ValidationResult"/> describing why the input is invalid,
    /// or a valid result when the input is a valid Swedish bank account.
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

        if (!Digits11To15.IsMatch(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "Bank account must be 11–15 digits.", "Bankkonto måste vara 11–15 siffror.");

        if (!IsValidLength(digits))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidAccountLengthForBank,
                "Account length is wrong for the identified bank.", "Kontolängden stämmer inte för den identifierade banken.");

        GetClearingAndAccount(digits, out var clearingDigits, out _);
        if (!Banking.SwedishBankClearingNumber.TryParse(clearingDigits, out _))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidClearingNumber,
                "The clearing number part is invalid.", "Clearingnummerdelen är ogiltig.");

        return ValidationResult.Valid(input);
    }

    private static bool IsValidLength(string digits)
    {
        var length = digits.Length;
        var c4 = ParseIntSafe(digits.Substring(0, 4));

        if (BankResolver.IsHandelsbanken(c4)) return length >= 12 && length <= 13;
        if (BankResolver.IsNordeaPersonkonto(c4)) return length == 14;
        if (BankResolver.IsNordeaPlusgirot(c4)) return length == 14;
        if (BankResolver.IsSparbankenSyd(c4)) return length == 14;
        if (BankResolver.IsRiksgalden(c4)) return length == 14;
        if (BankResolver.IsSwedbank10DigitsAccountNumber(c4)) return length >= 7 && length <= 15;

        var (bank, _) = BankResolver.Resolve(c4);
        return bank != null && length == 11;
    }

    private static void GetClearingAndAccount(string digits, out string clearing, out string account)
    {
        if (digits[0] == '8')
        {
            clearing = digits.Substring(0, 5);
            account = digits.Substring(5);
        }
        else
        {
            clearing = digits.Substring(0, 4);
            account = digits.Substring(4);
        }
    }

    private static int ParseIntSafe(string s) =>
        (s[0] - '0') * 1000 + (s[1] - '0') * 100 + (s[2] - '0') * 10 + (s[3] - '0');

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)\d{4,5}[\s\-]?\d{5,10}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swedish bank account numbers (clearing + account).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real bank account in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishBankAccount>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishBankAccount>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var account)) continue;
            results.Add(new TextCandidate<SwedishBankAccount>(
                match.Index,
                match.Length,
                match.Value,
                nameof(SwedishBankAccount),
                TextCandidateCategory.Financial,
                account!.ToNormalizedString(),
                account.ToString(),
                account.ToMaskedString(),
                TextMatchConfidence.Low,
                account));
        }
        return results;
    }

    public bool Equals(SwedishBankAccount? other) => other is not null && ClearingNumber == other.ClearingNumber && AccountNumber == other.AccountNumber;
    public override bool Equals(object? obj) => obj is SwedishBankAccount other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(ClearingNumber, AccountNumber);
    public static bool operator ==(SwedishBankAccount? a, SwedishBankAccount? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishBankAccount? a, SwedishBankAccount? b) => !(a == b);
    public int CompareTo(SwedishBankAccount? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishBankAccount left, SwedishBankAccount right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishBankAccount left, SwedishBankAccount right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishBankAccount left, SwedishBankAccount right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishBankAccount left, SwedishBankAccount right) => left.CompareTo(right) >= 0;
}
