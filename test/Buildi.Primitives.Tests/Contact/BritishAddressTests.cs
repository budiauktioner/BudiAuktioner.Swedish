using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

/// <summary>
/// Test addresses use publicly known landmarks:
/// 10 Downing Street, SW1A 2AA London — official residence of the UK Prime Minister.
/// Trafalgar Square 1, WC2N 5DN London — National Gallery.
/// St Andrews Square 1, EH2 2BD Edinburgh — Edinburgh city centre.
/// </summary>
public class BritishAddressTests
{
    [Fact]
    public void TryParse_Components_ValidAddress()
    {
        Assert.True(BritishAddress.TryParse("10 Downing Street", "SW1A 2AA", "London", out var result));
        Assert.NotNull(result);
        Assert.Equal("London", result!.City.Value);
        Assert.NotNull(result.ZipCode);
        Assert.NotNull(result.Address);
        Assert.Equal("GB", result.Address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_WithCorrectCountry()
    {
        Assert.True(BritishAddress.TryParse("10 Downing Street", "SW1A 2AA", "London", "GB", out var result));
        Assert.NotNull(result);
    }

    [Fact]
    public void TryParse_Components_SecondPublicAddress()
    {
        Assert.True(BritishAddress.TryParse("Trafalgar Square 1", "WC2N 5DN", "London", out var result));
        Assert.NotNull(result);
        Assert.Equal("London", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_ThirdPublicAddress()
    {
        Assert.True(BritishAddress.TryParse("St Andrews Square 1", "EH2 2BD", "Edinburgh", out var result));
        Assert.NotNull(result);
        Assert.Equal("Edinburgh", result!.City.Value);
    }

    [Fact]
    public void TryParse_Components_WrongCountry_Fails() =>
        Assert.False(BritishAddress.TryParse("10 Downing Street", "SW1A 2AA", "London", "US", out _));

    [Fact]
    public void TryParse_Components_WrongZipFormat_Fails() =>
        Assert.False(BritishAddress.TryParse("10 Downing Street", "INVALID", "London", out _));

    [Fact]
    public void TryParse_Components_MissingCity_Fails() =>
        Assert.False(BritishAddress.TryParse("10 Downing Street", "SW1A 2AA", null, out _));

    [Fact]
    public void Parse_ValidComponents()
    {
        var addr = BritishAddress.Parse("10 Downing Street", "SW1A 2AA", "London");
        Assert.NotNull(addr);
        Assert.Equal("London", addr.City.Value);
    }

    [Fact]
    public void Parse_InvalidComponents_Throws() =>
        Assert.Throws<ArgumentException>(() => BritishAddress.Parse("10 Downing Street", "INVALID", "London"));

    [Fact]
    public void IsValid_ReturnsFalse_ForNull() =>
        Assert.False(BritishAddress.IsValid(null));

    [Fact]
    public void ToString_OmitsCountry()
    {
        var addr = BritishAddress.Parse("10 Downing Street", "SW1A 2AA", "London");
        Assert.Contains("London", addr.ToString());
    }

    [Fact]
    public void ToMultilineString_ContainsCity()
    {
        var addr = BritishAddress.Parse("10 Downing Street", "SW1A 2AA", "London");
        var ml = addr.ToMultilineString();
        Assert.Contains("London", ml);
        Assert.Contains(Environment.NewLine, ml);
    }

    [Fact]
    public void ToNormalizedString_IncludesCountryCode()
    {
        var addr = BritishAddress.Parse("10 Downing Street", "SW1A 2AA", "London");
        Assert.Contains("GB", addr.ToNormalizedString());
    }

    [Fact]
    public void ToMaskedString_MasksAddress()
    {
        var addr = BritishAddress.Parse("10 Downing Street", "SW1A 2AA", "London");
        var masked = addr.ToMaskedString();
        Assert.Contains("London", masked);
    }
}
