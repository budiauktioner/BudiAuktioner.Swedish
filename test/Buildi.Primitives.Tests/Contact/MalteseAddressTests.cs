using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// Pjazza San Gorg, VLT 1190 Valletta — Grandmaster's Palace.
/// Triq il-Merkanti 1, VLT 1171 Valletta — Merchant Street.
/// Triq il-Kbira 1, MST 1150 Mosta — Mosta Dome (Rotunda of Mosta).
/// </summary>
public class MalteseAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(MalteseAddress.TryParse("Pjazza San Gorg", "VLT 1190", "Valletta", out var result));
        Assert.NotNull(result);
        Assert.Equal("Valletta", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("MT", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(MalteseAddress.TryParse("Pjazza San Gorg", "VLT 1190", "Valletta", "MT", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(MalteseAddress.TryParse("Triq il-Merkanti 1", "VLT 1171", "Valletta", out var result));
        Assert.NotNull(result);
        Assert.Equal("Valletta", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(MalteseAddress.TryParse("Triq il-Kbira 1", "MST 1150", "Mosta", out var result));
        Assert.NotNull(result);
        Assert.Equal("Mosta", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(MalteseAddress.TryParse("Pjazza San Gorg", "VLT 1190", "Valletta", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(MalteseAddress.TryParse("Pjazza San Gorg", "INVALID", "Valletta", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(MalteseAddress.TryParse("Pjazza San Gorg", "VLT 1190", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = MalteseAddress.Parse("Pjazza San Gorg", "VLT 1190", "Valletta");
        Assert.NotNull(addr);
        Assert.Equal("Valletta", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => MalteseAddress.Parse("Pjazza San Gorg", "INVALID", "Valletta"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(MalteseAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = MalteseAddress.Parse("Pjazza San Gorg", "VLT 1190", "Valletta");
        Assert.Contains("Valletta", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = MalteseAddress.Parse("Pjazza San Gorg", "VLT 1190", "Valletta");
        var ml = addr.ToMultilineString();
        Assert.Contains("Valletta", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = MalteseAddress.Parse("Pjazza San Gorg", "VLT 1190", "Valletta");
        Assert.Contains("MT", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = MalteseAddress.Parse("Pjazza San Gorg", "VLT 1190", "Valletta");
        var masked = addr.ToMaskedString();
        Assert.Contains("Valletta", masked);
    }
}
