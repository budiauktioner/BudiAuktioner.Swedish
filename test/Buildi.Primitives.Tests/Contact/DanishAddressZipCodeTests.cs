using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class DanishAddressZipCodeTests
{
    [Theory]
    [InlineData("1050")]
    [InlineData("8000")]
    [InlineData("DK 1050")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(DanishAddressZipCode.IsValid(input));
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
        Assert.False(DanishAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("1050", "1050", "1050")]
    [InlineData("8000", "8000", "8000")]
    [InlineData("DK 1050", "1050", "1050")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(DanishAddressZipCode.TryParse(input, out var result));
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
        Assert.False(DanishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = DanishAddressZipCode.Parse("1050");
        Assert.Equal("1050", zip.Value);
        Assert.Equal("1050", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => DanishAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("1050", "1050")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, DanishAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("DK 1050", "1050")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, DanishAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("1050", true)]
    [InlineData("DK 1050", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, DanishAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("1050", DanishAddressZipCode.Parse("1050").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("1050", DanishAddressZipCode.Parse("1050").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = DanishAddressZipCode.Parse("1050");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("1", masked);
    }
}
