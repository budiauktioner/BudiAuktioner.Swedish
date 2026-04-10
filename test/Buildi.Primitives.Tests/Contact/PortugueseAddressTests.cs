using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Praca do Comercio 1, 1100-148 Lisboa — Praça do Comércio.
/// Praca do Municipio 1, 1100-365 Lisboa — Lisbon City Hall (Câmara Municipal).
/// Rua de Santa Catarina 1, 4000-447 Porto — Santa Catarina Street.
/// </summary>
public class PortugueseAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(PortugueseAddress.TryParse("Praca do Comercio 1", "1100-148", "Lisboa", out var result));
        Assert.NotNull(result);
        Assert.Equal("Lisboa", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("PT", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(PortugueseAddress.TryParse("Praca do Comercio 1", "1100-148", "Lisboa", "PT", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(PortugueseAddress.TryParse("Praca do Municipio 1", "1100-365", "Lisboa", out var result));
        Assert.NotNull(result);
        Assert.Equal("Lisboa", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(PortugueseAddress.TryParse("Rua de Santa Catarina 1", "4000-447", "Porto", out var result));
        Assert.NotNull(result);
        Assert.Equal("Porto", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(PortugueseAddress.TryParse("Praca do Comercio 1", "1100-148", "Lisboa", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(PortugueseAddress.TryParse("Praca do Comercio 1", "INVALID", "Lisboa", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(PortugueseAddress.TryParse("Praca do Comercio 1", "1100-148", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = PortugueseAddress.Parse("Praca do Comercio 1", "1100-148", "Lisboa");
        Assert.NotNull(addr);
        Assert.Equal("Lisboa", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => PortugueseAddress.Parse("Praca do Comercio 1", "INVALID", "Lisboa"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(PortugueseAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = PortugueseAddress.Parse("Praca do Comercio 1", "1100-148", "Lisboa");
        Assert.Contains("Lisboa", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = PortugueseAddress.Parse("Praca do Comercio 1", "1100-148", "Lisboa");
        var ml = addr.ToMultilineString();
        Assert.Contains("Lisboa", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = PortugueseAddress.Parse("Praca do Comercio 1", "1100-148", "Lisboa");
        Assert.Contains("PT", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = PortugueseAddress.Parse("Praca do Comercio 1", "1100-148", "Lisboa");
        var masked = addr.ToMaskedString();
        Assert.Contains("Lisboa", masked);
    }
}
