using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class SwissAddressZipCodeTests
{
    [Theory]
    [InlineData("3005")]
    [InlineData("8001")]
    [InlineData("CH 3005")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(SwissAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcd")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(SwissAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("3005", "3005", "3005")]
    [InlineData("CH 3005", "3005", "3005")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(SwissAddressZipCode.TryParse(input, out var result));
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
        Assert.False(SwissAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = SwissAddressZipCode.Parse("3005");
        Assert.Equal("3005", zip.Value);
        Assert.Equal("3005", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => SwissAddressZipCode.Parse(input));

    [Theory]
    [InlineData("3005", "3005")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, SwissAddressZipCode.Format(input));

    [Theory]
    [InlineData("3005", "3005")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, SwissAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("3005", SwissAddressZipCode.Parse("3005").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("3005", SwissAddressZipCode.Parse("3005").ToNormalizedString());
}
