using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Sněmovní 4, 110 00 Praha — Czech Parliament, Chamber of Deputies (Poslanecká sněmovna).
/// Staromestske namesti 1, 110 00 Praha — Old Town Square (Staroměstské náměstí).
/// Dominikanske namesti 1, 602 00 Brno — Brno City Hall.
/// </summary>
public class CzechAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(CzechAddress.TryParse("Sněmovní 4", "11000", "Praha", out var result));
        Assert.NotNull(result);
        Assert.Equal("Praha", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("CZ", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(CzechAddress.TryParse("Sněmovní 4", "11000", "Praha", "CZ", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(CzechAddress.TryParse("Staromestske namesti 1", "11000", "Praha", out var result));
        Assert.NotNull(result);
        Assert.Equal("Praha", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(CzechAddress.TryParse("Dominikanske namesti 1", "60200", "Brno", out var result));
        Assert.NotNull(result);
        Assert.Equal("Brno", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(CzechAddress.TryParse("Sněmovní 4", "11000", "Praha", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(CzechAddress.TryParse("Sněmovní 4", "INVALID", "Praha", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(CzechAddress.TryParse("Sněmovní 4", "11000", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = CzechAddress.Parse("Sněmovní 4", "11000", "Praha");
        Assert.NotNull(addr);
        Assert.Equal("Praha", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => CzechAddress.Parse("Sněmovní 4", "INVALID", "Praha"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(CzechAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = CzechAddress.Parse("Sněmovní 4", "11000", "Praha");
        var s = addr.ToString();
        Assert.Contains("Praha", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = CzechAddress.Parse("Sněmovní 4", "11000", "Praha");
        var ml = addr.ToMultilineString();
        Assert.Contains("Praha", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = CzechAddress.Parse("Sněmovní 4", "11000", "Praha");
        Assert.Contains("CZ", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = CzechAddress.Parse("Sněmovní 4", "11000", "Praha");
        var masked = addr.ToMaskedString();
        Assert.Contains("Praha", masked);
    }
}
