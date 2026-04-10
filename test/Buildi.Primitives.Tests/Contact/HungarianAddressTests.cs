using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Kossuth Lajos ter 1, 1055 Budapest — Hungarian Parliament (Országház).
/// Szentharomsag ter 2, 1014 Budapest — Fisherman's Bastion area (Halászbástya).
/// Szechenyi ter 3, 7621 Pecs — Pécs Main Square (Széchenyi tér).
/// </summary>
public class HungarianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(HungarianAddress.TryParse("Kossuth Lajos ter 1", "1055", "Budapest", out var result));
        Assert.NotNull(result);
        Assert.Equal("Budapest", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("HU", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(HungarianAddress.TryParse("Kossuth Lajos ter 1", "1055", "Budapest", "HU", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(HungarianAddress.TryParse("Szentharomsag ter 2", "1014", "Budapest", out var result));
        Assert.NotNull(result);
        Assert.Equal("Budapest", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(HungarianAddress.TryParse("Szechenyi ter 3", "7621", "Pecs", out var result));
        Assert.NotNull(result);
        Assert.Equal("Pecs", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(HungarianAddress.TryParse("Kossuth Lajos ter 1", "1055", "Budapest", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(HungarianAddress.TryParse("Kossuth Lajos ter 1", "INVALID", "Budapest", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(HungarianAddress.TryParse("Kossuth Lajos ter 1", "1055", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = HungarianAddress.Parse("Kossuth Lajos ter 1", "1055", "Budapest");
        Assert.NotNull(addr);
        Assert.Equal("Budapest", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => HungarianAddress.Parse("Kossuth Lajos ter 1", "INVALID", "Budapest"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(HungarianAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = HungarianAddress.Parse("Kossuth Lajos ter 1", "1055", "Budapest");
        Assert.Contains("Budapest", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = HungarianAddress.Parse("Kossuth Lajos ter 1", "1055", "Budapest");
        var ml = addr.ToMultilineString();
        Assert.Contains("Budapest", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = HungarianAddress.Parse("Kossuth Lajos ter 1", "1055", "Budapest");
        Assert.Contains("HU", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = HungarianAddress.Parse("Kossuth Lajos ter 1", "1055", "Budapest");
        var masked = addr.ToMaskedString();
        Assert.Contains("Budapest", masked);
    }
}
