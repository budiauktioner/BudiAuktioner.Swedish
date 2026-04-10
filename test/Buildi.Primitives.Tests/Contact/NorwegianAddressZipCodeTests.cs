using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class NorwegianAddressZipCodeTests
{
    [Theory]
    [InlineData("0150")]
    [InlineData("5003")]
    [InlineData("NO 0150")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(NorwegianAddressZipCode.IsValid(input));
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
        Assert.False(NorwegianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("0150", "0150", "0150")]
    [InlineData("5003", "5003", "5003")]
    [InlineData("NO 0150", "0150", "0150")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(NorwegianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(NorwegianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = NorwegianAddressZipCode.Parse("0150");
        Assert.Equal("0150", zip.Value);
        Assert.Equal("0150", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => NorwegianAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("0150", "0150")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, NorwegianAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("NO 0150", "0150")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, NorwegianAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("0150", true)]
    [InlineData("NO 0150", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, NorwegianAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("0150", NorwegianAddressZipCode.Parse("0150").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("0150", NorwegianAddressZipCode.Parse("0150").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = NorwegianAddressZipCode.Parse("0150");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("0", masked);
    }
}
