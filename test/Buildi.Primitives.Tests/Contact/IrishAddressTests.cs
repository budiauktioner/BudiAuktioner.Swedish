using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Kildare Street, D02 XR20 Dublin — Leinster House, seat of the Oireachtas (Irish Parliament).
/// College Green 1, D02 VR66 Dublin — Trinity College Dublin.
/// Eyre Square 1, H91 K2E0 Galway — Eyre Square.
/// </summary>
public class IrishAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(IrishAddress.TryParse("Kildare Street", "D02 XR20", "Dublin", out var result));
        Assert.NotNull(result);
        Assert.Equal("Dublin", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("IE", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(IrishAddress.TryParse("Kildare Street", "D02 XR20", "Dublin", "IE", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(IrishAddress.TryParse("College Green 1", "D02 VR66", "Dublin", out var result));
        Assert.NotNull(result);
        Assert.Equal("Dublin", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(IrishAddress.TryParse("Eyre Square 1", "H91 K2E0", "Galway", out var result));
        Assert.NotNull(result);
        Assert.Equal("Galway", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(IrishAddress.TryParse("Kildare Street", "D02 XR20", "Dublin", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(IrishAddress.TryParse("Kildare Street", "INVALID", "Dublin", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(IrishAddress.TryParse("Kildare Street", "D02 XR20", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = IrishAddress.Parse("Kildare Street", "D02 XR20", "Dublin");
        Assert.NotNull(addr);
        Assert.Equal("Dublin", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => IrishAddress.Parse("Kildare Street", "INVALID", "Dublin"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(IrishAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = IrishAddress.Parse("Kildare Street", "D02 XR20", "Dublin");
        Assert.Contains("Dublin", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = IrishAddress.Parse("Kildare Street", "D02 XR20", "Dublin");
        var ml = addr.ToMultilineString();
        Assert.Contains("Dublin", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = IrishAddress.Parse("Kildare Street", "D02 XR20", "Dublin");
        Assert.Contains("IE", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = IrishAddress.Parse("Kildare Street", "D02 XR20", "Dublin");
        var masked = addr.ToMaskedString();
        Assert.Contains("Dublin", masked);
    }
}
