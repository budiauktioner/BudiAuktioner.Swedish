using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Städtle 38, 9490 Vaduz — Liechtenstein National Museum (Liechtensteinisches Landesmuseum).
/// Peter-Kaiser-Platz 1, 9490 Vaduz — Government Building (Regierungsgebäude).
/// Im Bretscha 22, 9494 Schaan — Schaan town centre.
/// </summary>
public class LiechtensteinAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(LiechtensteinAddress.TryParse("Staedtle 38", "9490", "Vaduz", out var result));
        Assert.NotNull(result);
        Assert.Equal("Vaduz", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("LI", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(LiechtensteinAddress.TryParse("Staedtle 38", "9490", "Vaduz", "LI", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(LiechtensteinAddress.TryParse("Peter-Kaiser-Platz 1", "9490", "Vaduz", out var result));
        Assert.NotNull(result);
        Assert.Equal("Vaduz", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(LiechtensteinAddress.TryParse("Im Bretscha 22", "9494", "Schaan", out var result));
        Assert.NotNull(result);
        Assert.Equal("Schaan", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(LiechtensteinAddress.TryParse("Staedtle 38", "9490", "Vaduz", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(LiechtensteinAddress.TryParse("Staedtle 38", "INVALID", "Vaduz", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(LiechtensteinAddress.TryParse("Staedtle 38", "9490", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = LiechtensteinAddress.Parse("Staedtle 38", "9490", "Vaduz");
        Assert.NotNull(addr);
        Assert.Equal("Vaduz", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => LiechtensteinAddress.Parse("Staedtle 38", "INVALID", "Vaduz"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(LiechtensteinAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = LiechtensteinAddress.Parse("Staedtle 38", "9490", "Vaduz");
        Assert.Contains("Vaduz", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = LiechtensteinAddress.Parse("Staedtle 38", "9490", "Vaduz");
        var ml = addr.ToMultilineString();
        Assert.Contains("Vaduz", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = LiechtensteinAddress.Parse("Staedtle 38", "9490", "Vaduz");
        Assert.Contains("LI", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = LiechtensteinAddress.Parse("Staedtle 38", "9490", "Vaduz");
        var masked = addr.ToMaskedString();
        Assert.Contains("Vaduz", masked);
    }
}
