using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class MalteseAddressZipCodeTests
{
    [Theory]
    [InlineData("VLT 1535")]
    [InlineData("VLT1535")]
    [InlineData("vlt 1535")]
    [InlineData("MST 1150")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input) =>
        Assert.True(MalteseAddressZipCode.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("VL 1535")]
    [InlineData("VLTT 1535")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(MalteseAddressZipCode.IsValid(input));

    [Theory]
    [InlineData("VLT 1535", "VLT1535", "VLT 1535")]
    [InlineData("VLT1535", "VLT1535", "VLT 1535")]
    [InlineData("vlt1535", "VLT1535", "VLT 1535")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(MalteseAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("VL 1535")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(MalteseAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = MalteseAddressZipCode.Parse("VLT 1535");
        Assert.Equal("VLT1535", zip.Value);
        Assert.Equal("VLT 1535", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Parse_Throws_ForInvalidInputs(string input) =>
        Assert.Throws<ArgumentException>(() => MalteseAddressZipCode.Parse(input));

    [Theory]
    [InlineData("VLT1535", "VLT 1535")]
    [InlineData("vlt 1535", "VLT 1535")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, MalteseAddressZipCode.Format(input));

    [Theory]
    [InlineData("VLT 1535", "VLT1535")]
    [InlineData("vlt1535", "VLT1535")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected) =>
        Assert.Equal(expected, MalteseAddressZipCode.Normalize(input));

    [Fact]
    public void ToString_ReturnsFormatted() =>
        Assert.Equal("VLT 1535", MalteseAddressZipCode.Parse("VLT1535").ToString());

    [Fact]
    public void ToNormalizedString_ReturnsValue() =>
        Assert.Equal("VLT1535", MalteseAddressZipCode.Parse("VLT1535").ToNormalizedString());
}
