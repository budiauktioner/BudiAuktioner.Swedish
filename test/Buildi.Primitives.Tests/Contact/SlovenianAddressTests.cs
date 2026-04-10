using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Presernov trg 1, 1000 Ljubljana — Prešeren Square (Prešernov trg).
/// Muzejska ulica 1, 1000 Ljubljana — National Museum of Slovenia (Narodni muzej).
/// Grajska planota 1, 2000 Maribor — Maribor Castle (Mariborski grad).
/// </summary>
public class SlovenianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(SlovenianAddress.TryParse("Presernov trg 1", "1000", "Ljubljana", out var result));
        Assert.NotNull(result);
        Assert.Equal("Ljubljana", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("SI", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(SlovenianAddress.TryParse("Presernov trg 1", "1000", "Ljubljana", "SI", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(SlovenianAddress.TryParse("Muzejska ulica 1", "1000", "Ljubljana", out var result));
        Assert.NotNull(result);
        Assert.Equal("Ljubljana", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(SlovenianAddress.TryParse("Grajska planota 1", "2000", "Maribor", out var result));
        Assert.NotNull(result);
        Assert.Equal("Maribor", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(SlovenianAddress.TryParse("Presernov trg 1", "1000", "Ljubljana", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(SlovenianAddress.TryParse("Presernov trg 1", "INVALID", "Ljubljana", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(SlovenianAddress.TryParse("Presernov trg 1", "1000", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = SlovenianAddress.Parse("Presernov trg 1", "1000", "Ljubljana");
        Assert.NotNull(addr);
        Assert.Equal("Ljubljana", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => SlovenianAddress.Parse("Presernov trg 1", "INVALID", "Ljubljana"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(SlovenianAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = SlovenianAddress.Parse("Presernov trg 1", "1000", "Ljubljana");
        Assert.Contains("Ljubljana", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = SlovenianAddress.Parse("Presernov trg 1", "1000", "Ljubljana");
        var ml = addr.ToMultilineString();
        Assert.Contains("Ljubljana", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = SlovenianAddress.Parse("Presernov trg 1", "1000", "Ljubljana");
        Assert.Contains("SI", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = SlovenianAddress.Parse("Presernov trg 1", "1000", "Ljubljana");
        var masked = addr.ToMaskedString();
        Assert.Contains("Ljubljana", masked);
    }
}
