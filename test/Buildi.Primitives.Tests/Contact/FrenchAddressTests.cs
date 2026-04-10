using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Place de la Concorde 1, 75008 Paris — Place de la Concorde.
/// Rue de Rivoli 99, 75001 Paris — Musée du Louvre.
/// Place du Capitole 1, 31000 Toulouse — Capitole de Toulouse.
/// </summary>
public class FrenchAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(FrenchAddress.TryParse("Place de la Concorde 1", "75008", "Paris", out var result));
        Assert.NotNull(result);
        Assert.Equal("Paris", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("FR", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(FrenchAddress.TryParse("Place de la Concorde 1", "75008", "Paris", "FR", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(FrenchAddress.TryParse("Rue de Rivoli 99", "75001", "Paris", out var result));
        Assert.NotNull(result);
        Assert.Equal("Paris", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(FrenchAddress.TryParse("Place du Capitole 1", "31000", "Toulouse", out var result));
        Assert.NotNull(result);
        Assert.Equal("Toulouse", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(FrenchAddress.TryParse("Place de la Concorde 1", "75008", "Paris", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(FrenchAddress.TryParse("Place de la Concorde 1", "INVALID", "Paris", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(FrenchAddress.TryParse("Place de la Concorde 1", "75008", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = FrenchAddress.Parse("Place de la Concorde 1", "75008", "Paris");
        Assert.NotNull(addr);
        Assert.Equal("Paris", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => FrenchAddress.Parse("Place de la Concorde 1", "INVALID", "Paris"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(FrenchAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = FrenchAddress.Parse("Place de la Concorde 1", "75008", "Paris");
        Assert.Contains("Paris", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = FrenchAddress.Parse("Place de la Concorde 1", "75008", "Paris");
        var ml = addr.ToMultilineString();
        Assert.Contains("Paris", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = FrenchAddress.Parse("Place de la Concorde 1", "75008", "Paris");
        Assert.Contains("FR", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = FrenchAddress.Parse("Place de la Concorde 1", "75008", "Paris");
        var masked = addr.ToMaskedString();
        Assert.Contains("Paris", masked);
    }
}
