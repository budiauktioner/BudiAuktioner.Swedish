using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Plac Defilad 1, 00-901 Warszawa — Palace of Culture and Science (Pałac Kultury i Nauki).
/// ul. Krakowskie Przedmiescie 26, 00-927 Warszawa — University of Warsaw.
/// Rynek Glowny 1, 31-042 Krakow — Main Market Square (Rynek Główny).
/// </summary>
public class PolishAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(PolishAddress.TryParse("Plac Defilad 1", "00-901", "Warszawa", out var result));
        Assert.NotNull(result);
        Assert.Equal("Warszawa", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("PL", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(PolishAddress.TryParse("Plac Defilad 1", "00-901", "Warszawa", "PL", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(PolishAddress.TryParse("ul. Krakowskie Przedmiescie 26", "00-927", "Warszawa", out var result));
        Assert.NotNull(result);
        Assert.Equal("Warszawa", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(PolishAddress.TryParse("Rynek Glowny 1", "31-042", "Krakow", out var result));
        Assert.NotNull(result);
        Assert.Equal("Krakow", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(PolishAddress.TryParse("Plac Defilad 1", "00-901", "Warszawa", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(PolishAddress.TryParse("Plac Defilad 1", "INVALID", "Warszawa", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(PolishAddress.TryParse("Plac Defilad 1", "00-901", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = PolishAddress.Parse("Plac Defilad 1", "00-901", "Warszawa");
        Assert.NotNull(addr);
        Assert.Equal("Warszawa", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => PolishAddress.Parse("Plac Defilad 1", "INVALID", "Warszawa"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(PolishAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = PolishAddress.Parse("Plac Defilad 1", "00-901", "Warszawa");
        var s = addr.ToString();
        Assert.Contains("Warszawa", s);
        Assert.DoesNotContain("PL", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = PolishAddress.Parse("Plac Defilad 1", "00-901", "Warszawa");
        var ml = addr.ToMultilineString();
        Assert.Contains("Warszawa", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = PolishAddress.Parse("Plac Defilad 1", "00-901", "Warszawa");
        Assert.Contains("PL", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = PolishAddress.Parse("Plac Defilad 1", "00-901", "Warszawa");
        var masked = addr.ToMaskedString();
        Assert.Contains("Warszawa", masked);
    }
}
