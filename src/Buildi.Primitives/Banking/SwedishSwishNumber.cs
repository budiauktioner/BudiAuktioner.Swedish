using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Contact;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Banking;

/// <summary>
/// A Swish payment number (<c>Swish-nummer</c>) used on the Swedish Swish platform.
/// Accepts three forms:
/// <list type="bullet">
/// <item><description><b>Swish 123</b> — a 10-digit corporate number starting with <c>123</c>, e.g. <c>1236652895</c></description></item>
/// <item><description><b>90-number</b> — a 7-digit short form starting with <c>90X</c> (Insamlingskontroll / 90-konto charities), expanded to <c>123</c> + 7 digits, e.g. <c>9020033</c> → <c>1239020033</c></description></item>
/// <item><description><b>Mobile number</b> — a Swedish mobile phone number for private Swish, delegated to <see cref="PhoneNumber"/></description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.swish.nu/faq/company/what-is-the-format-of-the-swish-number">Swish FAQ — Number format</see></description></item>
/// <item><description><see href="https://github.com/cisene/swish-123">cisene/swish-123</see> — community catalog of Swish 123 numbers</description></item>
/// <item><description><see href="https://www.insamlingskontroll.se/">Insamlingskontroll</see> — 90-konto organizations</description></item>
/// </list>
/// </remarks>
public sealed class SwedishSwishNumber : IEquatable<SwedishSwishNumber>, IComparable<SwedishSwishNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Swish Number", "Swish-nummer", "📱", ["https://www.swish.nu/faq/company/what-is-the-format-of-the-swish-number", "https://github.com/cisene/swish-123", "https://www.insamlingskontroll.se/"]);

    private const int MaxInputLength = 50;

    private static readonly Regex Digits7Starting90 = new(@"^90\d{5}$", RegexOptions.Compiled);

    /// <summary>
    /// Normalized digit string. For Swish 123 numbers: 10 digits (e.g. <c>1236652895</c>).
    /// For mobile numbers: the <see cref="Contact.PhoneNumber.Digits"/> form (e.g. <c>0046701234567</c>).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Human-readable display form. For Swish 123: <c>123-665 28 95</c>.
    /// For 90-numbers: <c>902 00 33</c>. For mobile numbers: local phone format.
    /// </summary>
    public string Formatted { get; }

    /// <summary><see langword="true"/> for corporate Swish 123 numbers (including 90-numbers).</summary>
    public bool IsSwish123 { get; }

    /// <summary><see langword="true"/> for phone-based private Swish numbers.</summary>
    public bool IsMobileNumber { get; }

    /// <summary><see langword="true"/> when the number belongs to an Insamlingskontroll 90-konto organization (digits start with <c>1239</c>).</summary>
    public bool Is90Number { get; }

    /// <summary>
    /// The underlying <see cref="Contact.PhoneNumber"/> when <see cref="IsMobileNumber"/> is <see langword="true"/>;
    /// otherwise <see langword="null"/>.
    /// </summary>
    public PhoneNumber? PhoneNumber { get; }

    private SwedishSwishNumber(string value, string formatted, bool isSwish123, bool isMobileNumber, bool is90Number, PhoneNumber? phoneNumber)
    {
        Value = value;
        Formatted = formatted;
        IsSwish123 = isSwish123;
        IsMobileNumber = isMobileNumber;
        Is90Number = is90Number;
        PhoneNumber = phoneNumber;
    }

    /// <summary>
    /// Attempts to parse a Swish number. Accepts Swish 123 numbers (10 digits starting with <c>123</c>),
    /// 90-numbers (7 digits starting with <c>90X</c>), and Swedish mobile phone numbers.
    /// </summary>
    public static bool TryParse(string? input, out SwedishSwishNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = InputSanitization.SanitizeInput(input!).Trim();
        if (cleaned.Length > MaxInputLength) return false;

        var digits = InputSanitization.KeepDigits(cleaned);

        if (Digits7Starting90.IsMatch(digits))
        {
            var expanded = "123" + digits;
            result = new SwedishSwishNumber(
                expanded,
                Format90Number(digits),
                isSwish123: true,
                isMobileNumber: false,
                is90Number: true,
                phoneNumber: null);
            return true;
        }

        if (digits.Length == 10 && digits.StartsWith("123"))
        {
            var is90 = digits.StartsWith("1239");
            result = new SwedishSwishNumber(
                digits,
                is90 ? Format90Number(digits[3..]) : FormatSwish123(digits),
                isSwish123: true,
                isMobileNumber: false,
                is90Number: is90,
                phoneNumber: null);
            return true;
        }

        if (Contact.PhoneNumber.TryParse(cleaned, out var phone) && phone is { IsSwedish: true, IsMobile: true })
        {
            result = new SwedishSwishNumber(
                phone.Digits,
                phone.Formatted,
                isSwish123: false,
                isMobileNumber: true,
                is90Number: false,
                phoneNumber: phone);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses a Swish number. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static SwedishSwishNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swish number.", nameof(input));
        return result!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a valid Swish number.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display form, e.g. <c>123-665 28 95</c> or <c>070-123 45 67</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized form, e.g. <c>1236652895</c> or <c>0046701234567</c>.
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

    /// <summary>Returns the normalized digit string, e.g. <c>1236652895</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the display form, e.g. <c>123-665 28 95</c>.</summary>
    public override string ToString() => Formatted;

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)123[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}(?!\d)|(?<!\d)90[0-9][\s\-]?\d{2}[\s\-]?\d{2}(?!\d)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Swish 123 numbers and 90-numbers.
    /// Does not scan for mobile phone numbers (use <see cref="Contact.PhoneNumber.FindCandidatesInText"/> for those).
    /// Results are heuristic-based candidates and may include false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishSwishNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SwedishSwishNumber>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var swish)) continue;
            var s = swish!;
            results.Add(new TextCandidate<SwedishSwishNumber>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(SwedishSwishNumber),
                TextCandidateCategory.Financial,
                s.ToNormalizedString(),
                s.ToString(),
                s.ToMaskedString(),
                TextMatchConfidence.Medium,
                s));
        }

        return results;
    }

    /// <summary>
    /// Returns a masked display string, e.g. <c>123-*** ** **</c> for 123-numbers
    /// or <c>9** ** **</c> for 90-numbers.
    /// </summary>
    public string ToMaskedString()
    {
        if (Is90Number)
        {
            var shortDigits = Value[3..];
            return $"{shortDigits[0]}** ** **";
        }

        if (IsSwish123)
            return "123-*** ** **";

        return PhoneNumber?.ToMaskedString() ?? new string('*', Value.Length);
    }

    private static string FormatSwish123(string digits)
    {
        if (digits.Length != 10) return digits;
        return $"{digits[..3]}-{digits[3..6]} {digits[6..8]} {digits[8..10]}";
    }

    private static string Format90Number(string shortDigits)
    {
        if (shortDigits.Length != 7) return shortDigits;
        return $"{shortDigits[..3]} {shortDigits[3..5]} {shortDigits[5..7]}";
    }

    public bool Equals(SwedishSwishNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is SwedishSwishNumber other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishSwishNumber? a, SwedishSwishNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishSwishNumber? a, SwedishSwishNumber? b) => !(a == b);
    public int CompareTo(SwedishSwishNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishSwishNumber left, SwedishSwishNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishSwishNumber left, SwedishSwishNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishSwishNumber left, SwedishSwishNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishSwishNumber left, SwedishSwishNumber right) => left.CompareTo(right) >= 0;
}
