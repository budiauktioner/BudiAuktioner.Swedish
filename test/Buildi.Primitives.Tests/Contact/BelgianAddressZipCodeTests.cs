using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class BelgianAddressZipCodeTests
{
    [Theory]
    [InlineData("1000")]
    [InlineData("9000")]
    [InlineData("BE 1000")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(BelgianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcd")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(BelgianAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("1000", "1000", "1000")]
    [InlineData("BE 1000", "1000", "1000")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(BelgianAddressZipCode.TryParse(input, out var result));
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
        Assert.False(BelgianAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = BelgianAddressZipCode.Parse("1000");
        Assert.Equal("1000", zip.Value);
        Assert.Equal("1000", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => BelgianAddressZipCode.Parse(input));

    [Theory]
    [InlineData("1000", "1000")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, BelgianAddressZipCode.Format(input));

    [Theory]
    [InlineData("1000", "1000")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, BelgianAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("1000", BelgianAddressZipCode.Parse("1000").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("1000", BelgianAddressZipCode.Parse("1000").ToNormalizedString());
}
