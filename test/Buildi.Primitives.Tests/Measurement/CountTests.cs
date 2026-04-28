using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class CountTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("5")]
    [InlineData("42")]
    [InlineData("1000")]
    [InlineData("1 345")]
    [InlineData("1.345")]
    [InlineData("1,345")]
    [InlineData("1 234 567")]
    [InlineData("1.234.567")]
    [InlineData("1,234,567")]
    [InlineData("5st")]
    [InlineData("5 st")]
    [InlineData("5 st.")]
    [InlineData("5 stk")]
    [InlineData("5 stycken")]
    [InlineData("5 stycke")]
    [InlineData("5 pcs")]
    [InlineData("5 pc")]
    [InlineData("5 pieces")]
    [InlineData("5 piece")]
    [InlineData("5 ea")]
    [InlineData("5 x")]
    [InlineData("1 345 st")]
    [InlineData("1 345st")]
    [InlineData("1.345 st")]
    [InlineData(" 5 ")]
    [InlineData("  5  st  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Count.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("-1")]
    [InlineData("-5 st")]
    [InlineData("1,5")]
    [InlineData("1.5")]
    [InlineData("12,50")]
    [InlineData("12.50")]
    [InlineData("abc")]
    [InlineData("five")]
    [InlineData("5 stx")]
    [InlineData("5 stuff")]
    [InlineData("5 kr")]
    [InlineData("st 5")]
    [InlineData("1000000001")]
    [InlineData("999999999999")]
    [InlineData(".5")]
    [InlineData("5.")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Count.IsValid(input));
    }

    [Theory]
    [InlineData("5", 5)]
    [InlineData("0", 0)]
    [InlineData("1 345", 1345)]
    [InlineData("1.345", 1345)]
    [InlineData("1,345", 1345)]
    [InlineData("5 st", 5)]
    [InlineData("5st", 5)]
    [InlineData("1 345 st", 1345)]
    [InlineData("1 345st", 1345)]
    [InlineData("12 stycken", 12)]
    [InlineData("7 pcs", 7)]
    public void TryParse_ReturnsExpectedValue(string input, int expected)
    {
        Assert.True(Count.TryParse(input, out var c));
        Assert.NotNull(c);
        Assert.Equal(expected, c!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("1,5")]
    public void TryParse_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Count.TryParse(input, out var c));
        Assert.Null(c);
    }

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => Count.Parse("abc"));

    [Fact]
    public void Create_Throws_ForNegative() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Count.Create(-1));

    [Fact]
    public void Create_Throws_ForAboveMax() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Count.Create(Count.MaxValue + 1));

    [Theory]
    [InlineData("5", "5")]
    [InlineData("5 st", "5")]
    [InlineData("1 345", "1345")]
    [InlineData("1.345", "1345")]
    [InlineData("1,345", "1345")]
    [InlineData("1 345 st", "1345")]
    [InlineData("1 345st", "1345")]
    [InlineData(" 42 stycken ", "42")]
    [InlineData("abc", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, Count.Normalize(input));

    [Theory]
    [InlineData("5", "5 st")]
    [InlineData("5 st", "5 st")]
    [InlineData("1345", "1 345 st")]
    [InlineData("1.345", "1 345 st")]
    [InlineData("1 345 st", "1 345 st")]
    [InlineData("abc", null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, Count.Format(input));

    [Theory]
    [InlineData("5", true)]
    [InlineData("0", true)]
    [InlineData("1345", true)]
    [InlineData("5 st", false)]
    [InlineData(" 5 ", false)]
    [InlineData("01", false)]
    [InlineData("abc", false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected) =>
        Assert.Equal(expected, Count.IsNormalized(input));

    [Fact]
    public void ToString_ReturnsDisplayForm()
    {
        Assert.Equal("5 st", Count.Create(5).ToString());
        Assert.Equal("1 345 st", Count.Create(1345).ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsDigitsOnly()
    {
        Assert.Equal("5", Count.Create(5).ToNormalizedString());
        Assert.Equal("1345", Count.Create(1345).ToNormalizedString());
    }

    [Fact]
    public void ToNaturalString_ReturnsGroupedDigits()
    {
        Assert.Equal("5", Count.Create(5).ToNaturalString());
        Assert.Equal("1 345", Count.Create(1345).ToNaturalString());
        Assert.Equal("1 234 567", Count.Create(1234567).ToNaturalString());
    }

    [Fact]
    public void ToMaskedString_ReturnsMaskedForm()
    {
        Assert.Equal("*** st", Count.Create(5).ToMaskedString());
    }

    [Fact]
    public void Equality_AndComparison_Works()
    {
        var a = Count.Create(5);
        var b = Count.Parse("5");
        var c = Count.Create(10);
        Assert.True(a == b);
        Assert.True(a < c);
        Assert.True(a.Equals(b));
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Count", Count.TypeInfo.EnglishName);
        Assert.Equal("Antal", Count.TypeInfo.LocalizedName);
        Assert.Equal("🔢", Count.TypeInfo.Emoji);
    }
}
