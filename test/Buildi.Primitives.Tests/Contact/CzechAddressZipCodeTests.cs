using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class CzechAddressZipCodeTests
{
    [Theory]
    [InlineData("11000")]
    [InlineData("110 00")]
    [InlineData("CZ 11000")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(CzechAddressZipCode.IsValid(input));
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
        Assert.False(CzechAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("11000", "11000", "110 00")]
    [InlineData("CZ 11000", "11000", "110 00")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(CzechAddressZipCode.TryParse(input, out var result));
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
        Assert.False(CzechAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = CzechAddressZipCode.Parse("11000");
        Assert.Equal("11000", zip.Value);
        Assert.Equal("110 00", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => CzechAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("11000", "110 00")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, CzechAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("11000", "11000")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, CzechAddressZipCode.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("110 00", CzechAddressZipCode.Parse("11000").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("11000", CzechAddressZipCode.Parse("11000").ToNormalizedString());
    }
}
