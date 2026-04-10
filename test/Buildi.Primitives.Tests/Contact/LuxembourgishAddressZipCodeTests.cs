using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class LuxembourgishAddressZipCodeTests
{
    [Theory]
    [InlineData("1648")]
    [InlineData("2163")]
    [InlineData("LU 1648")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(LuxembourgishAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcd")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(LuxembourgishAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("1648", "1648", "1648")]
    [InlineData("LU 1648", "1648", "1648")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(LuxembourgishAddressZipCode.TryParse(input, out var result));
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
        Assert.False(LuxembourgishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = LuxembourgishAddressZipCode.Parse("1648");
        Assert.Equal("1648", zip.Value);
        Assert.Equal("1648", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => LuxembourgishAddressZipCode.Parse(input));

    [Theory]
    [InlineData("1648", "1648")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, LuxembourgishAddressZipCode.Format(input));

    [Theory]
    [InlineData("1648", "1648")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, LuxembourgishAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("1648", LuxembourgishAddressZipCode.Parse("1648").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("1648", LuxembourgishAddressZipCode.Parse("1648").ToNormalizedString());
}
