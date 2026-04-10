using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Leoforos Lemesou 1, 1060 Nicosia — Limassol Avenue (Λεωφόρος Λεμεσού).
/// Plateia Eleftherias 1, 1011 Nicosia — Eleftheria Square (Πλατεία Ελευθερίας).
/// Leoforos Archiepiskopou Makariou III 1, 3020 Limassol — Makarios III Avenue.
/// </summary>
public class CypriotAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(CypriotAddress.TryParse("Leoforos Lemesou 1", "1060", "Nicosia", out var result));
        Assert.NotNull(result);
        Assert.Equal("Nicosia", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("CY", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(CypriotAddress.TryParse("Leoforos Lemesou 1", "1060", "Nicosia", "CY", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(CypriotAddress.TryParse("Plateia Eleftherias 1", "1011", "Nicosia", out var result));
        Assert.NotNull(result);
        Assert.Equal("Nicosia", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(CypriotAddress.TryParse("Leoforos Archiepiskopou Makariou III 1", "3020", "Limassol", out var result));
        Assert.NotNull(result);
        Assert.Equal("Limassol", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(CypriotAddress.TryParse("Leoforos Lemesou 1", "1060", "Nicosia", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(CypriotAddress.TryParse("Leoforos Lemesou 1", "INVALID", "Nicosia", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(CypriotAddress.TryParse("Leoforos Lemesou 1", "1060", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = CypriotAddress.Parse("Leoforos Lemesou 1", "1060", "Nicosia");
        Assert.NotNull(addr);
        Assert.Equal("Nicosia", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => CypriotAddress.Parse("Leoforos Lemesou 1", "INVALID", "Nicosia"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(CypriotAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = CypriotAddress.Parse("Leoforos Lemesou 1", "1060", "Nicosia");
        Assert.Contains("Nicosia", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = CypriotAddress.Parse("Leoforos Lemesou 1", "1060", "Nicosia");
        var ml = addr.ToMultilineString();
        Assert.Contains("Nicosia", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = CypriotAddress.Parse("Leoforos Lemesou 1", "1060", "Nicosia");
        Assert.Contains("CY", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = CypriotAddress.Parse("Leoforos Lemesou 1", "1060", "Nicosia");
        var masked = addr.ToMaskedString();
        Assert.Contains("Nicosia", masked);
    }
}
