using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class HungarianAddressZipCodeTests
{
    [Theory]
    [InlineData("1055")]
    [InlineData("6720")]
    [InlineData("HU 1055")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(HungarianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcd")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(HungarianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("1055", "1055", "1055")]
    [InlineData("HU 1055", "1055", "1055")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(HungarianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(HungarianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = HungarianAddressZipCode.Parse("1055");
        Assert.Equal("1055", zip.Value);
        Assert.Equal("1055", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => HungarianAddressZipCode.Parse(input));

    [Theory]
    [InlineData("1055", "1055")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, HungarianAddressZipCode.Format(input));

    [Theory]
    [InlineData("1055", "1055")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, HungarianAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("1055", HungarianAddressZipCode.Parse("1055").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("1055", HungarianAddressZipCode.Parse("1055").ToNormalizedString());
}
