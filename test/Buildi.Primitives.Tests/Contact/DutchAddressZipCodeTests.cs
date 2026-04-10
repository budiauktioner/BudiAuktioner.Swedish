using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class DutchAddressZipCodeTests
{
    [Theory]
    [InlineData("1012AB")]
    [InlineData("1012 AB")]
    [InlineData("1012ab")]
    [InlineData("NL1012AB")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(DutchAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1012")]
    [InlineData("1012A")]
    [InlineData("1012ABC")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(DutchAddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("1012AB", "1012AB", "1012 AB")]
    [InlineData("NL1012AB", "1012AB", "1012 AB")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedFormatted)
    {
        Assert.True(DutchAddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.Equal(expectedFormatted, result.Formatted);
        Assert.NotNull(result.ZipCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1012")]
    [InlineData("1012A")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(DutchAddressZipCode.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void Parse_ReturnsInstance_ForValid()
    {
        var zip = DutchAddressZipCode.Parse("1012AB");
        Assert.Equal("1012AB", zip.Value);
        Assert.Equal("1012 AB", zip.Formatted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1012")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => DutchAddressZipCode.Parse(input));
    }

    [Theory]
    [InlineData("1012AB", "1012 AB")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, DutchAddressZipCode.Format(input));
    }

    [Theory]
    [InlineData("1012AB", "1012AB")]
    [InlineData("1012 AB", "1012AB")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, DutchAddressZipCode.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsFormatted()
    {
        Assert.Equal("1012 AB", DutchAddressZipCode.Parse("1012AB").ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        Assert.Equal("1012AB", DutchAddressZipCode.Parse("1012AB").ToNormalizedString());
    }
}
