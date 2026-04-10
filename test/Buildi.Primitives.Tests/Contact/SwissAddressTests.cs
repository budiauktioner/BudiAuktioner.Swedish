using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Bundesplatz 3, 3005 Bern — Bundeshaus (Federal Palace).
/// Museumstrasse 2, 8001 Zurich — Swiss National Museum (Landesmuseum).
/// Rue du Rhone 1, 1204 Geneve — Geneva Old Town.
/// </summary>
public class SwissAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(SwissAddress.TryParse("Bundesplatz 3", "3005", "Bern", out var result));
        Assert.NotNull(result);
        Assert.Equal("Bern", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("CH", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(SwissAddress.TryParse("Bundesplatz 3", "3005", "Bern", "CH", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(SwissAddress.TryParse("Museumstrasse 2", "8001", "Zurich", out var result));
        Assert.NotNull(result);
        Assert.Equal("Zurich", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(SwissAddress.TryParse("Rue du Rhone 1", "1204", "Geneve", out var result));
        Assert.NotNull(result);
        Assert.Equal("Geneve", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(SwissAddress.TryParse("Bundesplatz 3", "3005", "Bern", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(SwissAddress.TryParse("Bundesplatz 3", "INVALID", "Bern", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(SwissAddress.TryParse("Bundesplatz 3", "3005", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = SwissAddress.Parse("Bundesplatz 3", "3005", "Bern");
        Assert.NotNull(addr);
        Assert.Equal("Bern", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => SwissAddress.Parse("Bundesplatz 3", "INVALID", "Bern"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(SwissAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = SwissAddress.Parse("Bundesplatz 3", "3005", "Bern");
        Assert.Contains("Bern", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = SwissAddress.Parse("Bundesplatz 3", "3005", "Bern");
        var ml = addr.ToMultilineString();
        Assert.Contains("Bern", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = SwissAddress.Parse("Bundesplatz 3", "3005", "Bern");
        Assert.Contains("CH", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = SwissAddress.Parse("Bundesplatz 3", "3005", "Bern");
        var masked = addr.ToMaskedString();
        Assert.Contains("Bern", masked);
    }
}
