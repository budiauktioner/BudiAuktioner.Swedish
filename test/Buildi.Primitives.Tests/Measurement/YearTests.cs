using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class YearTests
{
    [Theory]
    [InlineData("2024")]
    [InlineData("1900")]
    [InlineData("9999")]
    [InlineData("1000")]
    [InlineData(" 2024 ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Year.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("999")]
    [InlineData("10000")]
    [InlineData("abcd")]
    [InlineData("2024-01")]
    [InlineData("'24")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Year.IsValid(input));
    }

    [Fact]
    public void TryParse_ReturnsExpectedValue()
    {
        Assert.True(Year.TryParse("2024", out var y));
        Assert.NotNull(y);
        Assert.Equal(2024, y!.Value);
    }

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => Year.Parse("abc"));

    [Fact]
    public void Create_Throws_ForOutOfRange() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Year.Create(123));

    [Theory]
    [InlineData("2024", "2024")]
    [InlineData(" 2024 ", "2024")]
    [InlineData("0500", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, Year.Normalize(input));

    [Theory]
    [InlineData("2024", true)]
    [InlineData(" 2024 ", false)]
    [InlineData("nope", false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected) =>
        Assert.Equal(expected, Year.IsNormalized(input));

    [Fact]
    public void ToMaskedString_ReturnsFourStars()
    {
        var y = Year.Create(2024);
        Assert.Equal("****", y.ToMaskedString());
    }

    [Fact]
    public void Equality_AndComparison_Works()
    {
        var a = Year.Create(2024);
        var b = Year.Parse("2024");
        var c = Year.Create(2025);
        Assert.True(a == b);
        Assert.True(a < c);
        Assert.True(a.Equals(b));
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Year", Year.TypeInfo.EnglishName);
        Assert.Equal("År", Year.TypeInfo.LocalizedName);
    }
}
