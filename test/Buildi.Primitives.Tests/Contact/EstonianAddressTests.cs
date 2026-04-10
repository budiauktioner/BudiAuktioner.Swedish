using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Estonia pst 4, 10143 Tallinn — near Estonia Theatre (Estonia Teater).
/// Lossi plats 1a, 15165 Tallinn — Riigikogu (Estonian Parliament).
/// Raekoja plats 1, 51003 Tartu — Tartu Town Hall (Tartu Raekoda).
/// </summary>
public class EstonianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(EstonianAddress.TryParse("Estonia pst 4", "10143", "Tallinn", out var result));
        Assert.NotNull(result);
        Assert.Equal("Tallinn", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("EE", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(EstonianAddress.TryParse("Estonia pst 4", "10143", "Tallinn", "EE", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(EstonianAddress.TryParse("Lossi plats 1a", "15165", "Tallinn", out var result));
        Assert.NotNull(result);
        Assert.Equal("Tallinn", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(EstonianAddress.TryParse("Raekoja plats 1", "51003", "Tartu", out var result));
        Assert.NotNull(result);
        Assert.Equal("Tartu", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(EstonianAddress.TryParse("Estonia pst 4", "10143", "Tallinn", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(EstonianAddress.TryParse("Estonia pst 4", "INVALID", "Tallinn", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(EstonianAddress.TryParse("Estonia pst 4", "10143", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = EstonianAddress.Parse("Estonia pst 4", "10143", "Tallinn");
        Assert.NotNull(addr);
        Assert.Equal("Tallinn", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => EstonianAddress.Parse("Estonia pst 4", "INVALID", "Tallinn"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(EstonianAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = EstonianAddress.Parse("Estonia pst 4", "10143", "Tallinn");
        var s = addr.ToString();
        Assert.Contains("Tallinn", s);
        Assert.DoesNotContain("EE", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = EstonianAddress.Parse("Estonia pst 4", "10143", "Tallinn");
        var ml = addr.ToMultilineString();
        Assert.Contains("Tallinn", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = EstonianAddress.Parse("Estonia pst 4", "10143", "Tallinn");
        Assert.Contains("EE", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = EstonianAddress.Parse("Estonia pst 4", "10143", "Tallinn");
        var masked = addr.ToMaskedString();
        Assert.Contains("Tallinn", masked);
    }
}
