using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class CypriotAddressZipCodeTests
{
    [Theory]
    [InlineData("1060")]
    [InlineData("3040")]
    [InlineData("CY 1060")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(CypriotAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcd")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(CypriotAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("1060", "1060", "1060")]
    [InlineData("CY 1060", "1060", "1060")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(CypriotAddressZipCode.TryParse(input, out var result));
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
        Assert.False(CypriotAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = CypriotAddressZipCode.Parse("1060");
        Assert.Equal("1060", zip.Value);
        Assert.Equal("1060", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => CypriotAddressZipCode.Parse(input));

    [Theory]
    [InlineData("1060", "1060")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, CypriotAddressZipCode.Format(input));

    [Theory]
    [InlineData("1060", "1060")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, CypriotAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("1060", CypriotAddressZipCode.Parse("1060").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("1060", CypriotAddressZipCode.Parse("1060").ToNormalizedString());
}
