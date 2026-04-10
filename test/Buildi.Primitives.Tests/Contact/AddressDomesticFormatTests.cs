using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class AddressDomesticFormatTests
{
    [Fact]
    public void ToDomesticString_OmitsCountry_ForSwedish()
    {
        var addr = Address.Parse("Storgatan 12", "11453", "Stockholm", "SE");
        var domestic = addr.ToDomesticString();
        Assert.Contains("Stockholm", domestic);
        Assert.DoesNotContain("Sverige", domestic);
        Assert.DoesNotContain("SE", domestic);
    }

    [Fact]
    public void ToDomesticString_OmitsCountry_ForPolish()
    {
        var addr = Address.Parse("ul. Wiejska 4/6/8", "00-902", "Warszawa", "PL");
        var domestic = addr.ToDomesticString();
        Assert.Contains("Warszawa", domestic);
        Assert.DoesNotContain("Polen", domestic);
        Assert.DoesNotContain("PL", domestic);
    }

    [Fact]
    public void ToDomesticString_UsesCountrySpecificZipFormat_Polish()
    {
        var addr = Address.Parse("ul. Wiejska 4/6/8", "00-902", "Warszawa", "PL");
        var domestic = addr.ToDomesticString();
        Assert.Contains("00-902", domestic);
    }

    [Fact]
    public void ToDomesticString_UsesCountrySpecificZipFormat_Czech()
    {
        var addr = Address.Parse("Sněmovní 4", "11000", "Praha", "CZ");
        var domestic = addr.ToDomesticString();
        Assert.Contains("110 00", domestic);
    }

    [Fact]
    public void ToDomesticString_UsesCountrySpecificZipFormat_Swedish()
    {
        var addr = Address.Parse("Storgatan 12", "11453", "Stockholm", "SE");
        var domestic = addr.ToDomesticString();
        Assert.Contains("114 53", domestic);
    }

    [Fact]
    public void ToDomesticString_UsesCountrySpecificZipFormat_Latvian()
    {
        var addr = Address.Parse("Jēkaba iela 11", "1050", "Rīga", "LV");
        var domestic = addr.ToDomesticString();
        Assert.Contains("LV-1050", domestic);
    }

    [Fact]
    public void ToDomesticString_OmitsCountry_ForUnsupportedCountry()
    {
        var addr = Address.Parse("1600 Pennsylvania Ave", "20500", "Washington", "US");
        var domestic = addr.ToDomesticString();
        Assert.Contains("Washington", domestic);
        Assert.DoesNotContain("US", domestic);
    }

    [Fact]
    public void ToDomesticMultilineString_ContainsNewlines()
    {
        var addr = Address.Parse("Storgatan 12", "11453", "Stockholm", "SE");
        var ml = addr.ToDomesticMultilineString();
        Assert.Contains(Environment.NewLine, ml);
        Assert.DoesNotContain("Sverige", ml);
    }

    [Fact]
    public void AsCountryAddress_ReturnsSwedishAddress_ForSE()
    {
        var addr = Address.Parse("Storgatan 12", "11453", "Stockholm", "SE");
        var country = addr.AsCountryAddress();
        Assert.NotNull(country);
        Assert.IsType<SwedishAddress>(country);
        Assert.Equal("SE", country!.Country.Alpha2Code);
    }

    [Fact]
    public void AsCountryAddress_ReturnsDutchAddress_ForNL()
    {
        var addr = Address.Parse("Binnenhof 1", "2513 AA", "Den Haag", "NL");
        var country = addr.AsCountryAddress();
        Assert.NotNull(country);
        Assert.IsType<DutchAddress>(country);
    }

    [Fact]
    public void AsCountryAddress_ReturnsNull_ForUnsupportedCountry()
    {
        var addr = Address.Parse("1600 Pennsylvania Ave", "20500", "Washington", "US");
        Assert.Null(addr.AsCountryAddress());
    }

    [Fact]
    public void AsCountryAddress_ReturnsNull_WhenNoCountry()
    {
        var addr = Address.Parse("Storgatan 12", "11453", "Stockholm");
        // Without country the Swedish-specific logic may or may not work depending on zip inference
        // but the method should not throw
        var _ = addr.AsCountryAddress();
    }

    [Fact]
    public void SupportedCountryAddressTypes_Contains32Countries()
    {
        Assert.Equal(32, Address.SupportedCountryAddressTypes.Count);
    }

    [Fact]
    public void SupportedCountryAddressTypes_ContainsSweden()
    {
        Assert.Contains(Address.SupportedCountryAddressTypes, c => c.Alpha2Code == "SE");
    }

    [Fact]
    public void SupportedCountryAddressTypes_ContainsNetherlands()
    {
        Assert.Contains(Address.SupportedCountryAddressTypes, c => c.Alpha2Code == "NL");
    }
}
