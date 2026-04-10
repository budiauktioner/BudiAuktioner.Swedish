using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class BritishAddressZipCodeTests
{
    [Theory]
    [InlineData("SW1A 1AA")]
    [InlineData("SW1A1AA")]
    [InlineData("sw1a 1aa")]
    [InlineData("EC1A 1BB")]
    [InlineData("M1 1AA")]
    [InlineData("B1 1AA")]
    [InlineData("W1A 1AB")]
    [InlineData("CR2 6XH")]
    [InlineData("LS18 5NF")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(BritishAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("ABCDE")]
    [InlineData("1234 AB")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(BritishAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("SW1A 1AA", "SW1A1AA", "SW1A 1AA")]
    [InlineData("SW1A1AA", "SW1A1AA", "SW1A 1AA")]
    [InlineData("sw1a1aa", "SW1A1AA", "SW1A 1AA")]
    [InlineData("M1 1AA", "M11AA", "M1 1AA")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(BritishAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("ABCDE")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(BritishAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = BritishAddressZipCode.Parse("SW1A 1AA");
        Assert.Equal("SW1A1AA", zip.Value);
        Assert.Equal("SW1A 1AA", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => BritishAddressZipCode.Parse(input));

    [Theory]
    [InlineData("SW1A1AA", "SW1A 1AA")]
    [InlineData("sw1a 1aa", "SW1A 1AA")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, BritishAddressZipCode.Format(input));

    [Theory]
    [InlineData("SW1A 1AA", "SW1A1AA")]
    [InlineData("sw1a1aa", "SW1A1AA")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, BritishAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("SW1A 1AA", BritishAddressZipCode.Parse("SW1A1AA").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("SW1A1AA", BritishAddressZipCode.Parse("SW1A1AA").ToNormalizedString());
}
