using System.Globalization;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Geography;

[Collection("CultureSensitive")]
public class ContinentTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();

    [Theory]
    [InlineData("AF")]
    [InlineData("AN")]
    [InlineData("AS")]
    [InlineData("EU")]
    [InlineData("NA")]
    [InlineData("OC")]
    [InlineData("SA")]
    [InlineData("Africa")]
    [InlineData("Antarctica")]
    [InlineData("Asia")]
    [InlineData("Europe")]
    [InlineData("North America")]
    [InlineData("Oceania")]
    [InlineData("South America")]
    [InlineData("Afrika")]
    [InlineData("Antarktis")]
    [InlineData("Asien")]
    [InlineData("Europa")]
    [InlineData("Nordamerika")]
    [InlineData("Oceanien")]
    [InlineData("Sydamerika")]
    [InlineData("eu")]
    [InlineData("europe")]
    [InlineData(" EU ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Continent.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XX")]
    [InlineData("Atlantis")]
    [InlineData("Eurasia")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Continent.IsValid(input));
    }

    [Theory]
    [InlineData("AF", "AF", "Africa", "Afrika")]
    [InlineData("Africa", "AF", "Africa", "Afrika")]
    [InlineData("Afrika", "AF", "Africa", "Afrika")]
    [InlineData("EU", "EU", "Europe", "Europa")]
    [InlineData("Europe", "EU", "Europe", "Europa")]
    [InlineData("Europa", "EU", "Europe", "Europa")]
    [InlineData("NA", "NA", "North America", "Nordamerika")]
    [InlineData("North America", "NA", "North America", "Nordamerika")]
    [InlineData("Nordamerika", "NA", "North America", "Nordamerika")]
    [InlineData("SA", "SA", "South America", "Sydamerika")]
    [InlineData("South America", "SA", "South America", "Sydamerika")]
    [InlineData("Sydamerika", "SA", "South America", "Sydamerika")]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(string input, string expectedCode, string expectedEnglish, string expectedSwedish)
    {
        var ok = Continent.TryParse(input, out var result);

        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedCode, result!.Code);
        Assert.Equal(expectedEnglish, result.EnglishName);
        Assert.Equal(expectedSwedish, result.LocalizedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XX")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = Continent.TryParse(input, out var result);

        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("XX")]
    [InlineData("Atlantis")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Continent.Parse(input));
    }

    [Theory]
    [InlineData("EU", "EU")]
    [InlineData("Europe", "EU")]
    [InlineData("Europa", "EU")]
    [InlineData("eu", "EU")]
    [InlineData(" EU ", "EU")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("XX", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Continent.Normalize(input));
    }

    [Theory]
    [InlineData("XX", "XX")]
    [InlineData("Atlantis", "Atlantis")]
    [InlineData(" bogus ", "bogus")]
    public void Normalize_WithFallback_ReturnsTrimmedInput(string input, string expected)
    {
        Assert.Equal(expected, Continent.Normalize(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("EU", true)]
    [InlineData("AF", true)]
    [InlineData("Europe", false)]
    [InlineData("eu", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, Continent.IsNormalized(input));
    }

    [Fact]
    public void Format_ReturnsEnglishName_ByDefault()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");

        Assert.Equal("Europe", Continent.Format("EU"));
        Assert.Equal("North America", Continent.Format("NA"));
    }

    [Fact]
    public void Format_ReturnsLocalizedName_WhenSwedishCulture()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        Assert.Equal("Europa", Continent.Format("EU"));
        Assert.Equal("Nordamerika", Continent.Format("NA"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("XX", null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, Continent.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput()
    {
        Assert.Equal("Atlantis", Continent.Format("Atlantis", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        Assert.Equal("Europa", Continent.Europe.ToString());

        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        Assert.Equal("Europe", Continent.Europe.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsCode()
    {
        Assert.Equal("EU", Continent.Europe.ToNormalizedString());
        Assert.Equal("AF", Continent.Africa.ToNormalizedString());
    }

    [Fact]
    public void All_ContainsSevenContinents()
    {
        Assert.Equal(7, Continent.All.Count);
    }

    [Fact]
    public void Equality_SameContinent_AreEqual()
    {
        var a = Continent.Parse("EU");
        var b = Continent.Parse("Europe");

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equality_DifferentContinents_AreNotEqual()
    {
        Assert.NotEqual(Continent.Europe, Continent.Asia);
        Assert.True(Continent.Europe != Continent.Asia);
    }

    [Fact]
    public void Country_Continent_ReturnsExpectedContinent()
    {
        var sweden = Country.Parse("SE");
        Assert.Equal(Continent.Europe, sweden.Continent);
        Assert.Equal("EU", sweden.Continent.Code);
        Assert.Equal("Europe", sweden.Continent.EnglishName);
        Assert.Equal("Europa", sweden.Continent.LocalizedName);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Continent.Parse("Africa");
        var b = Continent.Parse("Europe");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Continent.Parse("Europe");
        Assert.Equal(1, a.CompareTo(null));
    }
}
