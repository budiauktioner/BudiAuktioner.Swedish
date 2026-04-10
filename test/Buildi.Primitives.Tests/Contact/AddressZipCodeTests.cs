using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Contact;

public class AddressZipCodeTests
{
    [Theory]
    [InlineData("12345", true)]
    [InlineData("123 45", true)]
    [InlineData(" 1 2 3 4 5 ", true)]
    [InlineData("12-345", true)]
    [InlineData("1234", true)]
    [InlineData("123456", true)]
    [InlineData("abcde", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, AddressZipCode.IsValid(input));
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("123456")]
    public void TryParse_InternationalDigitOnly_NotSwedish(string input)
    {
        Assert.True(AddressZipCode.TryParse(input, out var result));
        Assert.False(result!.IsSwedish);
    }

    [Theory]
    [InlineData("12345", "12345")]
    [InlineData("123 45", "12345")]
    [InlineData(" 1 2 3 4 5 ", "12345")]
    [InlineData("432 15 Varberg", "43215")]
    [InlineData("74496 Järlåsa", "74496")]
    [InlineData("114 53 Stockholm", "11453")]
    [InlineData("43215 VARBERG", "43215")]
    public void TryParse_SwedishInput_ReturnsDigitsAndIsSwedish(string input, string expectedValue)
    {
        Assert.True(AddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.True(result.IsSwedish);
    }

    [Theory]
    [InlineData("12345", "123 45")]
    [InlineData("123 45", "123 45")]
    [InlineData("54321", "543 21")]
    public void TryParse_SwedishInput_ReturnsFormatted(string input, string expectedFormatted)
    {
        Assert.True(AddressZipCode.TryParse(input, out var result));
        Assert.Equal(expectedFormatted, result!.Formatted);
    }

    [Fact]
    public void ToString_Swedish_ReturnsFormatted()
    {
        var zip = AddressZipCode.Parse("12345");
        Assert.Equal("123 45", zip.ToString());
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => AddressZipCode.Parse("@@@@"));
    }

