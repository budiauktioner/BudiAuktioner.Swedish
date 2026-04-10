using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Rådhuspladsen 1, 1550 København — Copenhagen City Hall (Københavns Rådhus).
/// Frederiksborggade 21, 1360 København — Nationalmuseet (National Museum).
/// Store Torv 1, 8000 Aarhus — Aarhus Cathedral.
/// </summary>
public class DanishAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(DanishAddress.TryParse("Rådhuspladsen 1", "1550", "København", out var result));
        Assert.NotNull(result);
        Assert.Equal("København", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("DK", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(DanishAddress.TryParse("Rådhuspladsen 1", "1550", "København", "DK", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(DanishAddress.TryParse("Frederiksborggade 21", "1360", "København", out var result));
        Assert.NotNull(result);
        Assert.Equal("København", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(DanishAddress.TryParse("Store Torv 1", "8000", "Aarhus", out var result));
        Assert.NotNull(result);
        Assert.Equal("Aarhus", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(DanishAddress.TryParse("Rådhuspladsen 1", "1550", "København", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(DanishAddress.TryParse("Rådhuspladsen 1", "INVALID", "København", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(DanishAddress.TryParse("Rådhuspladsen 1", "1550", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = DanishAddress.Parse("Rådhuspladsen 1", "1550", "København");
        Assert.NotNull(addr);
        Assert.Equal("København", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => DanishAddress.Parse("Rådhuspladsen 1", "INVALID", "København"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(DanishAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = DanishAddress.Parse("Rådhuspladsen 1", "1550", "København");
        var s = addr.ToString();
        Assert.Contains("København", s);
        Assert.DoesNotContain("DK", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = DanishAddress.Parse("Rådhuspladsen 1", "1550", "København");
        var ml = addr.ToMultilineString();
        Assert.Contains("København", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = DanishAddress.Parse("Rådhuspladsen 1", "1550", "København");
        Assert.Contains("DK", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = DanishAddress.Parse("Rådhuspladsen 1", "1550", "København");
        var masked = addr.ToMaskedString();
        Assert.Contains("København", masked);
    }
}
