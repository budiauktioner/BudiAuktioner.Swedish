using Buildi.Primitives;

namespace Buildi.Primitives.Organization;

/// <summary>
/// An SNI code (<c>SNI-kod</c>) is Statistics Sweden's classification code for a company's or establishment's economic activity. SNI 2025 is the current version (effective December 2024, replacing SNI 2007), based on the EU standard NACE Rev. 2.1. The most detailed level uses 5 digits and is commonly displayed as <c>XX.XXX</c>.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.scb.se/en/documentation/classifications-and-standards/swedish-standard-industrial-classification-sni/">SCB - Swedish Standard Industrial Classification (SNI)</see></description></item>
/// <item><description><see href="https://snisok.scb.se/en">SCB - SNI search (SNI-Sök)</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishSniCode : IEquatable<SwedishSniCode>, IComparable<SwedishSniCode>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("SNI Code", "SNI-kod", "📊", ["https://www.scb.se/en/documentation/classifications-and-standards/swedish-standard-industrial-classification-sni/", "https://snisok.scb.se/en"]);

    private const int MaxInputLength = 20;

    public string Code { get; }
    public string Formatted { get; }
    public string DivisionCode => Code[..2];
    public string GroupCode => Code[..3];
    public string SubGroupCode => Code[..4];

    private SwedishSniCode(string code, string formatted)
    {
        Code = code;
        Formatted = formatted;
    }

    public static bool TryParse(string? input, out SwedishSniCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(input);
        if (digits.Length > MaxInputLength) return false;
        if (digits.Length != 5) return false;
        if (digits == "00000") return false;
        if (digits[..2] == "00") return false;

        result = new SwedishSniCode(digits, $"{digits[..2]}.{digits[2..]}");
        return true;
    }

    public static SwedishSniCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid SNI code.", nameof(input));

        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the SNI code in display format with a dot, for example <c>62.010</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var result) ? result!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the SNI code in dot-separated canonical form, for example <c>62.010</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var result)) return result!.Formatted;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;
    /// <summary>
    /// Returns the SNI code in dot-separated canonical form, for example <c>62.010</c>.
    /// </summary>
    public string ToNormalizedString() => Formatted;
    /// <summary>
    /// Returns the SNI code in display format with a dot, for example <c>62.010</c>.
    /// </summary>
    public override string ToString() => Formatted;

    public bool Equals(SwedishSniCode? other) => other is not null && Formatted == other.Formatted;
    public override bool Equals(object? obj) => obj is SwedishSniCode other && Equals(other);
    public override int GetHashCode() => Formatted.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishSniCode? a, SwedishSniCode? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishSniCode? a, SwedishSniCode? b) => !(a == b);
    public int CompareTo(SwedishSniCode? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishSniCode left, SwedishSniCode right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishSniCode left, SwedishSniCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishSniCode left, SwedishSniCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishSniCode left, SwedishSniCode right) => left.CompareTo(right) >= 0;
}
