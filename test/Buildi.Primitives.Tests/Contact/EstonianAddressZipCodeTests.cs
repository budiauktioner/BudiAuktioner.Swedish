using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class EstonianAddressZipCodeTests
{
    [Theory]
    [InlineData("10115")]
    [InlineData("51014")]
    [InlineData("EE 10115")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EstonianAddressZipCode.IsValid(input));
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
        Assert.False(EstonianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("10115", "10115", "10115")]
    [InlineData("51014", "51014", "51014")]
    [InlineData("EE 10115", "10115", "10115")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(EstonianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(EstonianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = EstonianAddressZipCode.Parse("10115");
        Assert.Equal("10115", zip.Value);
        Assert.Equal("10115", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => EstonianAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("10115", "10115")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EstonianAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("EE 10115", "10115")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EstonianAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("10115", true)]
    [InlineData("EE 10115", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, EstonianAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("10115", EstonianAddressZipCode.Parse("10115").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("10115", EstonianAddressZipCode.Parse("10115").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = EstonianAddressZipCode.Parse("10115");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("1", masked);
    }
}
