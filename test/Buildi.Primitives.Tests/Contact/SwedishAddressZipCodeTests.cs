using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class SwedishAddressZipCodeTests
{
    // --- IsValid ---

    [Theory]
    [InlineData("12345")]
    [InlineData("123 45")]
    [InlineData(" 1 2 3 4 5 ")]
    [InlineData("12-345")]
    [InlineData("11453")]
    [InlineData("114 53")]
    [InlineData("SE 114 53")]
    [InlineData("SE-114 53")]
    [InlineData("SE12345")]
    [InlineData("s-114 53")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("abcde")]
    [InlineData("DK-9000")]
    [InlineData("W1A 1AB")]
    [InlineData("1012 AB")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishAddressZipCode.IsValid(input));
    }

    // --- TryParse ---

    [Theory]
    [InlineData("12345", "12345", "123 45")]
    [InlineData("123 45", "12345", "123 45")]
    [InlineData(" 1 2 3 4 5 ", "12345", "123 45")]
    [InlineData("11453", "11453", "114 53")]
    [InlineData("114 53", "11453", "114 53")]
    [InlineData("SE 114 53", "11453", "114 53")]
    [InlineData("SE-114 53", "11453", "114 53")]
    [InlineData("SE12345", "12345", "123 45")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(SwedishAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
        Assert.True(result.ZipCode.IsSwedish);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("DK-9000")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(SwedishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    // --- Parse ---

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = SwedishAddressZipCode.Parse("114 53");
        Assert.Equal("11453", zip.Value);
        Assert.Equal("114 53", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("DK-9000")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishAddressZipCode.Parse(input));
    }

    // --- Format / Normalize ---

    [Theory]
    [InlineData("11453", "114 53")]
    [InlineData("114 53", "114 53")]
    [InlineData(null, null)]
    [InlineData("DK-9000", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishAddressZipCode.Format(input));
    }

    [Fact]
    public void Format_FallbackReturnsInput()
    {
        Assert.Equal("DK-9000", SwedishAddressZipCode.Format("DK-9000", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("11453", "11453")]
    [InlineData("114 53", "11453")]
    [InlineData(null, null)]
    [InlineData("DK-9000", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishAddressZipCode.Normalize(input));
    }

    [Theory]
    [InlineData("11453", true)]
    [InlineData("114 53", false)]
    [InlineData("DK-9000", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, SwedishAddressZipCode.IsNormalized(input));
    }

    // --- ToString ---

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("114 53", SwedishAddressZipCode.Parse("11453").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("11453", SwedishAddressZipCode.Parse("114 53").ToNormalizedString());
    }

    // --- Underlying ZipCode ---

    [Fact]
    public void ZipCode_ExposesUnderlyingType()
    {
        var swedish = SwedishAddressZipCode.Parse("11453");
        Assert.NotNull(swedish.ZipCode);
        Assert.Equal("11453", swedish.ZipCode.Value);
        Assert.Equal("114 53", swedish.ZipCode.Formatted);
        Assert.True(swedish.ZipCode.IsSwedish);
    }

    // --- Masking ---

    [Fact]
    public void ToMaskedString_DelegatesToUnderlying()
    {
        var zip = SwedishAddressZipCode.Parse("114 53");
        Assert.Equal("*** **", zip.ToMaskedString());
    }
}
