using Buildi.Primitives.Person;

namespace Buildi.Primitives.Tests.Person;

public class PersonAgeTests
{
    [Theory]
    [InlineData("25")]
    [InlineData("0")]
    [InlineData("100")]
    [InlineData("200")]
    [InlineData("25 år")]
    [InlineData("25 years")]
    [InlineData("0.5 år")]
    [InlineData("0.5 years")]
    [InlineData("1.5 år")]
    [InlineData("8 månader")]
    [InlineData("8 months")]
    [InlineData("1 månad")]
    [InlineData("1 month")]
    [InlineData("300 dagar")]
    [InlineData("300 days")]
    [InlineData("1 dag")]
    [InlineData("1 day")]
    [InlineData("  25  ")]
    [InlineData("25 ÅR")]
    [InlineData("25 Years")]
    [InlineData("8 Månader")]
    [InlineData("8 MONTHS")]
    [InlineData("300 DAGAR")]
    [InlineData("300 Days")]
    [InlineData("0,5 år")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(PersonAge.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("-5 years")]
    [InlineData("201")]
    [InlineData("25 veckor")]
    [InlineData("25 weeks")]
    [InlineData("abc years")]
    [InlineData("years")]
    [InlineData("månader")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(PersonAge.IsValid(input));
    }

    [Theory]
    [InlineData("25", 25, 300, 9125)]
    [InlineData("0", 0, 0, 0)]
    [InlineData("100", 100, 1200, 36500)]
    [InlineData("  25  ", 25, 300, 9125)]
    public void TryParse_Years_ReturnsExpectedProperties(string input, int expectedYears, int expectedMonths, int expectedDays)
    {
        Assert.True(PersonAge.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedYears, result!.Years);
        Assert.Equal(expectedMonths, result.TotalMonths);
        Assert.Equal(expectedDays, result.TotalDays);
    }

    [Theory]
    [InlineData("25 år", 25, 300, 9125)]
    [InlineData("25 years", 25, 300, 9125)]
    [InlineData("0 år", 0, 0, 0)]
    [InlineData("0 years", 0, 0, 0)]
    [InlineData("25 ÅR", 25, 300, 9125)]
    [InlineData("25 year", 25, 300, 9125)]
    public void TryParse_YearsWithUnit_ReturnsExpectedProperties(string input, int expectedYears, int expectedMonths, int expectedDays)
    {
        Assert.True(PersonAge.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedYears, result!.Years);
        Assert.Equal(expectedMonths, result.TotalMonths);
        Assert.Equal(expectedDays, result.TotalDays);
    }

    [Theory]
    [InlineData("0.5 år", 0, 6, 182)]
    [InlineData("0.5 years", 0, 6, 182)]
    [InlineData("1.5 år", 1, 18, 547)]
    [InlineData("0,5 år", 0, 6, 182)]
    public void TryParse_FractionalYears_ReturnsExpectedProperties(string input, int expectedYears, int expectedMonths, int expectedDays)
    {
        Assert.True(PersonAge.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedYears, result!.Years);
        Assert.Equal(expectedMonths, result.TotalMonths);
        Assert.Equal(expectedDays, result.TotalDays);
    }

    [Theory]
    [InlineData("8 månader", 0, 8, 240)]
    [InlineData("8 months", 0, 8, 240)]
    [InlineData("1 månad", 0, 1, 30)]
    [InlineData("1 month", 0, 1, 30)]
    [InlineData("12 månader", 1, 12, 360)]
    [InlineData("24 months", 2, 24, 720)]
    [InlineData("8 Månader", 0, 8, 240)]
    [InlineData("8 MONTHS", 0, 8, 240)]
    public void TryParse_Months_ReturnsExpectedProperties(string input, int expectedYears, int expectedMonths, int expectedDays)
    {
        Assert.True(PersonAge.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedYears, result!.Years);
        Assert.Equal(expectedMonths, result.TotalMonths);
        Assert.Equal(expectedDays, result.TotalDays);
    }

    [Theory]
    [InlineData("300 dagar", 0, 10, 300)]
    [InlineData("300 days", 0, 10, 300)]
    [InlineData("1 dag", 0, 0, 1)]
    [InlineData("1 day", 0, 0, 1)]
    [InlineData("365 dagar", 1, 12, 365)]
    [InlineData("730 days", 2, 24, 730)]
    [InlineData("300 DAGAR", 0, 10, 300)]
    [InlineData("300 Days", 0, 10, 300)]
    public void TryParse_Days_ReturnsExpectedProperties(string input, int expectedYears, int expectedMonths, int expectedDays)
    {
        Assert.True(PersonAge.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedYears, result!.Years);
        Assert.Equal(expectedMonths, result.TotalMonths);
        Assert.Equal(expectedDays, result.TotalDays);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("201")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(PersonAge.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => PersonAge.Parse(input));
    }

    [Fact]
    public void FromYears_ReturnsExpectedProperties()
    {
        var age = PersonAge.FromYears(25);
        Assert.Equal(25, age.Years);
        Assert.Equal(300, age.TotalMonths);
        Assert.Equal(9125, age.TotalDays);
    }

    [Fact]
    public void FromYears_Zero_ReturnsZero()
    {
        var age = PersonAge.FromYears(0);
        Assert.Equal(0, age.Years);
        Assert.Equal(0, age.TotalMonths);
        Assert.Equal(0, age.TotalDays);
    }

    [Fact]
    public void FromYears_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonAge.FromYears(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonAge.FromYears(201));
    }

