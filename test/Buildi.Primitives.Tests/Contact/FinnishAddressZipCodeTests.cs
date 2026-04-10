using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class FinnishAddressZipCodeTests
{
    [Theory]
    [InlineData("00100")]
    [InlineData("33100")]
    [InlineData("FI 00100")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(FinnishAddressZipCode.IsValid(input));
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
        Assert.False(FinnishAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("00100", "00100", "00100")]
    [InlineData("33100", "33100", "33100")]
    [InlineData("FI 00100", "00100", "00100")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(FinnishAddressZipCode.TryParse(input, out var result));
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
        Assert.False(FinnishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = FinnishAddressZipCode.Parse("00100");
        Assert.Equal("00100", zip.Value);
        Assert.Equal("00100", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => FinnishAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("00100", "00100")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, FinnishAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("FI 00100", "00100")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, FinnishAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("00100", true)]
    [InlineData("FI 00100", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, FinnishAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("00100", FinnishAddressZipCode.Parse("00100").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("00100", FinnishAddressZipCode.Parse("00100").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = FinnishAddressZipCode.Parse("00100");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("0", masked);
    }
}
