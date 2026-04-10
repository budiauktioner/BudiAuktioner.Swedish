using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class RomanianAddressZipCodeTests
{
    [Theory]
    [InlineData("010011")]
    [InlineData("400001")]
    [InlineData("RO 010011")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(RomanianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(RomanianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("010011", "010011", "010011")]
    [InlineData("400001", "400001", "400001")]
    [InlineData("RO 010011", "010011", "010011")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(RomanianAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(RomanianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = RomanianAddressZipCode.Parse("010011");
        Assert.Equal("010011", zip.Value);
        Assert.Equal("010011", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => RomanianAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("010011", "010011")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, RomanianAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("RO 010011", "010011")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, RomanianAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("010011", true)]
    [InlineData("RO 010011", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, RomanianAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("010011", RomanianAddressZipCode.Parse("010011").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("010011", RomanianAddressZipCode.Parse("010011").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = RomanianAddressZipCode.Parse("010011");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("0", masked);
    }
}
