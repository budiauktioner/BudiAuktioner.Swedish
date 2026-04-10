using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Mannerheimintie 30, 00100 Helsinki — Mannerheimintie boulevard.
/// Museokatu 1, 00100 Helsinki — Finnish National Museum (Kansallismuseo).
/// Hameenkatu 1, 33100 Tampere — Hämeenkatu main street.
/// </summary>
public class FinnishAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(FinnishAddress.TryParse("Mannerheimintie 30", "00100", "Helsinki", out var result));
        Assert.NotNull(result);
        Assert.Equal("Helsinki", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("FI", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(FinnishAddress.TryParse("Mannerheimintie 30", "00100", "Helsinki", "FI", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(FinnishAddress.TryParse("Museokatu 1", "00100", "Helsinki", out var result));
        Assert.NotNull(result);
        Assert.Equal("Helsinki", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(FinnishAddress.TryParse("Hameenkatu 1", "33100", "Tampere", out var result));
        Assert.NotNull(result);
        Assert.Equal("Tampere", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(FinnishAddress.TryParse("Mannerheimintie 30", "00100", "Helsinki", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(FinnishAddress.TryParse("Mannerheimintie 30", "INVALID", "Helsinki", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(FinnishAddress.TryParse("Mannerheimintie 30", "00100", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = FinnishAddress.Parse("Mannerheimintie 30", "00100", "Helsinki");
        Assert.NotNull(addr);
        Assert.Equal("Helsinki", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => FinnishAddress.Parse("Mannerheimintie 30", "INVALID", "Helsinki"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(FinnishAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = FinnishAddress.Parse("Mannerheimintie 30", "00100", "Helsinki");
        var s = addr.ToString();
        Assert.Contains("Helsinki", s);
        Assert.DoesNotContain("FI", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = FinnishAddress.Parse("Mannerheimintie 30", "00100", "Helsinki");
        var ml = addr.ToMultilineString();
        Assert.Contains("Helsinki", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = FinnishAddress.Parse("Mannerheimintie 30", "00100", "Helsinki");
        Assert.Contains("FI", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = FinnishAddress.Parse("Mannerheimintie 30", "00100", "Helsinki");
        var masked = addr.ToMaskedString();
        Assert.Contains("Helsinki", masked);
    }
}
