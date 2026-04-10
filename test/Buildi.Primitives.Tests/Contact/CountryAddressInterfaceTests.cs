using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class CountryAddressInterfaceTests
{
    [Fact]
    public void PolymorphicList_WorksWithDifferentCountries()
    {
        var addresses = new List<ICountryAddress>
        {
            SwedishAddress.Parse("Storgatan 12", "11453", "Stockholm"),
            PolishAddress.Parse("ul. Wiejska 4/6/8", "00902", "Warszawa"),
            DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag"),
        };

        Assert.Equal(3, addresses.Count);
        Assert.Equal("SE", addresses[0].Country.Alpha2Code);
        Assert.Equal("PL", addresses[1].Country.Alpha2Code);
        Assert.Equal("NL", addresses[2].Country.Alpha2Code);
    }

    [Fact]
    public void CountryZipCode_ReturnsTypedZipViaInterface()
    {
        ICountryAddress addr = DutchAddress.Parse("Binnenhof 1", "2513AA", "Den Haag");
        Assert.Equal("NL", addr.CountryZipCode.Country.Alpha2Code);
        Assert.Equal("2513 AA", addr.CountryZipCode.Formatted);
        Assert.NotNull(addr.CountryZipCode.ZipCode);
    }

    [Fact]
    public void Country_ReturnsCorrectCountry()
    {
        ICountryAddress addr = PolishAddress.Parse("ul. Wiejska 4/6/8", "00902", "Warszawa");
        Assert.Equal("PL", addr.Country.Alpha2Code);
        Assert.Equal("Poland", addr.Country.EnglishName);
    }

    [Fact]
    public void Address_ProvidesGenericFallback()
    {
        ICountryAddress addr = CzechAddress.Parse("Sněmovní 4", "11000", "Praha");
        Assert.NotNull(addr.Address);
        Assert.Equal("CZ", addr.Address.Country!.Alpha2Code);
        Assert.Contains("CZ", addr.Address.ToNormalizedString());
    }

    [Fact]
    public void CareOf_ExposedThroughInterface()
    {
        ICountryAddress addr = SwedishAddress.Parse("c/o Anna, Storgatan 12, 11453 Stockholm");
        Assert.Equal("Anna", addr.CareOf);
    }

    [Fact]
    public void Value_ReturnsNormalizedString()
    {
        ICountryAddress addr = GermanAddress.Parse("Platz der Republik 1", "10557", "Berlin");
        Assert.Contains("DE", addr.Value);
    }

    [Fact]
    public void Formatted_ReturnsDisplayString()
    {
        ICountryAddress addr = DanishAddress.Parse("Christiansborg Slotsplads 1", "1218", "København");
        Assert.Contains("København", addr.Formatted);
    }

    [Fact]
    public void ToMultilineString_ContainsNewline()
    {
        ICountryAddress addr = NorwegianAddress.Parse("Karl Johans gate 22", "0026", "Oslo");
        Assert.Contains(Environment.NewLine, addr.ToMultilineString());
    }

    [Fact]
    public void ZipCodeInterface_Country_ReturnsCorrectCountry()
    {
        ICountryAddressZipCode zip = SwedishAddressZipCode.Parse("11453");
        Assert.Equal("SE", zip.Country.Alpha2Code);
    }

    [Fact]
    public void ZipCodeInterface_PolymorphicList()
    {
        var zips = new List<ICountryAddressZipCode>
        {
            SwedishAddressZipCode.Parse("11453"),
            DutchAddressZipCode.Parse("1012AB"),
            CzechAddressZipCode.Parse("11000"),
        };

        Assert.Equal("114 53", zips[0].Formatted);
        Assert.Equal("1012 AB", zips[1].Formatted);
        Assert.Equal("110 00", zips[2].Formatted);
    }

    [Fact]
    public void MaskingThroughInterface_Address()
    {
        ICountryAddress addr = PolishAddress.Parse("ul. Wiejska 4/6/8", "00902", "Warszawa");
        var masked = addr.ToMaskedString();
        Assert.Contains("Warszawa", masked);
    }

    [Fact]
    public void MaskingThroughInterface_ZipCode()
    {
        ICountryAddressZipCode zip = SwedishAddressZipCode.Parse("11453");
        var masked = zip.ToMaskedString();
        Assert.Contains("*", masked);
    }
}