    [Fact]
    public void FromMonths_ReturnsExpectedProperties()
    {
        var age = PersonAge.FromMonths(8);
        Assert.Equal(0, age.Years);
        Assert.Equal(8, age.TotalMonths);
        Assert.Equal(240, age.TotalDays);
    }

    [Fact]
    public void FromMonths_TwelveMonths_IsOneYear()
    {
        var age = PersonAge.FromMonths(12);
        Assert.Equal(1, age.Years);
        Assert.Equal(12, age.TotalMonths);
    }

    [Fact]
    public void FromMonths_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonAge.FromMonths(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonAge.FromMonths(200 * 12 + 1));
    }

    [Fact]
    public void FromDays_ReturnsExpectedProperties()
    {
        var age = PersonAge.FromDays(300);
        Assert.Equal(0, age.Years);
        Assert.Equal(10, age.TotalMonths);
        Assert.Equal(300, age.TotalDays);
    }

    [Fact]
    public void FromDays_365_IsOneYear()
    {
        var age = PersonAge.FromDays(365);
        Assert.Equal(1, age.Years);
    }

    [Fact]
    public void FromDays_364_IsZeroYears()
    {
        var age = PersonAge.FromDays(364);
        Assert.Equal(0, age.Years);
    }

    [Fact]
    public void FromDays_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonAge.FromDays(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonAge.FromDays(200 * 365 + 1));
    }

    [Fact]
    public void FromBirthDate_ExactYears()
    {
        var birthDate = new DateOnly(2000, 1, 15);
        var referenceDate = new DateOnly(2025, 1, 15);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(25, age.Years);
    }

    [Fact]
    public void FromBirthDate_BeforeBirthday_OneLessYear()
    {
        var birthDate = new DateOnly(2000, 6, 15);
        var referenceDate = new DateOnly(2025, 6, 14);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(24, age.Years);
    }

    [Fact]
    public void FromBirthDate_OnBirthday_ExactYear()
    {
        var birthDate = new DateOnly(2000, 6, 15);
        var referenceDate = new DateOnly(2025, 6, 15);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(25, age.Years);
    }

    [Fact]
    public void FromBirthDate_Infant_ZeroYears()
    {
        var birthDate = new DateOnly(2025, 1, 1);
        var referenceDate = new DateOnly(2025, 9, 1);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(0, age.Years);
        Assert.Equal(8, age.TotalMonths);
        Assert.Equal(243, age.TotalDays);
    }

    [Fact]
    public void FromBirthDate_SameDay_ZeroAge()
    {
        var date = new DateOnly(2025, 4, 8);
        var age = PersonAge.FromBirthDate(date, date);
        Assert.Equal(0, age.Years);
        Assert.Equal(0, age.TotalMonths);
        Assert.Equal(0, age.TotalDays);
    }

    [Fact]
    public void FromBirthDate_FutureDate_Throws()
    {
        var birthDate = new DateOnly(2030, 1, 1);
        var referenceDate = new DateOnly(2025, 1, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonAge.FromBirthDate(birthDate, referenceDate));
    }

    [Fact]
    public void FromBirthDate_ExactTotalDays()
    {
        var birthDate = new DateOnly(2025, 1, 1);
        var referenceDate = new DateOnly(2025, 1, 11);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(10, age.TotalDays);
    }

    [Fact]
    public void FromBirthDate_LeapYearBirthday()
    {
        var birthDate = new DateOnly(2000, 2, 29);
        var referenceDate = new DateOnly(2025, 2, 28);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(24, age.Years);
    }

    [Theory]
    [InlineData(18, true)]
    [InlineData(17, false)]
    [InlineData(0, false)]
    [InlineData(65, true)]
    [InlineData(100, true)]
    public void IsOfSwedishLegalAge_ReturnsExpected(int years, bool expected)
    {
        var age = PersonAge.FromYears(years);
        Assert.Equal(expected, age.IsOfSwedishLegalAge);
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(14, false)]
    [InlineData(0, false)]
    [InlineData(18, true)]
    public void IsSwedishCriminallyResponsible_ReturnsExpected(int years, bool expected)
    {
        var age = PersonAge.FromYears(years);
        Assert.Equal(expected, age.IsSwedishCriminallyResponsible);
    }

    [Theory]
    [InlineData(65, true)]
    [InlineData(64, false)]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void IsOfSwedishRetirementAge_ReturnsExpected(int years, bool expected)
    {
        var age = PersonAge.FromYears(years);
        Assert.Equal(expected, age.IsOfSwedishRetirementAge);
    }

    [Theory]
    [InlineData("25", "25 år")]
    [InlineData("25 years", "25 år")]
    [InlineData("25 år", "25 år")]
    [InlineData("0", "0 månader")]
    [InlineData("8 månader", "8 månader")]
    [InlineData("300 dagar", "10 månader")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("abc", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonAge.Format(input));
    }

    [Fact]
    public void Format_FallbackToTrimmedInput_WhenInvalid()
    {
        Assert.Equal("abc", PersonAge.Format("  abc  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PersonAge.Format("abc"));
    }

    [Theory]
    [InlineData("25", "25")]
    [InlineData("25 år", "25")]
    [InlineData("25 years", "25")]
    [InlineData("8 månader", "0")]
    [InlineData("300 dagar", "0")]
    [InlineData("365 days", "1")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("abc", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PersonAge.Normalize(input));
    }

    [Fact]
    public void Normalize_FallbackToTrimmedInput_WhenInvalid()
    {
        Assert.Equal("abc", PersonAge.Normalize("  abc  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PersonAge.Normalize(null, fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(PersonAge.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("25", true)]
    [InlineData("0", true)]
    [InlineData("25 år", false)]
    [InlineData("25 years", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, PersonAge.IsNormalized(input));
    }

    [Theory]
    [InlineData(25, "25 år")]
    [InlineData(0, "0 månader")]
    [InlineData(1, "1 år")]
    [InlineData(100, "100 år")]
    public void ToString_ReturnsFormattedValue(int years, string expected)
    {
        var age = PersonAge.FromYears(years);
        Assert.Equal(expected, age.ToString());
    }

    [Theory]
    [InlineData(25, "25")]
    [InlineData(0, "0")]
    [InlineData(100, "100")]
    public void ToNormalizedString_ReturnsYearCount(int years, string expected)
    {
        var age = PersonAge.FromYears(years);
        Assert.Equal(expected, age.ToNormalizedString());
    }

    [Fact]
    public void ToString_InfantMonths()
    {
        var age = PersonAge.FromMonths(8);
        Assert.Equal("8 månader", age.ToString());
    }

    [Fact]
    public void Equality_SameAge()
    {
        var a = PersonAge.FromYears(25);
        var b = PersonAge.FromYears(25);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentAge()
    {
        var a = PersonAge.FromYears(25);
        var b = PersonAge.FromYears(30);
        Assert.False(a == b);
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equality_NullHandling()
    {
        var age = PersonAge.FromYears(25);
        Assert.False(age.Equals(null));
        Assert.False(age == null);
        Assert.True(age != null);
        Assert.True((PersonAge?)null == null);
    }

    [Fact]
    public void CompareTo_Ordering()
    {
        var younger = PersonAge.FromYears(20);
        var older = PersonAge.FromYears(30);
        Assert.True(younger < older);
        Assert.True(older > younger);
        Assert.True(younger <= older);
        Assert.True(older >= younger);
        Assert.True(younger <= PersonAge.FromYears(20));
        Assert.True(younger >= PersonAge.FromYears(20));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var age = PersonAge.FromYears(25);
        Assert.True(age.CompareTo(null) > 0);
    }

    [Fact]
    public void FromBirthDate_TotalMonths_ExactCalculation()
    {
        var birthDate = new DateOnly(2024, 6, 15);
        var referenceDate = new DateOnly(2025, 2, 15);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(8, age.TotalMonths);
    }

    [Fact]
    public void FromBirthDate_TotalMonths_PartialMonth()
    {
        var birthDate = new DateOnly(2024, 6, 15);
        var referenceDate = new DateOnly(2025, 2, 10);
        var age = PersonAge.FromBirthDate(birthDate, referenceDate);
        Assert.Equal(7, age.TotalMonths);
    }
}
