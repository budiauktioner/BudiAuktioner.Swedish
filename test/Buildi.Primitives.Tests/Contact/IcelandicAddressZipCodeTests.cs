using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class IcelandicAddressZipCodeTests
{
    [Theory]
    [InlineData("101")]
    [InlineData("600")]
    [InlineData("IS 101")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(IcelandicAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12")]
    [InlineData("1234")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(IcelandicAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("101", "101", "101")]
    [InlineData("IS 101", "101", "101")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(IcelandicAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("1234")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(IcelandicAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = IcelandicAddressZipCode.Parse("101");
        Assert.Equal("101", zip.Value);
        Assert.Equal("101", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => IcelandicAddressZipCode.Parse(input));

    [Theory]
    [InlineData("101", "101")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, IcelandicAddressZipCode.Format(input));

    [Theory]
    [InlineData("101", "101")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, IcelandicAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("101", IcelandicAddressZipCode.Parse("101").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("101", IcelandicAddressZipCode.Parse("101").ToNormalizedString());
}
