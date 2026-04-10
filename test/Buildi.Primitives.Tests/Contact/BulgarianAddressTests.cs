using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// pl. Nezavisimost 2, 1000 Sofia — National Assembly of Bulgaria (Народно събрание).
/// pl. Aleksandar Battenberg 1, 1000 Sofia — National Art Gallery.
/// pl. Tsentralen 1, 4000 Plovdiv — Plovdiv Central Square.
/// </summary>
public class BulgarianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(BulgarianAddress.TryParse("pl. Nezavisimost 2", "1000", "Sofia", out var result));
        Assert.NotNull(result);
        Assert.Equal("Sofia", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("BG", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(BulgarianAddress.TryParse("pl. Nezavisimost 2", "1000", "Sofia", "BG", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(BulgarianAddress.TryParse("pl. Aleksandar Battenberg 1", "1000", "Sofia", out var result));
        Assert.NotNull(result);
        Assert.Equal("Sofia", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(BulgarianAddress.TryParse("pl. Tsentralen 1", "4000", "Plovdiv", out var result));
        Assert.NotNull(result);
        Assert.Equal("Plovdiv", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(BulgarianAddress.TryParse("pl. Nezavisimost 2", "1000", "Sofia", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(BulgarianAddress.TryParse("pl. Nezavisimost 2", "INVALID", "Sofia", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(BulgarianAddress.TryParse("pl. Nezavisimost 2", "1000", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = BulgarianAddress.Parse("pl. Nezavisimost 2", "1000", "Sofia");
        Assert.NotNull(addr);
        Assert.Equal("Sofia", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => BulgarianAddress.Parse("pl. Nezavisimost 2", "INVALID", "Sofia"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(BulgarianAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = BulgarianAddress.Parse("pl. Nezavisimost 2", "1000", "Sofia");
        var s = addr.ToString();
        Assert.Contains("Sofia", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = BulgarianAddress.Parse("pl. Nezavisimost 2", "1000", "Sofia");
        var ml = addr.ToMultilineString();
        Assert.Contains("Sofia", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = BulgarianAddress.Parse("pl. Nezavisimost 2", "1000", "Sofia");
        Assert.Contains("BG", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = BulgarianAddress.Parse("pl. Nezavisimost 2", "1000", "Sofia");
        var masked = addr.ToMaskedString();
        Assert.Contains("Sofia", masked);
    }
}
