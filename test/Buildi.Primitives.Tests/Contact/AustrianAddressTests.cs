using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Ballhausplatz 2, 1010 Wien — Austrian Federal Chancellery (Bundeskanzleramt).
/// Maria-Theresien-Platz 1, 1010 Wien — Kunsthistorisches Museum.
/// Mozartplatz 1, 5020 Salzburg — Salzburg Museum.
/// </summary>
public class AustrianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(AustrianAddress.TryParse("Ballhausplatz 2", "1010", "Wien", out var result));
        Assert.NotNull(result);
        Assert.Equal("Wien", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("AT", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(AustrianAddress.TryParse("Ballhausplatz 2", "1010", "Wien", "AT", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(AustrianAddress.TryParse("Maria-Theresien-Platz 1", "1010", "Wien", out var result));
        Assert.NotNull(result);
        Assert.Equal("Wien", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(AustrianAddress.TryParse("Mozartplatz 1", "5020", "Salzburg", out var result));
        Assert.NotNull(result);
        Assert.Equal("Salzburg", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(AustrianAddress.TryParse("Ballhausplatz 2", "1010", "Wien", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(AustrianAddress.TryParse("Ballhausplatz 2", "INVALID", "Wien", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(AustrianAddress.TryParse("Ballhausplatz 2", "1010", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = AustrianAddress.Parse("Ballhausplatz 2", "1010", "Wien");
        Assert.NotNull(addr);
        Assert.Equal("Wien", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => AustrianAddress.Parse("Ballhausplatz 2", "INVALID", "Wien"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(AustrianAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = AustrianAddress.Parse("Ballhausplatz 2", "1010", "Wien");
        Assert.Contains("Wien", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = AustrianAddress.Parse("Ballhausplatz 2", "1010", "Wien");
        var ml = addr.ToMultilineString();
        Assert.Contains("Wien", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = AustrianAddress.Parse("Ballhausplatz 2", "1010", "Wien");
        Assert.Contains("AT", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = AustrianAddress.Parse("Ballhausplatz 2", "1010", "Wien");
        var masked = addr.ToMaskedString();
        Assert.Contains("Wien", masked);
    }
}
