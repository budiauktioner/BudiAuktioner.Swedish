using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class SlovakAddressZipCodeTests
{
    [Theory]
    [InlineData("81101")]
    [InlineData("811 01")]
    [InlineData("SK 81101")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(SlovakAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("abcde")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(SlovakAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("81101", "81101", "811 01")]
    [InlineData("SK 81101", "81101", "811 01")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(SlovakAddressZipCode.TryParse(input, out var result));
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
        Assert.False(SlovakAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = SlovakAddressZipCode.Parse("81101");
        Assert.Equal("81101", zip.Value);
        Assert.Equal("811 01", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => SlovakAddressZipCode.Parse(input));

    [Theory]
    [InlineData("81101", "811 01")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, SlovakAddressZipCode.Format(input));

    [Theory]
    [InlineData("81101", "81101")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, SlovakAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("811 01", SlovakAddressZipCode.Parse("81101").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("81101", SlovakAddressZipCode.Parse("81101").ToNormalizedString());
}
