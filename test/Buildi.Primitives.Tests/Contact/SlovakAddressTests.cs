using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Primacialnenamestie 1, 811 01 Bratislava — Primaciálne námestie (Primatial Square).
/// Hviezdoslavovo namestie 1, 811 02 Bratislava — Slovak National Theatre (SND).
/// Hlavna 1, 040 01 Kosice — Košice Main Street (Hlavná ulica).
/// </summary>
public class SlovakAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(SlovakAddress.TryParse("Primacialnenamestie 1", "81101", "Bratislava", out var result));
        Assert.NotNull(result);
        Assert.Equal("Bratislava", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("SK", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(SlovakAddress.TryParse("Primacialnenamestie 1", "81101", "Bratislava", "SK", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(SlovakAddress.TryParse("Hviezdoslavovo namestie 1", "81102", "Bratislava", out var result));
        Assert.NotNull(result);
        Assert.Equal("Bratislava", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(SlovakAddress.TryParse("Hlavna 1", "04001", "Kosice", out var result));
        Assert.NotNull(result);
        Assert.Equal("Kosice", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(SlovakAddress.TryParse("Primacialnenamestie 1", "81101", "Bratislava", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(SlovakAddress.TryParse("Primacialnenamestie 1", "INVALID", "Bratislava", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(SlovakAddress.TryParse("Primacialnenamestie 1", "81101", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = SlovakAddress.Parse("Primacialnenamestie 1", "81101", "Bratislava");
        Assert.NotNull(addr);
        Assert.Equal("Bratislava", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => SlovakAddress.Parse("Primacialnenamestie 1", "INVALID", "Bratislava"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(SlovakAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = SlovakAddress.Parse("Primacialnenamestie 1", "81101", "Bratislava");
        Assert.Contains("Bratislava", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = SlovakAddress.Parse("Primacialnenamestie 1", "81101", "Bratislava");
        var ml = addr.ToMultilineString();
        Assert.Contains("Bratislava", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = SlovakAddress.Parse("Primacialnenamestie 1", "81101", "Bratislava");
        Assert.Contains("SK", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = SlovakAddress.Parse("Primacialnenamestie 1", "81101", "Bratislava");
        var masked = addr.ToMaskedString();
        Assert.Contains("Bratislava", masked);
    }
}
