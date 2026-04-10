using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class BulgarianAddressZipCodeTests
{
    [Theory]
    [InlineData("1000")]
    [InlineData("4000")]
    [InlineData("BG 1000")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(BulgarianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(BulgarianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("1000", "1000", "1000")]
    [InlineData("BG 1000", "1000", "1000")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(BulgarianAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(BulgarianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = BulgarianAddressZipCode.Parse("1000");
        Assert.Equal("1000", zip.Value);
        Assert.Equal("1000", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => BulgarianAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("1000", "1000")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, BulgarianAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("1000", "1000")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, BulgarianAddressZipCode.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("1000", BulgarianAddressZipCode.Parse("1000").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("1000", BulgarianAddressZipCode.Parse("1000").ToNormalizedString());
    }
}
