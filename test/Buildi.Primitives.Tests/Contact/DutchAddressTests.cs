using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Binnenhof 1, 2513 AA Den Haag — Binnenhof.
/// Museumstraat 1, 1071 XX Amsterdam — Rijksmuseum.
/// Coolsingel 40, 3011 AD Rotterdam — Rotterdam City Hall (Stadhuis).
/// </summary>
public class DutchAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(DutchAddress.TryParse("Binnenhof 1", "2513AA", "Den Haag", out var result));
        Assert.NotNull(result);
        Assert.Equal("Den Haag", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("NL", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(DutchAddress.TryParse("Binnenhof 1", "2513AA", "Den Haag", "NL", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(DutchAddress.TryParse("Museumstraat 1", "1071XX", "Amsterdam", out var result));
        Assert.NotNull(result);
        Assert.Equal("Amsterdam", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(DutchAddress.TryParse("Coolsingel 40", "3011AD", "Rotterdam", out var result));
        Assert.NotNull(result);
        Assert.Equal("Rotterdam", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(DutchAddress.TryParse("Binnenhof 1", "2513AA", "Den Haag", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(DutchAddress.TryParse("Binnenhof 1", "INVALID", "Den Haag", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(DutchAddress.TryParse("Binnenhof 1", "2513AA", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag");
        Assert.NotNull(addr);
        Assert.Equal("Den Haag", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => DutchAddress.Parse("Binnenhof 1", "INVALID", "Den Haag"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(DutchAddress.IsValid(null));
    }

    [Fact]
    public void ToString_ContainsCity()
    {
        var addr = DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag");
        var s = addr.ToString();
        Assert.Contains("Den Haag", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag");
        var ml = addr.ToMultilineString();
        Assert.Contains("Den Haag", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag");
        Assert.Contains("NL", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag");
        var masked = addr.ToMaskedString();
        Assert.Contains("Den Haag", masked);
    }
}
