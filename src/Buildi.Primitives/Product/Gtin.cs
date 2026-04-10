using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// A Global Trade Item Number (GTIN) in any of the four standard lengths: GTIN-8, GTIN-12, GTIN-13,
/// or GTIN-14. GTINs are assigned by GS1 to uniquely identify trade items (products, services, or
/// logistics units). All formats share the same modulo-10 check digit algorithm. Any GTIN can be
/// normalized to GTIN-14 by zero-padding on the left.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.gs1.org/standards/id-keys/gtin">GS1 — GTIN</see></description></item>
/// <item><description><see href="https://www.gs1.org/services/check-digit-calculator">GS1 — Check digit calculator</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Global_Trade_Item_Number">Wikipedia — Global Trade Item Number</see></description></item>
/// </list>
/// </remarks>
public sealed class Gtin : IEquatable<Gtin>, IComparable<Gtin>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("GTIN", "GTIN", "📦", ["https://www.gs1.org/standards/id-keys/gtin", "https://www.gs1.org/services/check-digit-calculator", "https://en.wikipedia.org/wiki/Global_Trade_Item_Number"]);

    /// <summary>The GTIN as a digit-only string, e.g. <c>5901234123457</c>.</summary>
    public string Digits { get; }

    /// <summary>The number of digits: 8, 12, 13, or 14.</summary>
    public int Length { get; }

    /// <summary>The GS1 modulo-10 check digit (last digit).</summary>
    public int CheckDigit { get; }

    private Gtin(string digits)
    {
        Digits = digits;
        Length = digits.Length;
        CheckDigit = digits[^1] - '0';
    }

    /// <summary>Returns the GTIN zero-padded to 14 digits, e.g. <c>05901234123457</c>.</summary>
    public string ToGtin14Digits() => Digits.PadLeft(14, '0');

    public static bool TryParse(string? input, out Gtin? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = GtinCheckDigit.ExtractDigits(InputSanitization.SanitizeInput(input!));
        if (digits is null) return false;
        if (!GtinCheckDigit.ValidLengths.Contains(digits.Length)) return false;
        if (!GtinCheckDigit.Validate(digits)) return false;

        result = new Gtin(digits);
        return true;
    }

    public static Gtin Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid GTIN.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the GTIN as digits only, e.g. <c>5901234123457</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Digits : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the GTIN as digits only, e.g. <c>5901234123457</c>.
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

    /// <summary>Returns the GTIN as digits only, e.g. <c>5901234123457</c>.</summary>
    public string ToNormalizedString() => Digits;

    /// <summary>Returns the GTIN as digits only, e.g. <c>5901234123457</c>.</summary>
    public override string ToString() => Digits;

    public bool Equals(Gtin? other) => other is not null && Digits == other.Digits;
    public override bool Equals(object? obj) => obj is Gtin other && Equals(other);
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(Gtin? a, Gtin? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Gtin? a, Gtin? b) => !(a == b);
    public int CompareTo(Gtin? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Gtin left, Gtin right) => left.CompareTo(right) < 0;
    public static bool operator >(Gtin left, Gtin right) => left.CompareTo(right) > 0;
    public static bool operator <=(Gtin left, Gtin right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Gtin left, Gtin right) => left.CompareTo(right) >= 0;
}
