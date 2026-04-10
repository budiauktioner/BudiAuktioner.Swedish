using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Calea Victoriei 141, 010071 Bucuresti — Calea Victoriei.
/// Str. Stavropoleos 4, 030084 Bucuresti — Stavropoleos Monastery (Mănăstirea Stavropoleos).
/// Piata Unirii 1, 300085 Timisoara — Timișoara Union Square (Piața Unirii).
/// </summary>
public class RomanianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(RomanianAddress.TryParse("Calea Victoriei 141", "010071", "București", out var result));
        Assert.NotNull(result);
        Assert.Equal("București", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("RO", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(RomanianAddress.TryParse("Calea Victoriei 141", "010071", "București", "RO", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(RomanianAddress.TryParse("Str. Stavropoleos 4", "030084", "București", out var result));
        Assert.NotNull(result);
        Assert.Equal("București", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(RomanianAddress.TryParse("Piata Unirii 1", "300085", "Timisoara", out var result));
        Assert.NotNull(result);
        Assert.Equal("Timisoara", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails()
    {
        Assert.False(RomanianAddress.TryParse("Calea Victoriei 141", "010071", "București", "US", out _));
    }

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails()
    {
        Assert.False(RomanianAddress.TryParse("Calea Victoriei 141", "INVALID", "București", out _));
    }

    [Fact]
    public void TryParse_Components_MissingCity_Fails()
    {
        Assert.False(RomanianAddress.TryParse("Calea Victoriei 141", "010071", null, out _));
    }

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = RomanianAddress.Parse("Calea Victoriei 141", "010071", "București");
        Assert.NotNull(addr);
        Assert.Equal("București", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws()
    {
        Assert.Throws<ArgumentException>(() => RomanianAddress.Parse("Calea Victoriei 141", "INVALID", "București"));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(RomanianAddress.IsValid(null));
    }

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = RomanianAddress.Parse("Calea Victoriei 141", "010071", "București");
        var s = addr.ToString();
        Assert.Contains("București", s);
        Assert.DoesNotContain("RO", s);
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = RomanianAddress.Parse("Calea Victoriei 141", "010071", "București");
        var ml = addr.ToMultilineString();
        Assert.Contains("București", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = RomanianAddress.Parse("Calea Victoriei 141", "010071", "București");
        Assert.Contains("RO", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = RomanianAddress.Parse("Calea Victoriei 141", "010071", "București");
        var masked = addr.ToMaskedString();
        Assert.Contains("București", masked);
    }
}
