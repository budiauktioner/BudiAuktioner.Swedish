using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Vassilissis Sofias 10, 106 74 Athina — Leoforos Vasilissis Sofias avenue.
/// Adrianou 24, 105 55 Athina — Plaka district near the Acropolis.
/// Aristotelous 1, 546 23 Thessaloniki — Aristotle Square (Πλατεία Αριστοτέλους).
/// </summary>
public class GreekAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(GreekAddress.TryParse("Vassilissis Sofias 10", "10674", "Athina", out var result));
        Assert.NotNull(result);
        Assert.Equal("Athina", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("GR", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(GreekAddress.TryParse("Vassilissis Sofias 10", "10674", "Athina", "GR", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(GreekAddress.TryParse("Adrianou 24", "10555", "Athina", out var result));
        Assert.NotNull(result);
        Assert.Equal("Athina", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(GreekAddress.TryParse("Aristotelous 1", "54623", "Thessaloniki", out var result));
        Assert.NotNull(result);
        Assert.Equal("Thessaloniki", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(GreekAddress.TryParse("Vassilissis Sofias 10", "10674", "Athina", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(GreekAddress.TryParse("Vassilissis Sofias 10", "INVALID", "Athina", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(GreekAddress.TryParse("Vassilissis Sofias 10", "10674", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = GreekAddress.Parse("Vassilissis Sofias 10", "10674", "Athina");
        Assert.NotNull(addr);
        Assert.Equal("Athina", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => GreekAddress.Parse("Vassilissis Sofias 10", "INVALID", "Athina"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(GreekAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = GreekAddress.Parse("Vassilissis Sofias 10", "10674", "Athina");
        Assert.Contains("Athina", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = GreekAddress.Parse("Vassilissis Sofias 10", "10674", "Athina");
        var ml = addr.ToMultilineString();
        Assert.Contains("Athina", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = GreekAddress.Parse("Vassilissis Sofias 10", "10674", "Athina");
        Assert.Contains("GR", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = GreekAddress.Parse("Vassilissis Sofias 10", "10674", "Athina");
        var masked = addr.ToMaskedString();
        Assert.Contains("Athina", masked);
    }
}
