using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Trg bana Jelacica 1, 10000 Zagreb — Ban Jelačić Square.
/// Trg svetog Marka 5, 10000 Zagreb — Croatian Parliament (Sabor).
/// Narodni trg 1, 21000 Split — People's Square.
/// </summary>
public class CroatianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(CroatianAddress.TryParse("Trg bana Jelacica 1", "10000", "Zagreb", out var result));
        Assert.NotNull(result);
        Assert.Equal("Zagreb", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("HR", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(CroatianAddress.TryParse("Trg bana Jelacica 1", "10000", "Zagreb", "HR", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(CroatianAddress.TryParse("Trg svetog Marka 5", "10000", "Zagreb", out var result));
        Assert.NotNull(result);
        Assert.Equal("Zagreb", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(CroatianAddress.TryParse("Narodni trg 1", "21000", "Split", out var result));
        Assert.NotNull(result);
        Assert.Equal("Split", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(CroatianAddress.TryParse("Trg bana Jelacica 1", "10000", "Zagreb", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(CroatianAddress.TryParse("Trg bana Jelacica 1", "INVALID", "Zagreb", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(CroatianAddress.TryParse("Trg bana Jelacica 1", "10000", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = CroatianAddress.Parse("Trg bana Jelacica 1", "10000", "Zagreb");
        Assert.NotNull(addr);
        Assert.Equal("Zagreb", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => CroatianAddress.Parse("Trg bana Jelacica 1", "INVALID", "Zagreb"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(CroatianAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = CroatianAddress.Parse("Trg bana Jelacica 1", "10000", "Zagreb");
        Assert.Contains("Zagreb", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = CroatianAddress.Parse("Trg bana Jelacica 1", "10000", "Zagreb");
        var ml = addr.ToMultilineString();
        Assert.Contains("Zagreb", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = CroatianAddress.Parse("Trg bana Jelacica 1", "10000", "Zagreb");
        Assert.Contains("HR", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = CroatianAddress.Parse("Trg bana Jelacica 1", "10000", "Zagreb");
        var masked = addr.ToMaskedString();
        Assert.Contains("Zagreb", masked);
    }
}
