using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Rue de la Loi 16, 1000 Bruxelles — European Council.
/// Grote Markt 1, 2000 Antwerpen — Antwerp City Hall (Stadhuis).
/// Place Saint-Lambert 1, 4000 Liege — Place Saint-Lambert.
/// </summary>
public class BelgianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(BelgianAddress.TryParse("Rue de la Loi 16", "1000", "Bruxelles", out var result));
        Assert.NotNull(result);
        Assert.Equal("Bruxelles", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("BE", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(BelgianAddress.TryParse("Rue de la Loi 16", "1000", "Bruxelles", "BE", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(BelgianAddress.TryParse("Grote Markt 1", "2000", "Antwerpen", out var result));
        Assert.NotNull(result);
        Assert.Equal("Antwerpen", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(BelgianAddress.TryParse("Place Saint-Lambert 1", "4000", "Liège", out var result));
        Assert.NotNull(result);
        Assert.Equal("Liège", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(BelgianAddress.TryParse("Rue de la Loi 16", "1000", "Bruxelles", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(BelgianAddress.TryParse("Rue de la Loi 16", "INVALID", "Bruxelles", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(BelgianAddress.TryParse("Rue de la Loi 16", "1000", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = BelgianAddress.Parse("Rue de la Loi 16", "1000", "Bruxelles");
        Assert.NotNull(addr);
        Assert.Equal("Bruxelles", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => BelgianAddress.Parse("Rue de la Loi 16", "INVALID", "Bruxelles"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(BelgianAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = BelgianAddress.Parse("Rue de la Loi 16", "1000", "Bruxelles");
        Assert.Contains("Bruxelles", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = BelgianAddress.Parse("Rue de la Loi 16", "1000", "Bruxelles");
        var ml = addr.ToMultilineString();
        Assert.Contains("Bruxelles", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = BelgianAddress.Parse("Rue de la Loi 16", "1000", "Bruxelles");
        Assert.Contains("BE", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = BelgianAddress.Parse("Rue de la Loi 16", "1000", "Bruxelles");
        var masked = addr.ToMaskedString();
        Assert.Contains("Bruxelles", masked);
    }
}
