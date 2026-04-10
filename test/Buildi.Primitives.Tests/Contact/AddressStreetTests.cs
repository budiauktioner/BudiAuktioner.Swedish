using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class AddressStreetTests
{
    [Theory]
    [InlineData("Storgatan 12", "Storgatan 12")]
    [InlineData("STORGATAN 12", "Storgatan 12")]
    [InlineData("kungsgatan 5", "Kungsgatan 5")]
    [InlineData("  Storgatan  12  ", "Storgatan 12")]
    [InlineData("Friedrichstraße 42", "Friedrichstraße 42")]
    [InlineData("Rue de la Paix 15", "Rue de la Paix 15")]
    [InlineData("Via Roma 10", "Via Roma 10")]
    [InlineData("Calle Mayor 25", "Calle Mayor 25")]
    [InlineData("Nørregade 12", "Nørregade 12")]
    [InlineData("Kääriku tee 3", "Kääriku tee 3")]
    public void TryParse_ValidInput_Succeeds(string input, string expected)
    {
        Assert.True(AddressStreet.TryParse(input, out var result));
        Assert.Equal(expected, result!.Street);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_InvalidInput_Fails(string? input)
    {
        Assert.False(AddressStreet.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Storgatan 12", "Storgatan", "12")]
    [InlineData("Storgatan 12A", "Storgatan", "12A")]
    [InlineData("Linta Gårdsväg 5A", "Linta Gårdsväg", "5A")]
    [InlineData("Vasagatan 10", "Vasagatan", "10")]
    [InlineData("Hornsgatan 20", "Hornsgatan", "20")]
    [InlineData("Friedrichstraße 42", "Friedrichstraße", "42")]
    [InlineData("Via Roma 10", "Via Roma", "10")]
    [InlineData("Calle Mayor 25", "Calle Mayor", "25")]
    [InlineData("Nørregade 12", "Nørregade", "12")]
    public void TryParse_ExtractsStreetNameAndNumber(string input, string expectedName, string expectedNumber)
    {
        Assert.True(AddressStreet.TryParse(input, out var result));
        Assert.Equal(expectedName, result!.StreetName);
        Assert.Equal(expectedNumber, result.StreetNumber);
    }

    [Fact]
    public void TryParse_PostBox_HasNullStreetNameAndNumber()
    {
        Assert.True(AddressStreet.TryParse("Box 123", out var result));
        Assert.Null(result!.StreetName);
        Assert.Null(result.StreetNumber);
    }

    [Fact]
    public void TryParse_ExtractsCareOf()
    {
        Assert.True(AddressStreet.TryParse("c/o Anna Svensson, Storgatan 12", out var result));
        Assert.Equal("Anna Svensson", result!.CareOf);
        Assert.Equal("Storgatan 12", result.Street);
    }

    [Fact]
    public void TryParse_CareOfOnly_PersonName()
    {
        Assert.True(AddressStreet.TryParse("C/o Erik Lindgren", out var result));
        Assert.Equal("Erik Lindgren", result!.CareOf);
        Assert.Equal("c/o Erik Lindgren", result.Street);
        Assert.Null(result.StreetName);
        Assert.Null(result.StreetNumber);
        Assert.Null(result.PostBox);
    }

    [Fact]
    public void TryParse_CareOfOnly_CompanyName()
    {
        Assert.True(AddressStreet.TryParse("C/O Nordisk Fastighets AB", out var result));
        Assert.Equal("Nordisk Fastighets AB", result!.CareOf);
        Assert.Equal("c/o Nordisk Fastighets AB", result.Street);
    }

    [Fact]
    public void TryParse_CareOfOnly_Lowercase()
    {
        Assert.True(AddressStreet.TryParse("c/o Pettersson", out var result));
        Assert.Equal("Pettersson", result!.CareOf);
        Assert.Equal("c/o Pettersson", result.Street);
    }

    [Fact]
    public void TryParse_CareOfAlone_Invalid()
    {
        Assert.False(AddressStreet.TryParse("c/o", out _));
        Assert.False(AddressStreet.TryParse("c/o ", out _));
    }

    [Fact]
    public void TryParse_ExtractsApartmentNumber()
    {
        Assert.True(AddressStreet.TryParse("Storgatan 12 lgh 1201", out var result));
        Assert.Equal("1201", result!.ApartmentNumber);
        Assert.Equal("Storgatan 12", result.Street);
    }

    [Fact]
    public void TryParse_ExtractsPostBox()
    {
        Assert.True(AddressStreet.TryParse("Box 123", out var result));
        Assert.Equal("123", result!.PostBox);
        Assert.True(result.IsPostBox);
        Assert.Equal("Box 123", result.Street);
    }

    [Fact]
    public void TryParse_WithCityAndZip_StripsTrailing()
    {
        Assert.True(AddressStreet.TryParse("Storgatan 12 114 53 Stockholm", "Stockholm", "114 53", out var result));
        Assert.Equal("Storgatan 12", result!.Street);
    }

    [Theory]
    [InlineData("Storgatan 12A", true)]
    [InlineData("Friedrichstraße 42", true)]
    [InlineData("Rue de la Paix 15", true)]
    [InlineData("Via Roma 10", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, AddressStreet.IsValid(input));
    }

    [Theory]
    [InlineData("STORGATAN 12", "Storgatan 12")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, AddressStreet.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !AddressStreet.IsValid(input)));
    }

    [Theory]
    [InlineData("STORGATAN 12", "Storgatan 12")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, AddressStreet.Normalize(input));
    }

    [Fact]
    public void Parse_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => AddressStreet.Parse(""));
    }

    [Fact]
    public void ToString_ReturnsStreet()
    {
        var street = AddressStreet.Parse("Storgatan 12");
        Assert.Equal("Storgatan 12", street.ToString());
        Assert.Equal("Storgatan 12", street.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = AddressStreet.Parse("STORGATAN 12");
        var b = AddressStreet.Parse("Storgatan 12");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = AddressStreet.Parse("Storgatan 12");
        var b = AddressStreet.Parse("Kungsgatan 5");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = AddressStreet.Parse("Storgatan 12");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = AddressStreet.Parse("Calle Mayor 25");
        var b = AddressStreet.Parse("Storgatan 12");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = AddressStreet.Parse("Via Roma 10");
        Assert.Equal(1, a.CompareTo(null));
    }
}
