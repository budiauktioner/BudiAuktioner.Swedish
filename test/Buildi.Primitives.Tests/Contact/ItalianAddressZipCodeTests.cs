using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class ItalianAddressZipCodeTests
{
    [Theory]
    [InlineData("00186")]
    [InlineData("20121")]
    [InlineData("IT 00186")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(ItalianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(ItalianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("00186", "00186", "00186")]
    [InlineData("IT 00186", "00186", "00186")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(ItalianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(ItalianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = ItalianAddressZipCode.Parse("00186");
        Assert.Equal("00186", zip.Value);
        Assert.Equal("00186", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => ItalianAddressZipCode.Parse(input));

    [Theory]
    [InlineData("00186", "00186")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, ItalianAddressZipCode.Format(input));

    [Theory]
    [InlineData("00186", "00186")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, ItalianAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("00186", ItalianAddressZipCode.Parse("00186").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("00186", ItalianAddressZipCode.Parse("00186").ToNormalizedString());
}
