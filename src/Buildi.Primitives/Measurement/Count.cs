using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A non-negative integer count of items (<c>antal</c>), used for fields like number of keys
/// (<c>antal nycklar</c>), number of seats (<c>antal säten</c>), number of doors, owners, units, etc.
/// </summary>
/// <remarks>
/// <para>Accepts a bare integer (<c>5</c>), an integer with the Swedish piece-count abbreviation
/// (<c>5 st</c>, <c>5st</c>, <c>5 st.</c>, <c>5 stycken</c>), Norwegian/Danish piece abbreviation
/// (<c>5 stk</c>), or English equivalents (<c>5 pcs</c>, <c>5 pc</c>, <c>5 pieces</c>, <c>5 piece</c>,
/// <c>5 ea</c>). Thousand separators are tolerated when they form whole groups of three digits
/// (e.g. <c>1 345</c>, <c>1.345</c>, <c>1,345</c>); decimal-looking inputs (<c>1,5</c>, <c>1.5</c>)
/// are rejected because counts are integers.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.tnc.se/">TNC — Terminologicentrum</see> — Swedish technical terminology, including <c>st</c> as the standard abbreviation for <c>stycken</c></description></item>
/// </list>
/// </remarks>
public sealed class Count : IEquatable<Count>, IComparable<Count>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Count",
        "Antal",
        "🔢",
        ["https://www.tnc.se/"]);

    /// <summary>Minimum supported count.</summary>
    public const int MinValue = 0;

    /// <summary>Maximum supported count.</summary>
    public const int MaxValue = 1_000_000_000;

    private const int MaxInputLength = 50;

    private static readonly Regex InputPattern = new(
        @"^(?<number>[0-9](?:[0-9 .,\u00A0]*[0-9])?)\s*(?<suffix>st\.?|stk\.?|stycken|stycke|pcs\.?|pc\.?|pieces|piece|ea\.?|x)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>The count as an integer, e.g. <c>5</c>.</summary>
    public int Value { get; }

    private Count(int value)
    {
        Value = value;
    }

    /// <summary>Creates a <see cref="Count"/> from an integer, e.g. <c>5</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is outside <see cref="MinValue"/>–<see cref="MaxValue"/>.</exception>
    public static Count Create(int value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Count must be between {MinValue} and {MaxValue}.");
        return new Count(value);
    }

    public static bool TryParse(string? input, out Count? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        if (trimmed.Length == 0 || trimmed.Length > MaxInputLength) return false;

        var match = InputPattern.Match(trimmed);
        if (!match.Success) return false;

        if (!TryParseInteger(match.Groups["number"].Value, out var value)) return false;
        if (value < MinValue || value > MaxValue) return false;

        result = new Count(value);
        return true;
    }

    public static Count Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid count.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display string with the Swedish piece abbreviation, e.g. <c>5 st</c> or <c>1 345 st</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns the canonical digit-only form, e.g. <c>5</c> or <c>1345</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the digit-only canonical form, e.g. <c>5</c> or <c>1345</c>.</summary>
    public string ToNormalizedString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Returns the count with the Swedish piece abbreviation and grouped thousands, e.g. <c>5 st</c> or <c>1 345 st</c>.</summary>
    public override string ToString() => $"{FormatGrouped(Value)} st";

    /// <summary>Returns a grouped, suffix-less display form, e.g. <c>5</c> or <c>1 345</c>.</summary>
    public string ToNaturalString() => FormatGrouped(Value);

    private static string FormatGrouped(int value)
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        nfi.NumberDecimalDigits = 0;
        return value.ToString("N0", nfi);
    }

    private static bool TryParseInteger(string raw, out int value)
    {
        value = 0;
        var s = raw.Replace(" ", "").Replace("\u00A0", "");
        if (s.Length == 0) return false;

        var lastComma = s.LastIndexOf(',');
        var lastPeriod = s.LastIndexOf('.');
        var lastSep = Math.Max(lastComma, lastPeriod);

        if (lastSep >= 0)
        {
            var afterSep = s[(lastSep + 1)..];
            if (afterSep.Length is > 0 and < 3 && afterSep.All(char.IsDigit))
                return false;
        }

        var cleaned = s.Replace(".", "").Replace(",", "");
        if (cleaned.Length == 0 || !cleaned.All(char.IsDigit)) return false;

        return int.TryParse(cleaned, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    public int CompareTo(Count? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public bool Equals(Count? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Count other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Count? a, Count? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Count? a, Count? b) => !(a == b);
    public static bool operator <(Count a, Count b) => a.Value < b.Value;
    public static bool operator >(Count a, Count b) => a.Value > b.Value;
    public static bool operator <=(Count a, Count b) => a.Value <= b.Value;
    public static bool operator >=(Count a, Count b) => a.Value >= b.Value;
}
