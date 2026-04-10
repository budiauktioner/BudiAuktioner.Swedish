using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Gedimino pr. 53, 01109 Vilnius — Seimas (Lithuanian Parliament).
/// Arsenalo g. 1, 01143 Vilnius — National Museum of Lithuania.
/// Laisves al. 96, 44251 Kaunas — Kaunas Town Hall (Kauno rotušė).
/// </summary>
public class LithuanianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(LithuanianAddress.TryParse("Gedimino pr. 53", "01109", "Vilnius", out var result));
        Assert.NotNull(result);
        Assert.Equal("Vilnius", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("LT", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(LithuanianAddress.TryParse("Gedimino pr. 53", "01109", "Vilnius", "LT", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(LithuanianAddress.TryParse("Arsenalo g. 1", "01143", "Vilnius", out var result));
        Assert.NotNull(result);
        Assert.Equal("Vilnius", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(LithuanianAddress.TryParse("Laisves al. 96", "44251", "Kaunas", out var result));
        Assert.NotNull(result);
        Assert.Equal("Kaunas", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(LithuanianAddress.TryParse("Gedimino pr. 53", "01109", "Vilnius", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(LithuanianAddress.TryParse("Gedimino pr. 53", "INVALID", "Vilnius", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(LithuanianAddress.TryParse("Gedimino pr. 53", "01109", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = LithuanianAddress.Parse("Gedimino pr. 53", "01109", "Vilnius");
        Assert.NotNull(addr);
        Assert.Equal("Vilnius", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => LithuanianAddress.Parse("Gedimino pr. 53", "INVALID", "Vilnius"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(LithuanianAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = LithuanianAddress.Parse("Gedimino pr. 53", "01109", "Vilnius");
        var s = addr.ToString();
        Assert.Contains("Vilnius", s);
        // Zip display is LT-01109 (includes alpha2 as prefix); ensure no trailing country suffix after city.
        Assert.DoesNotContain("Vilnius, LT", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = LithuanianAddress.Parse("Gedimino pr. 53", "01109", "Vilnius");
        var ml = addr.ToMultilineString();
        Assert.Contains("Vilnius", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = LithuanianAddress.Parse("Gedimino pr. 53", "01109", "Vilnius");
        Assert.Contains("LT", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = LithuanianAddress.Parse("Gedimino pr. 53", "01109", "Vilnius");
        var masked = addr.ToMaskedString();
        Assert.Contains("Vilnius", masked);
    }
}
