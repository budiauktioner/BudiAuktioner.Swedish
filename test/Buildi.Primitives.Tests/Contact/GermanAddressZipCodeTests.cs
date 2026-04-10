using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class GermanAddressZipCodeTests
{
    [Theory]
    [InlineData("10115")]
    [InlineData("80331")]
    [InlineData("DE 10115")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(GermanAddressZipCode.IsValid(input));
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
        Assert.False(GermanAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("10115", "10115", "10115")]
    [InlineData("80331", "80331", "80331")]
    [InlineData("DE 10115", "10115", "10115")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(GermanAddressZipCode.TryParse(input, out var result));
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
        Assert.False(GermanAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = GermanAddressZipCode.Parse("10115");
        Assert.Equal("10115", zip.Value);
        Assert.Equal("10115", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => GermanAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("10115", "10115")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, GermanAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("DE 10115", "10115")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, GermanAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("10115", true)]
    [InlineData("DE 10115", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, GermanAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("10115", GermanAddressZipCode.Parse("10115").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("10115", GermanAddressZipCode.Parse("10115").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = GermanAddressZipCode.Parse("10115");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("1", masked);
    }
}
