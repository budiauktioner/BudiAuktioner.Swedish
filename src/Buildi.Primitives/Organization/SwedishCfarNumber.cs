using Buildi.Primitives;

namespace Buildi.Primitives.Organization;

/// <summary>
/// A CFAR number (<c>CFAR-nummer</c>) is Statistics Sweden's 8-digit identifier for an establishment or workplace in the Swedish business register. It is commonly used together with organization number and SNI data in company master data.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.cfarnrsok.scb.se/">SCB - CfarNr search</see></description></item>
/// <item><description><see href="https://www.scb.se/vara-tjanster/bestall-data-och-statistik/foretagsregistret/variabelbeskrivning/">SCB - Business Register variable descriptions</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishCfarNumber : IEquatable<SwedishCfarNumber>, IComparable<SwedishCfarNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("CFAR Number", "CFAR-nummer", "🏭", ["https://www.cfarnrsok.scb.se/", "https://www.scb.se/vara-tjanster/bestall-data-och-statistik/foretagsregistret/variabelbeskrivning/"]);

    private const int MaxInputLength = 20;

    public string Number { get; }

    private SwedishCfarNumber(string number)
    {
        Number = number;
    }

    public static bool TryParse(string? input, out SwedishCfarNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(InputSanitization.SanitizeInput(input!));
        if (digits.Length > MaxInputLength) return false;
        if (digits.Length != 8) return false;
        if (digits == "00000000") return false;

        result = new SwedishCfarNumber(digits);
        return true;
    }

    public static SwedishCfarNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid CFAR number.", nameof(input));

        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);
    /// <summary>
    /// Returns the CFAR number as 8 digits, for example <c>12345678</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) => TryParse(input, out var result) ? result!.Number : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the normalized CFAR number as 8 digits, for example <c>12345678</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var result)) return result!.Number;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;
    /// <summary>
    /// Returns the normalized CFAR number as 8 digits, for example <c>12345678</c>.
    /// </summary>
    public string ToNormalizedString() => Number;
    /// <summary>
    /// Returns the CFAR number as 8 digits, for example <c>12345678</c>.
    /// </summary>
    public override string ToString() => Number;

    public bool Equals(SwedishCfarNumber? other) => other is not null && Number == other.Number;
    public override bool Equals(object? obj) => obj is SwedishCfarNumber other && Equals(other);
    public override int GetHashCode() => Number.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishCfarNumber? a, SwedishCfarNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishCfarNumber? a, SwedishCfarNumber? b) => !(a == b);
    public int CompareTo(SwedishCfarNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(SwedishCfarNumber left, SwedishCfarNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishCfarNumber left, SwedishCfarNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishCfarNumber left, SwedishCfarNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishCfarNumber left, SwedishCfarNumber right) => left.CompareTo(right) >= 0;
}
