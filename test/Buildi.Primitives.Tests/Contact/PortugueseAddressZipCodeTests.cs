using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class PortugueseAddressZipCodeTests
{
    [Theory]
    [InlineData("1100-148")]
    [InlineData("1100148")]
    [InlineData("PT 1100-148")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(PortugueseAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("abcdefg")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(PortugueseAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("1100148", "1100148", "1100-148")]
    [InlineData("1100-148", "1100148", "1100-148")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(PortugueseAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456")]
    [InlineData("12345678")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(PortugueseAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = PortugueseAddressZipCode.Parse("1100-148");
        Assert.Equal("1100148", zip.Value);
        Assert.Equal("1100-148", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => PortugueseAddressZipCode.Parse(input));

    [Theory]
    [InlineData("1100-148", "1100-148")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, PortugueseAddressZipCode.Format(input));

    [Theory]
    [InlineData("1100148", "1100148")]
    [InlineData("1100-148", "1100148")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, PortugueseAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("1100-148", PortugueseAddressZipCode.Parse("1100148").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("1100148", PortugueseAddressZipCode.Parse("1100148").ToNormalizedString());
}
