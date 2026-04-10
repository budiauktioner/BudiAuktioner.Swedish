using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// A Harmonized System (HS) code used for classifying goods in international trade. The HS is
/// maintained by the World Customs Organization (WCO) and provides a universal 6-digit classification.
/// The EU extends this with the Combined Nomenclature (CN, 8 digits, Swedish: <c>KN-nummer</c>) and
/// TARIC (10 digits). This type accepts codes at all five hierarchy levels: chapter (2 digits),
/// heading (4), subheading (6), CN subheading (8), and TARIC code (10).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.wcoomd.org/en/topics/nomenclature/overview/what-is-the-harmonized-system.aspx">WCO — What is the Harmonized System?</see> — international standard maintained by the World Customs Organization</description></item>
/// <item><description><see href="https://taxation-customs.ec.europa.eu/customs/common-customs-tariff-cct/tariff-classification-goods/combined-nomenclature_en">EU — Combined Nomenclature</see> — the EU's 8-digit extension of the HS</description></item>
/// <item><description><see href="https://www.tullverket.se">Tullverket</see> — Swedish Customs, administers the Combined Nomenclature (KN) in Sweden</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Harmonized_System">Wikipedia — Harmonized System</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Combined_Nomenclature">Wikipedia — Combined Nomenclature</see></description></item>
/// </list>
/// </remarks>
public sealed class HsCode : IEquatable<HsCode>, IComparable<HsCode>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("HS Code", "HS-kod", "📋", ["https://www.wcoomd.org/en/topics/nomenclature/overview/what-is-the-harmonized-system.aspx", "https://taxation-customs.ec.europa.eu/customs/common-customs-tariff-cct/tariff-classification-goods/combined-nomenclature_en", "https://www.tullverket.se", "https://en.wikipedia.org/wiki/Harmonized_System", "https://en.wikipedia.org/wiki/Combined_Nomenclature"]);

    private const int MaxInputLength = 30;

    private static readonly int[] ValidLengths = [2, 4, 6, 8, 10];

    /// <summary>The code as a digit-only string, e.g. <c>847130</c>.</summary>
    public string Digits { get; }

    /// <summary>The display-formatted code with dots, e.g. <c>8471.30</c>.</summary>
    public string Formatted { get; }

    /// <summary>The chapter (first 2 digits), e.g. <c>84</c>.</summary>
    public string Chapter { get; }

    /// <summary>The hierarchy level of this code.</summary>
    public HsCodeLevel Level { get; }

    private HsCode(string digits, string formatted, string chapter, HsCodeLevel level)
    {
        Digits = digits;
        Formatted = formatted;
        Chapter = chapter;
        Level = level;
    }

    public static bool TryParse(string? input, out HsCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length > MaxInputLength) return false;
        if (!Array.Exists(ValidLengths, l => l == digits.Length)) return false;

        var chapter = int.Parse(digits[..2]);
        if (chapter is < 1 or > 97) return false;

        var level = digits.Length switch
        {
            2 => HsCodeLevel.Chapter,
            4 => HsCodeLevel.Heading,
            6 => HsCodeLevel.Subheading,
            8 => HsCodeLevel.CnSubheading,
            10 => HsCodeLevel.TaricCode,
            _ => HsCodeLevel.Unknown
        };

        var formatted = FormatDigits(digits);

        result = new HsCode(digits, formatted, digits[..2], level);
        return true;
    }

    public static HsCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid HS code.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the HS code in display format with dots, e.g. <c>8471.30</c> for a 6-digit subheading
    /// or <c>8471.30.00</c> for an 8-digit CN code.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Formatted : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the HS code in dot-separated canonical form, e.g. <c>8471.30</c>.
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

    /// <summary>Returns the HS code in dot-separated canonical form, e.g. <c>8471.30</c>.</summary>
    public string ToNormalizedString() => Formatted;

    /// <summary>Returns the HS code in display format with dots, e.g. <c>8471.30</c>.</summary>
    public override string ToString() => Formatted;

    /// <summary>
    /// Formats digit-only HS code into dot-separated display form.
    /// 2 → <c>84</c>, 4 → <c>84.71</c>, 6 → <c>8471.30</c>, 8 → <c>8471.30.00</c>, 10 → <c>8471.30.00.00</c>.
    /// </summary>
    private static string FormatDigits(string digits) => digits.Length switch
    {
        2 => digits,
        4 => $"{digits[..2]}.{digits[2..]}",
        6 => $"{digits[..4]}.{digits[4..]}",
        8 => $"{digits[..4]}.{digits[4..6]}.{digits[6..]}",
        10 => $"{digits[..4]}.{digits[4..6]}.{digits[6..8]}.{digits[8..]}",
        _ => digits
    };

    public bool Equals(HsCode? other) => other is not null && Formatted == other.Formatted;
    public override bool Equals(object? obj) => obj is HsCode other && Equals(other);
    public override int GetHashCode() => Formatted.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(HsCode? a, HsCode? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(HsCode? a, HsCode? b) => !(a == b);
    public int CompareTo(HsCode? other) => other is null ? 1 : string.Compare(Digits, other.Digits, StringComparison.Ordinal);
    public static bool operator <(HsCode left, HsCode right) => left.CompareTo(right) < 0;
    public static bool operator >(HsCode left, HsCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(HsCode left, HsCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(HsCode left, HsCode right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// The hierarchy level of an <see cref="HsCode"/>.
/// </summary>
public enum HsCodeLevel
{
    /// <summary>Unknown or unrecognized level.</summary>
    Unknown = 0,

    /// <summary>2-digit chapter (e.g. <c>84</c>).</summary>
    Chapter = 1,

    /// <summary>4-digit heading (e.g. <c>84.71</c>).</summary>
    Heading = 2,

    /// <summary>6-digit HS subheading (e.g. <c>8471.30</c>), the universal international level.</summary>
    Subheading = 3,

    /// <summary>8-digit EU Combined Nomenclature code (e.g. <c>8471.30.00</c>).</summary>
    CnSubheading = 4,

    /// <summary>10-digit EU TARIC code (e.g. <c>8471.30.00.00</c>).</summary>
    TaricCode = 5
}
