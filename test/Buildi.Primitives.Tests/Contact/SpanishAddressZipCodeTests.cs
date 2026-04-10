using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class SpanishAddressZipCodeTests
{
    [Theory]
    [InlineData("28001")]
    [InlineData("08001")]
    [InlineData("ES 28001")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SpanishAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SpanishAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("28001", "28001", "28001")]
    [InlineData("ES 28001", "28001", "28001")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(SpanishAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("123456")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(SpanishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = SpanishAddressZipCode.Parse("28001");
        Assert.Equal("28001", zip.Value);
        Assert.Equal("28001", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SpanishAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("28001", "28001")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SpanishAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("28001", "28001")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SpanishAddressZipCode.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("28001", SpanishAddressZipCode.Parse("28001").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("28001", SpanishAddressZipCode.Parse("28001").ToNormalizedString());
    }
}
