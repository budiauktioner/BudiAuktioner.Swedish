using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Karl Johans gate 22, 0026 Oslo — Stortinget (Norwegian Parliament).
/// Brynjulf Bulls plass 1, 0250 Oslo — Oslo Opera House (Operahuset).
/// Olav Kyrres gate 49, 5014 Bergen — Grieghallen concert hall.
/// </summary>
public class NorwegianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(NorwegianAddress.TryParse("Karl Johans gate 22", "0026", "Oslo", out var result));
        Assert.NotNull(result);
        Assert.Equal("Oslo", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("NO", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(NorwegianAddress.TryParse("Karl Johans gate 22", "0026", "Oslo", "NO", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(NorwegianAddress.TryParse("Brynjulf Bulls plass 1", "0250", "Oslo", out var result));
        Assert.NotNull(result);
        Assert.Equal("Oslo", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(NorwegianAddress.TryParse("Olav Kyrres gate 49", "5014", "Bergen", out var result));
        Assert.NotNull(result);
        Assert.Equal("Bergen", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(NorwegianAddress.TryParse("Karl Johans gate 22", "0026", "Oslo", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(NorwegianAddress.TryParse("Karl Johans gate 22", "INVALID", "Oslo", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(NorwegianAddress.TryParse("Karl Johans gate 22", "0026", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = NorwegianAddress.Parse("Karl Johans gate 22", "0026", "Oslo");
        Assert.NotNull(addr);
        Assert.Equal("Oslo", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => NorwegianAddress.Parse("Karl Johans gate 22", "INVALID", "Oslo"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(NorwegianAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = NorwegianAddress.Parse("Karl Johans gate 22", "0026", "Oslo");
        var s = addr.ToString();
        Assert.Contains("Oslo", s);
        Assert.DoesNotContain("NO", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = NorwegianAddress.Parse("Karl Johans gate 22", "0026", "Oslo");
        var ml = addr.ToMultilineString();
        Assert.Contains("Oslo", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = NorwegianAddress.Parse("Karl Johans gate 22", "0026", "Oslo");
        Assert.Contains("NO", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = NorwegianAddress.Parse("Karl Johans gate 22", "0026", "Oslo");
        var masked = addr.ToMaskedString();
        Assert.Contains("Oslo", masked);
    }
}
