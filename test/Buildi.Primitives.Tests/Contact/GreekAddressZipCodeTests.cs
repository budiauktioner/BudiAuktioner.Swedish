using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class GreekAddressZipCodeTests
{
    [Theory]
    [InlineData("10674")]
    [InlineData("106 74")]
    [InlineData("GR 10674")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(GreekAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(GreekAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("10674", "10674", "106 74")]
    [InlineData("GR 10674", "10674", "106 74")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(GreekAddressZipCode.TryParse(input, out var result));
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
        Assert.False(GreekAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = GreekAddressZipCode.Parse("10674");
        Assert.Equal("10674", zip.Value);
        Assert.Equal("106 74", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => GreekAddressZipCode.Parse(input));

    [Theory]
    [InlineData("10674", "106 74")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, GreekAddressZipCode.Format(input));

    [Theory]
    [InlineData("10674", "10674")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, GreekAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("106 74", GreekAddressZipCode.Parse("10674").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("10674", GreekAddressZipCode.Parse("10674").ToNormalizedString());
}
