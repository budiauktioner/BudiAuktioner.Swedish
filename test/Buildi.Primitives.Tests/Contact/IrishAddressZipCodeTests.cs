using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class IrishAddressZipCodeTests
{
    [Theory]
    [InlineData("D02 XR20")]
    [InlineData("D02XR20")]
    [InlineData("d02 xr20")]
    [InlineData("A65 F4E2")]
    [InlineData("T12 A2WT")]
    [InlineData("V94 T9PX")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(IrishAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("ABCDEFG")]
    [InlineData("123 4567")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(IrishAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("D02 XR20", "D02XR20", "D02 XR20")]
    [InlineData("D02XR20", "D02XR20", "D02 XR20")]
    [InlineData("d02xr20", "D02XR20", "D02 XR20")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(IrishAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("ABCDEFG")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(IrishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = IrishAddressZipCode.Parse("D02 XR20");
        Assert.Equal("D02XR20", zip.Value);
        Assert.Equal("D02 XR20", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => IrishAddressZipCode.Parse(input));

    [Theory]
    [InlineData("D02XR20", "D02 XR20")]
    [InlineData("d02 xr20", "D02 XR20")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, IrishAddressZipCode.Format(input));

    [Theory]
    [InlineData("D02 XR20", "D02XR20")]
    [InlineData("d02xr20", "D02XR20")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, IrishAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("D02 XR20", IrishAddressZipCode.Parse("D02XR20").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("D02XR20", IrishAddressZipCode.Parse("D02XR20").ToNormalizedString());
}
