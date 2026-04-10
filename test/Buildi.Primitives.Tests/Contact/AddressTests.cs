using System.Globalization;
using Buildi.Primitives.Contact;
using Buildi.Primitives.Geography;

namespace Buildi.Primitives.Tests.Contact;

[Collection("CultureSensitive")]
public class AddressTests : IDisposable
{
    public void Dispose() => PrimitivesDefaults.Reset();
    [Fact]
    public void TryParse_FreeTextWithCommas_ReturnsTypedComponents()
    {
        var ok = Address.TryParse("c/o Anna Svensson, Storgatan 12 lgh 1201, 114 53 Stockholm, Sweden", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Anna Svensson", address!.CareOf);
        Assert.Equal("1201", address.ApartmentNumber);
        Assert.Equal("Storgatan 12", address.Street.Street);
        Assert.Equal("11453", address.ZipCode!.Value);
        Assert.Equal("Stockholm", address.City!.Value);
        Assert.Equal("SE", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_FreeTextSingleLine_ReturnsTypedComponents()
    {
        var ok = Address.TryParse("Storgatan 12 114 53 Stockholm Sweden", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Storgatan 12", address!.Street.Street);
        Assert.Equal("11453", address.ZipCode!.Value);
        Assert.Equal("Stockholm", address.City!.Value);
        Assert.Equal("SE", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_FreeTextWithNewlines_ReturnsTypedComponents()
    {
        var ok = Address.TryParse("Box 123\n114 53 Stockholm\nSweden", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.True(address!.IsPostBox);
        Assert.Equal("123", address.PostBox);
        Assert.Equal("11453", address.ZipCode!.Value);
        Assert.Equal("Stockholm", address.City!.Value);
        Assert.Equal("SE", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_ReturnsTypedComponents()
    {
        var ok = Address.TryParse("Storgatan 12 lgh 1201", "114 53", "Stockholm", "SE", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("1201", address!.ApartmentNumber);
        Assert.Equal("11453", address.ZipCode!.Value);
        Assert.Equal("Stockholm", address.City!.Value);
        Assert.Equal("SE", address.Country!.Alpha2Code);
    }

    [Fact]
    public void Constructor_ToMultilineString_ReturnsExpectedValue()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var street = AddressStreet.Parse("c/o Anna Svensson, Storgatan 12 lgh 1201");
        var zipCode = AddressZipCode.Parse("114 53");
        var city = AddressCity.Parse("stockholm");
        var country = Country.Parse("Sweden");
        var address = new Address(street, zipCode, city, country);

        var expected = string.Join(
            Environment.NewLine,
            "c/o Anna Svensson",
            "Storgatan 12 lgh 1201",
            "114 53 Stockholm");

        Assert.Equal(expected, address.ToMultilineString());
    }

    [Fact]
    public void ToString_AndNormalize_ReturnExpectedValues()
    {
        PrimitivesDefaults.UICulture = CultureInfo.GetCultureInfo("sv-SE");

        var address = Address.Parse("c/o Anna Svensson, Storgatan 12 lgh 1201, 114 53 Stockholm, Sweden");

        Assert.Equal("c/o Anna Svensson, Storgatan 12 lgh 1201, 114 53 Stockholm", address.ToString());
        Assert.Equal("c/o Anna Svensson, Storgatan 12 lgh 1201, 11453, Stockholm, SE", address.ToNormalizedString());
        Assert.Equal("c/o Anna Svensson, Storgatan 12 lgh 1201, 11453, Stockholm, SE", Address.Normalize("c/o Anna Svensson, Storgatan 12 lgh 1201, 114 53 Stockholm, Sweden"));
    }

    [Fact]
    public void Format_ReturnsTrimmedOriginal_WhenInvalid()
    {
        Assert.Equal("not an address", Address.Format("  not an address  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(Address.Format(" "));
    }

    [Fact]
    public void TryParse_Components_GermanAddress()
    {
        var ok = Address.TryParse("Friedrichstraße 42", "10117", "Berlin", "DE", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Friedrichstraße 42", address!.Street.Street);
        Assert.Equal("10117", address.ZipCode!.Value);
        Assert.Equal("Berlin", address.City!.Value);
        Assert.Equal("DE", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_FrenchAddress()
    {
        var ok = Address.TryParse("Rue de la Paix 15", "75002", "Paris", "FR", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Rue de la Paix 15", address!.Street.Street);
        Assert.Equal("Paris", address.City!.Value);
        Assert.Equal("FR", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_DanishAddress()
    {
        var ok = Address.TryParse("Nørregade 12", "DK-1165", "København", "DK", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Nørregade 12", address!.Street.Street);
        Assert.Equal("København", address.City!.Value);
        Assert.Equal("DK", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_SpanishAddress()
    {
        var ok = Address.TryParse("Calle Mayor 25", "28013", "Madrid", "ES", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Calle Mayor 25", address!.Street.Street);
        Assert.Equal("Madrid", address.City!.Value);
        Assert.Equal("ES", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_ItalianAddress()
    {
        var ok = Address.TryParse("Via Roma 10", "00184", "Roma", "IT", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Via Roma 10", address!.Street.Street);
        Assert.Equal("Roma", address.City!.Value);
        Assert.Equal("IT", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_NorwegianAddress()
    {
        var ok = Address.TryParse("Karl Johans gate 22", "0159", "Oslo", "NO", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Oslo", address!.City!.Value);
        Assert.Equal("NO", address.Country!.Alpha2Code);
    }

    [Fact]
    public void TryParse_Components_FinnishAddress()
    {
        var ok = Address.TryParse("Mannerheimintie 5", "00100", "Helsinki", "FI", out var address);

        Assert.True(ok);
        Assert.NotNull(address);
        Assert.Equal("Helsinki", address!.City!.Value);
        Assert.Equal("FI", address.Country!.Alpha2Code);
    }
}
