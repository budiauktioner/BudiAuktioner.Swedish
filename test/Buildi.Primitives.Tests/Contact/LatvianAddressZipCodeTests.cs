using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class LatvianAddressZipCodeTests
{
    [Theory]
    [InlineData("1050")]
    [InlineData("LV-1050")]
    [InlineData("LV1050")]
    [InlineData("LV 1050")]
    [InlineData("LV4 729")]
    [InlineData("LV- 2130")]
    [InlineData("LV1 013")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(LatvianAddressZipCode.IsValid(input));
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
        Assert.False(LatvianAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("1050", "1050", "LV-1050")]
    [InlineData("LV-1050", "1050", "LV-1050")]
    [InlineData("LV1050", "1050", "LV-1050")]
    [InlineData("LV 1050", "1050", "LV-1050")]
    [InlineData("LV4 729", "4729", "LV-4729")]
    [InlineData("LV- 2130", "2130", "LV-2130")]
    [InlineData("LV1 013", "1013", "LV-1013")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(LatvianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(LatvianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = LatvianAddressZipCode.Parse("1050");
        Assert.Equal("1050", zip.Value);
        Assert.Equal("LV-1050", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => LatvianAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("1050", "LV-1050")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, LatvianAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("1050", "1050")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, LatvianAddressZipCode.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("LV-1050", LatvianAddressZipCode.Parse("1050").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("1050", LatvianAddressZipCode.Parse("1050").ToNormalizedString());
    }
}
