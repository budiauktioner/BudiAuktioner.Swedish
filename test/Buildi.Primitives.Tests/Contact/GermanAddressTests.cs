using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Platz der Republik 1, 11011 Berlin — Reichstag building.
/// Museumsinsel 1, 10178 Berlin — Museum Island (Museumsinsel).
/// Marienplatz 8, 80331 Munchen — Munich New City Hall (Neues Rathaus).
/// </summary>
public class GermanAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(GermanAddress.TryParse("Platz der Republik 1", "11011", "Berlin", out var result));
        Assert.NotNull(result);
        Assert.Equal("Berlin", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("DE", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(GermanAddress.TryParse("Platz der Republik 1", "11011", "Berlin", "DE", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(GermanAddress.TryParse("Museumsinsel 1", "10178", "Berlin", out var result));
        Assert.NotNull(result);
        Assert.Equal("Berlin", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(GermanAddress.TryParse("Marienplatz 8", "80331", "Munchen", out var result));
        Assert.NotNull(result);
        Assert.Equal("Munchen", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(GermanAddress.TryParse("Platz der Republik 1", "11011", "Berlin", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(GermanAddress.TryParse("Platz der Republik 1", "INVALID", "Berlin", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(GermanAddress.TryParse("Platz der Republik 1", "11011", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = GermanAddress.Parse("Platz der Republik 1", "11011", "Berlin");
        Assert.NotNull(addr);
        Assert.Equal("Berlin", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => GermanAddress.Parse("Platz der Republik 1", "INVALID", "Berlin"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(GermanAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = GermanAddress.Parse("Platz der Republik 1", "11011", "Berlin");
        var s = addr.ToString();
        Assert.Contains("Berlin", s);
        Assert.DoesNotContain("DE", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = GermanAddress.Parse("Platz der Republik 1", "11011", "Berlin");
        var ml = addr.ToMultilineString();
        Assert.Contains("Berlin", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = GermanAddress.Parse("Platz der Republik 1", "11011", "Berlin");
        Assert.Contains("DE", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = GermanAddress.Parse("Platz der Republik 1", "11011", "Berlin");
        var masked = addr.ToMaskedString();
        Assert.Contains("Berlin", masked);
    }
}
