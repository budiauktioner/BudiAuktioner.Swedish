using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class FrenchAddressZipCodeTests
{
    [Theory]
    [InlineData("75008")]
    [InlineData("13001")]
    [InlineData("FR 75008")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(FrenchAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(FrenchAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("75008", "75008", "75008")]
    [InlineData("FR 75008", "75008", "75008")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(FrenchAddressZipCode.TryParse(input, out var result));
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
        Assert.False(FrenchAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = FrenchAddressZipCode.Parse("75008");
        Assert.Equal("75008", zip.Value);
        Assert.Equal("75008", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => FrenchAddressZipCode.Parse(input));

    [Theory]
    [InlineData("75008", "75008")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, FrenchAddressZipCode.Format(input));

    [Theory]
    [InlineData("75008", "75008")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, FrenchAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("75008", FrenchAddressZipCode.Parse("75008").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("75008", FrenchAddressZipCode.Parse("75008").ToNormalizedString());
}