    [Theory]
    [InlineData("DK9000", "DK-9000")]
    [InlineData("DK-9000", "DK-9000")]
    [InlineData("1234", "1234")]
    [InlineData("123", "123")]
    [InlineData("W1A 1AB", "W1A1AB")]
    [InlineData("MD1234", "MD-1234")]
    [InlineData("MD-1234", "MD-1234")]
    [InlineData("AD123", "AD123")]
    [InlineData("10115", "10115")]
    [InlineData("1012 AB", "1012AB")]
    [InlineData("00100", "00100")]
    [InlineData("28001", "28001")]
    public void TryParseInternational_ValidFormats_ReturnsNormalized(string input, string expectedValue)
    {
        Assert.True(AddressZipCode.TryParseInternational(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
        Assert.False(result.IsSwedish);
    }

    [Theory]
    [InlineData("10115")]
    [InlineData("75008")]
    public void TryParse_FiveDigitOnly_PrefersSwedish(string input)
    {
        Assert.True(AddressZipCode.TryParse(input, out var result));
        Assert.True(result!.IsSwedish);
    }

    [Theory]
    [InlineData("DK-9000")]
    [InlineData("W1A 1AB")]
    [InlineData("MD-1234")]
    [InlineData("1012 AB")]
    public void TryParse_InternationalFormats_NotSwedish(string input)
    {
        Assert.True(AddressZipCode.TryParse(input, out var result));
        Assert.False(result!.IsSwedish);
    }

    // --- Country property ---

    [Fact]
    public void TryParse_SwedishZip_CountryIsSweden()
    {
        Assert.True(AddressZipCode.TryParse("11453", out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal("SE", result.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParseSwedish_CountryIsSweden()
    {
        Assert.True(AddressZipCode.TryParseSwedish("11453", out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal("SE", result.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParseInternational_DkPrefix_CountryIsDenmark()
    {
        Assert.True(AddressZipCode.TryParseInternational("DK-9000", out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal("DK", result.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParseInternational_MdPrefix_CountryIsMoldova()
    {
        Assert.True(AddressZipCode.TryParseInternational("MD-1234", out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal("MD", result.Country!.Alpha2Code);
    }

    [Theory]
    [InlineData("LV-1050", "LV-1050")]
    [InlineData("LV1050", "LV-1050")]
    [InlineData("LV 1050", "LV-1050")]
    [InlineData("LV4 729", "LV-4729")]
    [InlineData("LV1 013", "LV-1013")]
    [InlineData("LV2 170", "LV-2170")]
    [InlineData("LV- 2130", "LV-2130")]
    public void TryParseInternational_LatvianFormats_ReturnsNormalized(string input, string expectedValue)
    {
        Assert.True(AddressZipCode.TryParseInternational(input, out var result));
        Assert.Equal(expectedValue, result!.Value);
    }

    [Fact]
    public void TryParseInternational_LvPrefix_CountryIsLatvia()
    {
        Assert.True(AddressZipCode.TryParseInternational("LV-1050", out var result));
        Assert.NotNull(result!.Country);
        Assert.Equal("LV", result.Country!.Alpha2Code);
    }

    [Theory]
    [InlineData("LV4 729")]
    [InlineData("LV1 013")]
    [InlineData("LV2 170")]
    [InlineData("LV- 2130")]
    [InlineData("LV1 046")]
    public void TryParse_WithLatvianCountry_AcceptsLvPrefixVariants(string input)
    {
        Assert.True(AddressZipCode.TryParse(input, Country.Latvia, out var result));
        Assert.Equal("LV", result!.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParseInternational_UnknownPrefix_CountryIsNull()
    {
        Assert.True(AddressZipCode.TryParseInternational("W1A1AB", out var result));
        Assert.Null(result!.Country);
    }

    [Fact]
    public void TryParseInternational_WithExplicitCountry_CountryIsSet()
    {
        var norway = Country.Norway;
        Assert.True(AddressZipCode.TryParseInternational("1234", norway, out var result));
        Assert.Equal("NO", result!.Country!.Alpha2Code);
    }

    // --- TryParse with country parameter ---

    [Fact]
    public void TryParse_WithSwedishCountry_AcceptsSwedishFormat()
    {
        Assert.True(AddressZipCode.TryParse("11453", Country.Sweden, out var result));
        Assert.True(result!.IsSwedish);
        Assert.Equal("SE", result.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_WithSwedishCountry_RejectsInternationalFormat()
    {
        Assert.False(AddressZipCode.TryParse("DK-9000", Country.Sweden, out _));
    }

    [Fact]
    public void TryParse_WithNonSwedishCountry_UsesInternationalParsing()
    {
        Assert.True(AddressZipCode.TryParse("1234", Country.Norway, out var result));
        Assert.False(result!.IsSwedish);
        Assert.Equal("NO", result.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_WithNonSwedishCountry_RejectsNonMatchingFormat()
    {
        // Swedish 5-digit codes should not be accepted when a non-Swedish country is specified
        Assert.False(AddressZipCode.TryParse("ZZZZZ", Country.Norway, out _));
    }

    [Fact]
    public void TryParse_WithNullCountry_BehavesLikeDefault()
    {
        // null country = same as no-country overload: tries Swedish first
        Assert.True(AddressZipCode.TryParse("11453", null, out var result));
        Assert.True(result!.IsSwedish);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = AddressZipCode.Parse("123 45");
        var b = AddressZipCode.Parse("12345");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = AddressZipCode.Parse("12345");
        var b = AddressZipCode.Parse("54321");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = AddressZipCode.Parse("12345");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = AddressZipCode.Parse("12345");
        var b = AddressZipCode.Parse("54321");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = AddressZipCode.Parse("114 53");
        Assert.Equal(1, a.CompareTo(null));
    }
}
