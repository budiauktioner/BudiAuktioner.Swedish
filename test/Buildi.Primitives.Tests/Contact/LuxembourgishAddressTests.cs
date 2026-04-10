using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Place Guillaume II 9, 1648 Luxembourg — City Hall (Hôtel de Ville).
/// Place de la Constitution 1, 1475 Luxembourg — Gëlle Fra monument.
/// Marche-aux-Poissons 1, 2345 Luxembourg — National Museum of History and Art.
/// </summary>
public class LuxembourgishAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(LuxembourgishAddress.TryParse("Place Guillaume II 9", "1648", "Luxembourg", out var result));
        Assert.NotNull(result);
        Assert.Equal("Luxembourg", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("LU", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(LuxembourgishAddress.TryParse("Place Guillaume II 9", "1648", "Luxembourg", "LU", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(LuxembourgishAddress.TryParse("Place de la Constitution 1", "1475", "Luxembourg", out var result));
        Assert.NotNull(result);
        Assert.Equal("Luxembourg", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(LuxembourgishAddress.TryParse("Marche-aux-Poissons 1", "2345", "Luxembourg", out var result));
        Assert.NotNull(result);
        Assert.Equal("Luxembourg", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(LuxembourgishAddress.TryParse("Place Guillaume II 9", "1648", "Luxembourg", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(LuxembourgishAddress.TryParse("Place Guillaume II 9", "INVALID", "Luxembourg", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(LuxembourgishAddress.TryParse("Place Guillaume II 9", "1648", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = LuxembourgishAddress.Parse("Place Guillaume II 9", "1648", "Luxembourg");
        Assert.NotNull(addr);
        Assert.Equal("Luxembourg", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => LuxembourgishAddress.Parse("Place Guillaume II 9", "INVALID", "Luxembourg"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(LuxembourgishAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = LuxembourgishAddress.Parse("Place Guillaume II 9", "1648", "Luxembourg");
        Assert.Contains("Luxembourg", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = LuxembourgishAddress.Parse("Place Guillaume II 9", "1648", "Luxembourg");
        var ml = addr.ToMultilineString();
        Assert.Contains("Luxembourg", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = LuxembourgishAddress.Parse("Place Guillaume II 9", "1648", "Luxembourg");
        Assert.Contains("LU", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = LuxembourgishAddress.Parse("Place Guillaume II 9", "1648", "Luxembourg");
        var masked = addr.ToMaskedString();
        Assert.Contains("Luxembourg", masked);
    }
}
