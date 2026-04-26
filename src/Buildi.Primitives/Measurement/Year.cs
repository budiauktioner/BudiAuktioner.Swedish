using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A four-digit calendar year (<c>år</c>) used for manufacture year, model year, and similar
/// year-precision metadata where a full date would invent precision that is not present in the source.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/iso-8601-date-and-time-format.html">ISO 8601</see> — date and time format</description></item>
/// </list>
/// </remarks>
public sealed class Year : IEquatable<Year>, IComparable<Year>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Year", "År", "📅", ["https://www.iso.org/iso-8601-date-and-time-format.html"]);

    /// <summary>Minimum supported year.</summary>
    public const int MinYear = 1000;

    /// <summary>Maximum supported year.</summary>
    public const int MaxYear = 9999;

    private static readonly Regex YearPattern = new(
        @"^\s*(?<year>\d{4})\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Calendar year as an integer, e.g. <c>2024</c>.</summary>
    public int Value { get; }

    private Year(int value)
    {
        Value = value;
    }

    /// <summary>Creates a <see cref="Year"/> from an integer year, e.g. <c>2024</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> is outside <see cref="MinYear"/>–<see cref="MaxYear"/>.</exception>
    public static Year Create(int year)
    {
        if (year < MinYear || year > MaxYear)
            throw new ArgumentOutOfRangeException(nameof(year), $"Year must be between {MinYear} and {MaxYear}.");
        return new Year(year);
    }

    public static bool TryParse(string? input, out Year? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var match = YearPattern.Match(trimmed);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups["year"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var year))
            return false;
        if (year < MinYear || year > MaxYear) return false;

        result = new Year(year);
        return true;
    }

    public static Year Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid year.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the year as a 4-digit string, e.g. <c>2024</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical 4-digit year, e.g. <c>2024</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the year as a 4-digit string, e.g. <c>2024</c>.</summary>
    public string ToNormalizedString() => Value.ToString("D4", CultureInfo.InvariantCulture);

    /// <summary>Returns the year as a 4-digit string, e.g. <c>2024</c>.</summary>
    public override string ToString() => ToNormalizedString();

    public int CompareTo(Year? other) => other is null ? 1 : Value.CompareTo(other.Value);
    public bool Equals(Year? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Year other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Year? a, Year? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Year? a, Year? b) => !(a == b);
    public static bool operator <(Year a, Year b) => a.Value < b.Value;
    public static bool operator >(Year a, Year b) => a.Value > b.Value;
    public static bool operator <=(Year a, Year b) => a.Value <= b.Value;
    public static bool operator >=(Year a, Year b) => a.Value >= b.Value;
}
