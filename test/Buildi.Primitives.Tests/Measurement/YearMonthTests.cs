using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class YearMonthTests
{
    [Theory]
    [InlineData("2026-07")]
    [InlineData("2026-7")]
    [InlineData("2026/07")]
    [InlineData("07/2026")]
    [InlineData("7-2026")]
    [InlineData("2026-07-01")]
    [InlineData("2026-07-31")]
    [InlineData("2024-02-29")]
    [InlineData("juli 2026")]
    [InlineData("Juli 2026")]
    [InlineData("July 2026")]
    [InlineData("Jul 2026")]
    [InlineData("2026 Juli")]
    [InlineData(" 2026-07 ")]
    [InlineData("Jan 2026")]
    [InlineData("dec 2026")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(YearMonth.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("2026")]
    [InlineData("2026-13")]
    [InlineData("0900-05")]
    [InlineData("abc-2026")]
    [InlineData("12/12/2026")]
    [InlineData("2023-02-29")]
    [InlineData("2026-02-30")]
    [InlineData("2026-04-31")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(YearMonth.IsValid(input));
    }

    [Theory]
    [InlineData("2026-07", 2026, 7)]
    [InlineData("2026-7", 2026, 7)]
    [InlineData("07/2026", 2026, 7)]
    [InlineData("juli 2026", 2026, 7)]
    [InlineData("December 2025", 2025, 12)]
    [InlineData("2026-07-15", 2026, 7)]
    public void TryParse_ReturnsExpectedComponents(string input, int year, int month)
    {
        Assert.True(YearMonth.TryParse(input, out var ym));
        Assert.NotNull(ym);
        Assert.Equal(year, ym!.Year);
        Assert.Equal(month, ym.Month);
    }

    [Fact]
    public void ToNormalizedString_ReturnsIso8601()
    {
        var ym = YearMonth.Create(2026, 7);
        Assert.Equal("2026-07", ym.ToNormalizedString());
        Assert.Equal("2026-07", ym.ToString());
    }

    [Theory]
    [InlineData("2026-07", "2026-07")]
    [InlineData("juli 2026", "2026-07")]
    [InlineData("07/2026", "2026-07")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, YearMonth.Normalize(input));

    [Theory]
    [InlineData("2026-07", true)]
    [InlineData("2026-7", false)]
    [InlineData("juli 2026", false)]
    [InlineData(" 2026-07 ", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected) =>
        Assert.Equal(expected, YearMonth.IsNormalized(input));

    [Fact]
    public void Create_Throws_ForOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => YearMonth.Create(2026, 13));
        Assert.Throws<ArgumentOutOfRangeException>(() => YearMonth.Create(50, 1));
    }

    [Fact]
    public void ToMaskedString_ReturnsMasked()
    {
        var ym = YearMonth.Create(2026, 7);
        Assert.Equal("****-**", ym.ToMaskedString());
    }

    [Fact]
    public void FromDate_PreservesYearAndMonth()
    {
        var ym = YearMonth.FromDate(new DateOnly(2026, 7, 15));
        Assert.Equal(2026, ym.Year);
        Assert.Equal(7, ym.Month);
    }

    [Fact]
    public void Comparison_OrdersChronologically()
    {
        var a = YearMonth.Create(2026, 7);
        var b = YearMonth.Create(2026, 8);
        var c = YearMonth.Create(2027, 1);
        Assert.True(a < b);
        Assert.True(b < c);
        Assert.True(a < c);
    }

    [Fact]
    public void FirstAndLastDayOfMonth_AreCorrect()
    {
        var ym = YearMonth.Create(2024, 2);
        Assert.Equal(new DateOnly(2024, 2, 1), ym.ToFirstDayOfMonth());
        Assert.Equal(new DateOnly(2024, 2, 29), ym.ToLastDayOfMonth());
    }

    [Fact]
    public void Format_WithFallback_TrimsInvalidInput() =>
        Assert.Equal("nope", YearMonth.Format(" nope ", fallbackToTrimmedInputWhenInvalid: true));
}
