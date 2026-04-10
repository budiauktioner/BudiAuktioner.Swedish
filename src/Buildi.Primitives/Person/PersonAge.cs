using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Person;

/// <summary>
/// A person's age (<c>ålder</c>), expressed in completed years. Can be parsed from a numeric string
/// with optional unit labels in Swedish or English (<c>25</c>, <c>25 år</c>, <c>8 månader</c>,
/// <c>300 dagar</c>), or constructed from a birth date via <see cref="FromBirthDate(DateOnly)"/>.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/foraldrabalk-1949381_sfs-1949-381/">Föräldrabalken (SFS 1949:381)</see> — Swedish legal age of majority (18)</description></item>
/// <item><description><see href="https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/brottsbalk-1962700_sfs-1962-700/">Brottsbalken (SFS 1962:700) 1 kap. 6 §</see> — Swedish age of criminal responsibility (15)</description></item>
/// </list>
/// </remarks>
public sealed class PersonAge : IEquatable<PersonAge>, IComparable<PersonAge>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Age", "Ålder", "🎂", ["https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/foraldrabalk-1949381_sfs-1949-381/", "https://www.riksdagen.se/sv/dokument-och-lagar/dokument/svensk-forfattningssamling/brottsbalk-1962700_sfs-1962-700/"]);

    private const int MaxYears = 200;
    private const int MaxTotalDays = MaxYears * 365;
    private const int DaysPerYear = 365;
    private const int DaysPerMonth = 30;

    // "25", "25 år", "25 years", "0.5 år", "0.5 years"
    private static readonly Regex YearsPattern = new(
        @"^\s*(?<value>\d+(?:[.,]\d+)?)\s*(?:år|years?)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "8 månader", "8 months", "1 månad", "1 month"
    private static readonly Regex MonthsPattern = new(
        @"^\s*(?<value>\d+)\s+(?:månader?|månad|months?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "300 dagar", "300 days", "1 dag", "1 day"
    private static readonly Regex DaysPattern = new(
        @"^\s*(?<value>\d+)\s+(?:dagar?|dag|days?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Completed years, e.g. <c>25</c>. Returns <c>0</c> for ages under one year.</summary>
    public int Years { get; }

    /// <summary>Completed months, e.g. <c>8</c> for a 240-day-old infant.</summary>
    public int TotalMonths { get; }

    /// <summary>Total number of days, e.g. <c>9125</c> for a 25-year-old.</summary>
    public int TotalDays { get; }

    /// <summary>
    /// <see langword="true"/> when the person is 18 or older — the Swedish age of legal majority
    /// (<c>myndig</c>) per Föräldrabalken (SFS 1949:381).
    /// </summary>
    public bool IsOfSwedishLegalAge => Years >= 18;

    /// <summary>
    /// <see langword="true"/> when the person is 15 or older — the Swedish age of criminal responsibility
    /// (<c>straffmyndig</c>) per Brottsbalken (SFS 1962:700) 1 kap. 6 §.
    /// </summary>
    public bool IsSwedishCriminallyResponsible => Years >= 15;

    /// <summary>
    /// <see langword="true"/> when the person is 65 or older — the earliest age for Swedish public pension
    /// withdrawal (<c>pensionsålder</c>).
    /// </summary>
    public bool IsOfSwedishRetirementAge => Years >= 65;

    private PersonAge(int years, int totalMonths, int totalDays)
    {
        Years = years;
        TotalMonths = totalMonths;
        TotalDays = totalDays;
    }

    public static bool TryParse(string? input, out PersonAge? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var sanitized = input!.Trim();
        if (sanitized.Length > 50) return false;

        var match = DaysPattern.Match(sanitized);
        if (match.Success)
        {
            if (!int.TryParse(match.Groups["value"].Value, CultureInfo.InvariantCulture, out var days))
                return false;
            return TryCreateFromDays(days, out result);
        }

        match = MonthsPattern.Match(sanitized);
        if (match.Success)
        {
            if (!int.TryParse(match.Groups["value"].Value, CultureInfo.InvariantCulture, out var months))
                return false;
            return TryCreateFromMonths(months, out result);
        }

        match = YearsPattern.Match(sanitized);
        if (match.Success)
        {
            var valueStr = match.Groups["value"].Value.Replace(',', '.');
            if (!double.TryParse(valueStr, CultureInfo.InvariantCulture, out var years))
                return false;
            if (years < 0) return false;
            var intYears = (int)years;
            if (years == intYears)
                return TryCreateFromYears(intYears, out result);
            var totalDays = (int)(years * DaysPerYear);
            return TryCreateFromDays(totalDays, out result);
        }

        return false;
    }

    public static PersonAge Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid person age.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Creates a <see cref="PersonAge"/> from a number of completed years.
    /// </summary>
    public static PersonAge FromYears(int years)
    {
        if (years < 0 || years > MaxYears)
            throw new ArgumentOutOfRangeException(nameof(years), $"Years must be between 0 and {MaxYears}.");
        return new PersonAge(years, years * 12, years * DaysPerYear);
    }

    /// <summary>
    /// Creates a <see cref="PersonAge"/> from a number of months.
    /// </summary>
    public static PersonAge FromMonths(int months)
    {
        if (months < 0 || months > MaxYears * 12)
            throw new ArgumentOutOfRangeException(nameof(months), $"Months must be between 0 and {MaxYears * 12}.");
        return new PersonAge(months / 12, months, months * DaysPerMonth);
    }

    /// <summary>
    /// Creates a <see cref="PersonAge"/> from a number of days.
    /// </summary>
    public static PersonAge FromDays(int days)
    {
        if (days < 0 || days > MaxTotalDays)
            throw new ArgumentOutOfRangeException(nameof(days), $"Days must be between 0 and {MaxTotalDays}.");
        return new PersonAge(days / DaysPerYear, days / DaysPerMonth, days);
    }

    /// <summary>
    /// Creates a <see cref="PersonAge"/> from a birth date, using <see cref="TimeProvider.System"/>
    /// to determine today's date.
    /// </summary>
    public static PersonAge FromBirthDate(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(TimeProvider.System.GetLocalNow().DateTime);
        return FromBirthDate(birthDate, today);
    }

    /// <summary>
    /// Creates a <see cref="PersonAge"/> from a birth date and an explicit reference date.
    /// </summary>
    public static PersonAge FromBirthDate(DateOnly birthDate, DateOnly referenceDate)
    {
        if (birthDate > referenceDate)
            throw new ArgumentOutOfRangeException(nameof(birthDate), "Birth date cannot be in the future relative to the reference date.");

        var totalDays = referenceDate.DayNumber - birthDate.DayNumber;

        var years = referenceDate.Year - birthDate.Year;
        if (referenceDate.Month < birthDate.Month ||
            (referenceDate.Month == birthDate.Month && referenceDate.Day < birthDate.Day))
            years--;

        var monthDate = birthDate.AddYears(years);
        var months = 0;
        while (monthDate.AddMonths(months + 1) <= referenceDate)
            months++;
        var totalMonths = years * 12 + months;

        return new PersonAge(years, totalMonths, totalDays);
    }

    /// <summary>
    /// Returns a display-friendly Swedish string, e.g. <c>25 år</c> or <c>8 månader</c> for ages under one year.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns the age as a year count string, e.g. <c>25</c>.
    /// Returns <see langword="null"/> when the input is invalid.
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

    /// <summary>Returns the age as a year count string, e.g. <c>25</c>.</summary>
    public string ToNormalizedString() => Years.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a display-friendly Swedish string, e.g. <c>25 år</c> or <c>8 månader</c> for ages under one year.
    /// </summary>
    public override string ToString() =>
        Years >= 1
            ? $"{Years} år"
            : $"{TotalMonths} månader";

    public bool Equals(PersonAge? other) => other is not null && TotalDays == other.TotalDays;
    public override bool Equals(object? obj) => obj is PersonAge other && Equals(other);
    public override int GetHashCode() => TotalDays.GetHashCode();
    public static bool operator ==(PersonAge? a, PersonAge? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(PersonAge? a, PersonAge? b) => !(a == b);
    public int CompareTo(PersonAge? other) => other is null ? 1 : TotalDays.CompareTo(other.TotalDays);
    public static bool operator <(PersonAge left, PersonAge right) => left.CompareTo(right) < 0;
    public static bool operator >(PersonAge left, PersonAge right) => left.CompareTo(right) > 0;
    public static bool operator <=(PersonAge left, PersonAge right) => left.CompareTo(right) <= 0;
    public static bool operator >=(PersonAge left, PersonAge right) => left.CompareTo(right) >= 0;

    private static bool TryCreateFromYears(int years, out PersonAge? result)
    {
        result = null;
        if (years < 0 || years > MaxYears) return false;
        result = new PersonAge(years, years * 12, years * DaysPerYear);
        return true;
    }

    private static bool TryCreateFromDays(int days, out PersonAge? result)
    {
        result = null;
        if (days < 0 || days > MaxTotalDays) return false;
        result = new PersonAge(days / DaysPerYear, days / DaysPerMonth, days);
        return true;
    }

    private static bool TryCreateFromMonths(int months, out PersonAge? result)
    {
        result = null;
        if (months < 0 || months > MaxYears * 12) return false;
        var totalDays = months * DaysPerMonth;
        if (totalDays > MaxTotalDays) return false;
        result = new PersonAge(months / 12, months, totalDays);
        return true;
    }
}
