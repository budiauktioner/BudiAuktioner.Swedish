using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class PolishAddressZipCodeTests
{
    [Theory]
    [InlineData("00-950")]
    [InlineData("00950")]
    [InlineData("80-001")]
    [InlineData("PL 00-950")]
    [InlineData("PL00950")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(PolishAddressZipCode.IsValid(input));
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
        Assert.False(PolishAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("00950", "00950", "00-950")]
    [InlineData("00-950", "00950", "00-950")]
    [InlineData("PL 00-950", "00950", "00-950")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(PolishAddressZipCode.TryParse(input, out var result));
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
        Assert.False(PolishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = PolishAddressZipCode.Parse("00950");
        Assert.Equal("00950", zip.Value);
        Assert.Equal("00-950", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => PolishAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("00950", "00-950")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PolishAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("00-950", "00950")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, PolishAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("00950", true)]
    [InlineData("00-950", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, PolishAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("00-950", PolishAddressZipCode.Parse("00950").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("00950", PolishAddressZipCode.Parse("00950").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = PolishAddressZipCode.Parse("00950");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("0", masked);
    }
}
