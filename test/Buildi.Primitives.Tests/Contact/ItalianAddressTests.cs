using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Via del Corso 1, 00186 Roma — Via del Corso.
/// Piazza del Duomo 1, 20122 Milano — Milan Cathedral (Duomo di Milano).
/// Piazza San Marco 1, 30124 Venezia — St. Mark's Square (Piazza San Marco).
/// </summary>
public class ItalianAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(ItalianAddress.TryParse("Via del Corso 1", "00186", "Roma", out var result));
        Assert.NotNull(result);
        Assert.Equal("Roma", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("IT", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(ItalianAddress.TryParse("Via del Corso 1", "00186", "Roma", "IT", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(ItalianAddress.TryParse("Piazza del Duomo 1", "20122", "Milano", out var result));
        Assert.NotNull(result);
        Assert.Equal("Milano", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(ItalianAddress.TryParse("Piazza San Marco 1", "30124", "Venezia", out var result));
        Assert.NotNull(result);
        Assert.Equal("Venezia", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(ItalianAddress.TryParse("Via del Corso 1", "00186", "Roma", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(ItalianAddress.TryParse("Via del Corso 1", "INVALID", "Roma", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(ItalianAddress.TryParse("Via del Corso 1", "00186", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = ItalianAddress.Parse("Via del Corso 1", "00186", "Roma");
        Assert.NotNull(addr);
        Assert.Equal("Roma", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => ItalianAddress.Parse("Via del Corso 1", "INVALID", "Roma"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(ItalianAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = ItalianAddress.Parse("Via del Corso 1", "00186", "Roma");
        Assert.Contains("Roma", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = ItalianAddress.Parse("Via del Corso 1", "00186", "Roma");
        var ml = addr.ToMultilineString();
        Assert.Contains("Roma", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = ItalianAddress.Parse("Via del Corso 1", "00186", "Roma");
        Assert.Contains("IT", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = ItalianAddress.Parse("Via del Corso 1", "00186", "Roma");
        var masked = addr.ToMaskedString();
        Assert.Contains("Roma", masked);
    }
}
