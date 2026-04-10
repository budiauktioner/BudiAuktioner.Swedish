using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Calle de Bailen 2, 28071 Madrid — Royal Palace (Palacio Real).
/// Paseo del Prado 36, 28014 Madrid — Museo del Prado.
/// Placa de Catalunya 1, 08002 Barcelona — Plaça de Catalunya.
/// </summary>
public class SpanishAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(SpanishAddress.TryParse("Calle de Bailén 2", "28071", "Madrid", out var result));
        Assert.NotNull(result);
        Assert.Equal("Madrid", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("ES", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(SpanishAddress.TryParse("Calle de Bailén 2", "28071", "Madrid", "ES", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(SpanishAddress.TryParse("Paseo del Prado 36", "28014", "Madrid", out var result));
        Assert.NotNull(result);
        Assert.Equal("Madrid", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(SpanishAddress.TryParse("Placa de Catalunya 1", "08002", "Barcelona", out var result));
        Assert.NotNull(result);
        Assert.Equal("Barcelona", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(SpanishAddress.TryParse("Calle de Bailén 2", "28071", "Madrid", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(SpanishAddress.TryParse("Calle de Bailén 2", "INVALID", "Madrid", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(SpanishAddress.TryParse("Calle de Bailén 2", "28071", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = SpanishAddress.Parse("Calle de Bailén 2", "28071", "Madrid");
        Assert.NotNull(addr);
        Assert.Equal("Madrid", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => SpanishAddress.Parse("Calle de Bailén 2", "INVALID", "Madrid"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(SpanishAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = SpanishAddress.Parse("Calle de Bailén 2", "28071", "Madrid");
        var s = addr.ToString();
        Assert.Contains("Madrid", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = SpanishAddress.Parse("Calle de Bailén 2", "28071", "Madrid");
        var ml = addr.ToMultilineString();
        Assert.Contains("Madrid", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = SpanishAddress.Parse("Calle de Bailén 2", "28071", "Madrid");
        Assert.Contains("ES", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = SpanishAddress.Parse("Calle de Bailén 2", "28071", "Madrid");
        var masked = addr.ToMaskedString();
        Assert.Contains("Madrid", masked);
    }
}
