using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A year-month value (<c>år och månad</c>) with month precision but no day,
/// used for inspection-validity dates, registration months, and any data point
/// reported with month precision where forcing a day would invent precision
/// that is not present in the source.
/// </summary>
/// <remarks>
/// <para>Accepts ISO 8601 (<c>2026-07</c>), full ISO date that is treated as month precision (<c>2026-07-01</c>),
/// slash-separated forms (<c>2026/07</c>, <c>07/2026</c>), Swedish month names (<c>juli 2026</c>, <c>jul 2026</c>),
/// and English month names (<c>July 2026</c>, <c>Jul 2026</c>).</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/iso-8601-date-and-time-format.html">ISO 8601</see> — date and time format</description></item>
/// </list>
/// </remarks>
public sealed class YearMonth : IEquatable<YearMonth>, IComparable<YearMonth>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Year-Month", "Årsmånad", "📅", ["https://www.iso.org/iso-8601-date-and-time-format.html"]);

    /// <summary>Minimum supported year.</summary>
    public const int MinYear = 1000;

    /// <summary>Maximum supported year.</summary>
    public const int MaxYear = 9999;

    private static readonly Regex IsoLikePattern = new(
        @"^\s*(?<year>\d{4})[-/](?<month>0?[1-9]|1[0-2])(?:[-/](?<day>0?[1-9]|[12]\d|3[01]))?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex MonthYearNumericPattern = new(
        @"^\s*(?<month>0?[1-9]|1[0-2])[-/](?<year>\d{4})\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex MonthNameYearPattern = new(
        @"^\s*(?<name>[A-Za-zÅÄÖåäö]+\.?)\s+(?<year>\d{4})\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex YearMonthNamePattern = new(
        @"^\s*(?<year>\d{4})\s+(?<name>[A-Za-zÅÄÖåäö]+\.?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Dictionary<string, int> MonthNameLookup = BuildMonthNameLookup();

    /// <summary>Calendar year, e.g. <c>2026</c>.</summary>
    public int Year { get; }

    /// <summary>Calendar month (1–12).</summary>
    public int Month { get; }

    private YearMonth(int year, int month)
    {
        Year = year;
        Month = month;
    }

    /// <summary>Creates a <see cref="YearMonth"/> from a year and month.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the year or month is out of range.</exception>
    public static YearMonth Create(int year, int month)
    {
        if (year < MinYear || year > MaxYear)
            throw new ArgumentOutOfRangeException(nameof(year), $"Year must be between {MinYear} and {MaxYear}.");
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        return new YearMonth(year, month);
    }

    /// <summary>Creates a <see cref="YearMonth"/> from a <see cref="DateOnly"/>, discarding the day.</summary>
    public static YearMonth FromDate(DateOnly date) => Create(date.Year, date.Month);

    /// <summary>Creates a <see cref="YearMonth"/> from a <see cref="DateTime"/>, discarding the day and time.</summary>
    public static YearMonth FromDate(DateTime date) => Create(date.Year, date.Month);

    public static bool TryParse(string? input, out YearMonth? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        var iso = IsoLikePattern.Match(trimmed);
        if (iso.Success)
        {
            if (iso.Groups["day"].Success && !IsValidDay(
                    iso.Groups["year"].Value,
                    iso.Groups["month"].Value,
                    iso.Groups["day"].Value))
                return false;
            return TryBuild(iso.Groups["year"].Value, iso.Groups["month"].Value, out result);
        }

        var monthYear = MonthYearNumericPattern.Match(trimmed);
        if (monthYear.Success)
            return TryBuild(monthYear.Groups["year"].Value, monthYear.Groups["month"].Value, out result);

        var nameYear = MonthNameYearPattern.Match(trimmed);
        if (nameYear.Success
            && TryParseMonthName(nameYear.Groups["name"].Value, out var nm))
            return TryBuild(nameYear.Groups["year"].Value, nm.ToString(CultureInfo.InvariantCulture), out result);

        var yearName = YearMonthNamePattern.Match(trimmed);
        if (yearName.Success
            && TryParseMonthName(yearName.Groups["name"].Value, out var ynm))
            return TryBuild(yearName.Groups["year"].Value, ynm.ToString(CultureInfo.InvariantCulture), out result);

        return false;
    }

    public static YearMonth Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid year-month.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the canonical ISO 8601 form, e.g. <c>2026-07</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical ISO 8601 form, e.g. <c>2026-07</c>.
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

    /// <summary>Returns the canonical ISO 8601 form, e.g. <c>2026-07</c>.</summary>
    public string ToNormalizedString() =>
        $"{Year.ToString("D4", CultureInfo.InvariantCulture)}-{Month.ToString("D2", CultureInfo.InvariantCulture)}";

    /// <summary>Returns the canonical ISO 8601 form, e.g. <c>2026-07</c>.</summary>
    public override string ToString() => ToNormalizedString();

    /// <summary>Returns the first day of the month as a <see cref="DateOnly"/>.</summary>
    public DateOnly ToFirstDayOfMonth() => new(Year, Month, 1);

    /// <summary>Returns the last day of the month as a <see cref="DateOnly"/>.</summary>
    public DateOnly ToLastDayOfMonth() => new(Year, Month, DateTime.DaysInMonth(Year, Month));

    private static bool TryBuild(string yearStr, string monthStr, out YearMonth? result)
    {
        result = null;
        if (!int.TryParse(yearStr, NumberStyles.None, CultureInfo.InvariantCulture, out var year)) return false;
        if (!int.TryParse(monthStr, NumberStyles.None, CultureInfo.InvariantCulture, out var month)) return false;
        if (year < MinYear || year > MaxYear) return false;
        if (month < 1 || month > 12) return false;
        result = new YearMonth(year, month);
        return true;
    }

    private static bool IsValidDay(string yearStr, string monthStr, string dayStr)
    {
        if (!int.TryParse(yearStr, NumberStyles.None, CultureInfo.InvariantCulture, out var year)) return false;
        if (!int.TryParse(monthStr, NumberStyles.None, CultureInfo.InvariantCulture, out var month)) return false;
        if (!int.TryParse(dayStr, NumberStyles.None, CultureInfo.InvariantCulture, out var day)) return false;
        if (year < MinYear || year > MaxYear) return false;
        if (month < 1 || month > 12) return false;
        return day <= DateTime.DaysInMonth(year, month);
    }

    private static bool TryParseMonthName(string name, out int month)
    {
        month = 0;
        var key = name.Trim().TrimEnd('.').ToLowerInvariant();
        return MonthNameLookup.TryGetValue(key, out month);
    }

    private static Dictionary<string, int> BuildMonthNameLookup()
    {
        var d = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        string[] svFull = ["januari", "februari", "mars", "april", "maj", "juni",
                            "juli", "augusti", "september", "oktober", "november", "december"];
        for (int i = 0; i < svFull.Length; i++) d[svFull[i]] = i + 1;

        string[] enFull = ["january", "february", "march", "april", "may", "june",
                            "july", "august", "september", "october", "november", "december"];
        for (int i = 0; i < enFull.Length; i++) d.TryAdd(enFull[i], i + 1);

        d["jan"] = 1; d["feb"] = 2; d["mar"] = 3; d["apr"] = 4;
        d["maj"] = 5; d["jun"] = 6; d["jul"] = 7; d["aug"] = 8;
        d["sep"] = 9; d["sept"] = 9; d["okt"] = 10; d["oct"] = 10;
        d["nov"] = 11; d["dec"] = 12; d["may"] = 5;

        return d;
    }

    public int CompareTo(YearMonth? other)
    {
        if (other is null) return 1;
        var y = Year.CompareTo(other.Year);
        return y != 0 ? y : Month.CompareTo(other.Month);
    }

    public bool Equals(YearMonth? other) => other is not null && Year == other.Year && Month == other.Month;
    public override bool Equals(object? obj) => obj is YearMonth other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Year, Month);

    public static bool operator ==(YearMonth? a, YearMonth? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(YearMonth? a, YearMonth? b) => !(a == b);
    public static bool operator <(YearMonth a, YearMonth b) => a.CompareTo(b) < 0;
    public static bool operator >(YearMonth a, YearMonth b) => a.CompareTo(b) > 0;
    public static bool operator <=(YearMonth a, YearMonth b) => a.CompareTo(b) <= 0;
    public static bool operator >=(YearMonth a, YearMonth b) => a.CompareTo(b) >= 0;
}
