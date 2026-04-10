using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class CroatianAddressZipCodeTests
{
    [Theory]
    [InlineData("10000")]
    [InlineData("21000")]
    [InlineData("HR 10000")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(CroatianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(CroatianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("10000", "10000", "10000")]
    [InlineData("HR 10000", "10000", "10000")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(CroatianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(CroatianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = CroatianAddressZipCode.Parse("10000");
        Assert.Equal("10000", zip.Value);
        Assert.Equal("10000", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => CroatianAddressZipCode.Parse(input));

    [Theory]
    [InlineData("10000", "10000")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, CroatianAddressZipCode.Format(input));

    [Theory]
    [InlineData("10000", "10000")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, CroatianAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("10000", CroatianAddressZipCode.Parse("10000").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("10000", CroatianAddressZipCode.Parse("10000").ToNormalizedString());
}
