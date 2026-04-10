using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class LiechtensteinAddressZipCodeTests
{
    [Theory]
    [InlineData("9490")]
    [InlineData("9495")]
    [InlineData("LI 9490")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(LiechtensteinAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcd")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(LiechtensteinAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("9490", "9490", "9490")]
    [InlineData("LI 9490", "9490", "9490")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(LiechtensteinAddressZipCode.TryParse(input, out var result));
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
        Assert.False(LiechtensteinAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = LiechtensteinAddressZipCode.Parse("9490");
        Assert.Equal("9490", zip.Value);
        Assert.Equal("9490", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => LiechtensteinAddressZipCode.Parse(input));

    [Theory]
    [InlineData("9490", "9490")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, LiechtensteinAddressZipCode.Format(input));

    [Theory]
    [InlineData("9490", "9490")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, LiechtensteinAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("9490", LiechtensteinAddressZipCode.Parse("9490").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("9490", LiechtensteinAddressZipCode.Parse("9490").ToNormalizedString());
}
