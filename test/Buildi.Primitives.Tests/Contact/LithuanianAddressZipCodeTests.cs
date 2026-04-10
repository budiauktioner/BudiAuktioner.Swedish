using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class LithuanianAddressZipCodeTests
{
    [Theory]
    [InlineData("01001")]
    [InlineData("LT-01001")]
    [InlineData("LT01001")]
    [InlineData("08200")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(LithuanianAddressZipCode.IsValid(input));
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
        Assert.False(LithuanianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("01001", "01001", "LT-01001")]
    [InlineData("LT-01001", "01001", "LT-01001")]
    [InlineData("LT01001", "01001", "LT-01001")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(LithuanianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(LithuanianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = LithuanianAddressZipCode.Parse("01001");
        Assert.Equal("01001", zip.Value);
        Assert.Equal("LT-01001", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => LithuanianAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("01001", "LT-01001")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, LithuanianAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("LT-01001", "01001")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, LithuanianAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("01001", true)]
    [InlineData("LT-01001", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, LithuanianAddressZipCode.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("LT-01001", LithuanianAddressZipCode.Parse("01001").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("01001", LithuanianAddressZipCode.Parse("01001").ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksDigits()
    {
        var zip = LithuanianAddressZipCode.Parse("01001");
        var masked = zip.ToMaskedString();
        Assert.DoesNotContain("0", masked);
    }
}
