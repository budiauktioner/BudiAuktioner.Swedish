using System.Globalization;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Finance;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests;

[Collection("CultureSensitive")]
public class LanguageFormattingTests : IDisposable
{
    public LanguageFormattingTests()
    {
        PrimitivesDefaults.Reset();
    }

    public void Dispose()
    {
        PrimitivesDefaults.Reset();
    }

    [Fact]
    public void Country_ToDisplayString_ReturnsSwedish_WhenUICultureIsSv()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        var country = Country.Parse("SE");
        Assert.Equal("Sverige", country.ToDisplayString());
    }

    [Fact]
    public void Country_ToDisplayString_ReturnsEnglish_WhenUICultureIsEn()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var country = Country.Parse("SE");
        Assert.Equal("Sweden", country.ToDisplayString());
    }

    [Fact]
    public void Country_ToEnglishString_ReturnsEnglish()
    {
        var country = Country.Parse("SE");
        Assert.Equal("Sweden", country.ToEnglishString());
    }

    [Fact]
    public void Country_ToNativeString_ReturnsEndonym()
    {
        var country = Country.Parse("DE");
        Assert.Equal("Deutschland", country.ToNativeString());
    }

    [Fact]
    public void Country_ToString_DelegatesToToDisplayString()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var country = Country.Parse("DE");
        Assert.Equal(country.ToDisplayString(), country.ToString());
    }

    [Fact]
    public void Country_DisplayName_MatchesToDisplayString()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        var country = Country.Parse("DE");
        Assert.Equal("Tyskland", country.DisplayName);
        Assert.Equal(country.DisplayName, country.ToDisplayString());
    }

    [Fact]
    public void SwedishCounty_ToDisplayString_ReturnsSwedish_WhenUICultureIsSv()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        var county = SwedishCounty.Parse("01");
        Assert.Equal("Stockholms län", county.ToDisplayString());
    }

    [Fact]
    public void SwedishCounty_ToDisplayString_ReturnsEnglish_WhenUICultureIsEn()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var county = SwedishCounty.Parse("01");
        Assert.Equal("Stockholm County", county.ToDisplayString());
    }

    [Fact]
    public void SwedishCounty_ToEnglishString_ReturnsEnglish()
    {
        var county = SwedishCounty.Parse("01");
        Assert.Equal("Stockholm County", county.ToEnglishString());
    }

    [Fact]
    public void SwedishCounty_ToLocalString_ReturnsSwedish()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var county = SwedishCounty.Parse("01");
        Assert.Equal("Stockholms län", county.ToLocalString());
    }

    [Fact]
    public void SwedishCounty_ToString_DelegatesToToDisplayString()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var county = SwedishCounty.Parse("01");
        Assert.Equal(county.ToDisplayString(), county.ToString());
    }

    [Fact]
    public void SwedishMunicipality_ToDisplayString_ReturnsSwedish_WhenUICultureIsSv()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        var muni = SwedishMunicipality.Parse("1480");
        Assert.Equal("Göteborg", muni.ToDisplayString());
    }

    [Fact]
    public void SwedishMunicipality_ToDisplayString_ReturnsEnglish_WhenUICultureIsEn()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var muni = SwedishMunicipality.Parse("1480");
        Assert.Equal("Gothenburg", muni.ToDisplayString());
    }

    [Fact]
    public void SwedishMunicipality_ToLocalString_ReturnsSwedish()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var muni = SwedishMunicipality.Parse("1480");
        Assert.Equal("Göteborg", muni.ToLocalString());
    }

    [Fact]
    public void Currency_ToDisplayString_ReturnsSwedish_WhenUICultureIsSv()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        Assert.Equal("Svensk krona", Currency.SEK.ToDisplayString());
    }

    [Fact]
    public void Currency_ToDisplayString_ReturnsEnglish_WhenUICultureIsEn()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        Assert.Equal("Swedish krona", Currency.SEK.ToDisplayString());
    }

    [Fact]
    public void Currency_ToEnglishString_ReturnsEnglish()
    {
        Assert.Equal("Swedish krona", Currency.SEK.ToEnglishString());
    }

    [Fact]
    public void Currency_ToNativeString_ReturnsNativeName()
    {
        Assert.Equal("Svensk krona", Currency.SEK.ToNativeString());
    }

    [Fact]
    public void Currency_ToString_DelegatesToToDisplayString()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        Assert.Equal(Currency.EUR.ToDisplayString(), Currency.EUR.ToString());
    }

    [Fact]
    public void Address_ToDisplayString_OmitsSwedishCountryForDomesticAddresses_WhenUICultureIsSv()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "SE");
        var display = address.ToDisplayString();
        Assert.DoesNotContain("Sverige", display);
        Assert.DoesNotContain("Sweden", display);
    }

    [Fact]
    public void Address_ToEnglishString_UsesEnglishCountryName()
    {
        var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "SE");
        var english = address.ToEnglishString();
        Assert.Contains("Sweden", english);
        Assert.DoesNotContain("Sverige", english);
    }

    [Fact]
    public void Address_ToNativeString_UsesCountryEndonym()
    {
        var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "DE");
        var native = address.ToNativeString();
        Assert.Contains("Deutschland", native);
    }

    [Fact]
    public void Address_ToString_DelegatesToToDisplayString()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "SE");
        Assert.Equal(address.ToDisplayString(), address.ToString());
    }

    [Fact]
    public void Address_ToMultilineString_UsesDisplayLanguage_Swedish()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");
        var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "DE");
        var multiline = address.ToMultilineString();
        Assert.Contains("Tyskland", multiline);
    }

    [Fact]
    public void Address_ToMultilineString_UsesDisplayLanguage_English()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("en-US");
        var address = Address.Parse("Storgatan 1", "114 53", "Stockholm", "DE");
        var multiline = address.ToMultilineString();
        Assert.Contains("Germany", multiline);
        Assert.DoesNotContain("Tyskland", multiline);
    }
}
