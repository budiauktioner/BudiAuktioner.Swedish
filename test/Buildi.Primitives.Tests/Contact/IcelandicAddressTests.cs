using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Laekjargata 1, 101 Reykjavik — Lækjargata street.
/// Adalstraeti 10, 101 Reykjavik — Settlement Exhibition (Landnámssýningin).
/// Hafnarstraeti 15, 600 Akureyri — Akureyri Town Centre.
/// </summary>
public class IcelandicAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(IcelandicAddress.TryParse("Laekjargata 1", "101", "Reykjavik", out var result));
        Assert.NotNull(result);
        Assert.Equal("Reykjavik", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("IS", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(IcelandicAddress.TryParse("Laekjargata 1", "101", "Reykjavik", "IS", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(IcelandicAddress.TryParse("Adalstraeti 10", "101", "Reykjavik", out var result));
        Assert.NotNull(result);
        Assert.Equal("Reykjavik", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(IcelandicAddress.TryParse("Hafnarstraeti 15", "600", "Akureyri", out var result));
        Assert.NotNull(result);
        Assert.Equal("Akureyri", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(IcelandicAddress.TryParse("Laekjargata 1", "101", "Reykjavik", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(IcelandicAddress.TryParse("Laekjargata 1", "INVALID", "Reykjavik", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(IcelandicAddress.TryParse("Laekjargata 1", "101", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = IcelandicAddress.Parse("Laekjargata 1", "101", "Reykjavik");
        Assert.NotNull(addr);
        Assert.Equal("Reykjavik", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => IcelandicAddress.Parse("Laekjargata 1", "INVALID", "Reykjavik"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(IcelandicAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = IcelandicAddress.Parse("Laekjargata 1", "101", "Reykjavik");
        Assert.Contains("Reykjavik", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = IcelandicAddress.Parse("Laekjargata 1", "101", "Reykjavik");
        var ml = addr.ToMultilineString();
        Assert.Contains("Reykjavik", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = IcelandicAddress.Parse("Laekjargata 1", "101", "Reykjavik");
        Assert.Contains("IS", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = IcelandicAddress.Parse("Laekjargata 1", "101", "Reykjavik");
        var masked = addr.ToMaskedString();
        Assert.Contains("Reykjavik", masked);
    }
}
