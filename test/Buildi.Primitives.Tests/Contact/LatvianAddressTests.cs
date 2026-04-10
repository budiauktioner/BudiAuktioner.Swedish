using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Jēkaba iela 11, LV-1050 Rīga — Saeima (Latvian Parliament).
/// Doma laukums 1, LV-1050 Riga — Riga Cathedral (Rīgas Doms).
/// Liela iela 14, LV-3401 Liepaja — Liepāja City Council.
/// </summary>
public class LatvianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(LatvianAddress.TryParse("Jēkaba iela 11", "1050", "Rīga", out var result));
        Assert.NotNull(result);
        Assert.Equal("Rīga", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("LV", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(LatvianAddress.TryParse("Jēkaba iela 11", "1050", "Rīga", "LV", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(LatvianAddress.TryParse("Doma laukums 1", "1050", "Riga", out var result));
        Assert.NotNull(result);
        Assert.Equal("Riga", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(LatvianAddress.TryParse("Liela iela 14", "3401", "Liepaja", out var result));
        Assert.NotNull(result);
        Assert.Equal("Liepaja", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(LatvianAddress.TryParse("Jēkaba iela 11", "1050", "Rīga", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(LatvianAddress.TryParse("Jēkaba iela 11", "INVALID", "Rīga", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(LatvianAddress.TryParse("Jēkaba iela 11", "1050", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = LatvianAddress.Parse("Jēkaba iela 11", "1050", "Rīga");
        Assert.NotNull(addr);
        Assert.Equal("Rīga", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => LatvianAddress.Parse("Jēkaba iela 11", "INVALID", "Rīga"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(LatvianAddress.IsValid(null));
    }

    [Fact]
    public void ToString_ContainsCity()
    {
        var addr = LatvianAddress.Parse("Jēkaba iela 11", "1050", "Rīga");
        var s = addr.ToString();
        Assert.Contains("Rīga", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = LatvianAddress.Parse("Jēkaba iela 11", "1050", "Rīga");
        var ml = addr.ToMultilineString();
        Assert.Contains("Rīga", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = LatvianAddress.Parse("Jēkaba iela 11", "1050", "Rīga");
        Assert.Contains("LV", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = LatvianAddress.Parse("Jēkaba iela 11", "1050", "Rīga");
        var masked = addr.ToMaskedString();
        Assert.Contains("Rīga", masked);
    }
}
