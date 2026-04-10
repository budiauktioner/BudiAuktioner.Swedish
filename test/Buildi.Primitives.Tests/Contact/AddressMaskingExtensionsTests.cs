using Buildi.Primitives.Contact;

namespace Buildi.Primitives.Tests.Contact;

public class AddressMaskingExtensionsTests
{
    [Fact]
    public void ZipCode_MasksDigitsPreservesSpace()
    {
        var zip = AddressZipCode.Parse("114 53");
        var masked = zip.ToMaskedString();
        Assert.Equal("*** **", masked);
    }

    [Fact]
    public void ZipCode_MaskedLengthMatchesFormatted()
    {
        var zip = AddressZipCode.Parse("11453");
        var masked = zip.ToMaskedString();
        Assert.Equal(zip.Formatted.Length, masked.Length);
    }

    [Fact]
    public void ZipCode_AllDigitsAreMasked()
    {
        var zip = AddressZipCode.Parse("114 53");
        var masked = zip.ToMaskedString();
        Assert.All(masked.Where(c => c != ' '), c => Assert.Equal('*', c));
    }

    [Fact]
    public void ZipCode_SpacePositionPreserved()
    {
        var zip = AddressZipCode.Parse("114 53");
        var masked = zip.ToMaskedString();
        Assert.Equal(' ', masked[3]);
    }

    [Fact]
    public void Address_MasksNumberAndZipKeepsStreetNameAndCity()
    {
        var address = Address.Parse("Storgatan 12, 114 53 Stockholm");
        var masked = address.ToMaskedString();
        Assert.Equal("Storgatan **, *** ** Stockholm", masked);
    }

    [Fact]
    public void Address_Box_MasksBoxNumberAndZip()
    {
        var address = Address.Parse("Box 123, 114 53 Stockholm");
        var masked = address.ToMaskedString();
        Assert.Equal("Box ***, *** ** Stockholm", masked);
    }

    [Fact]
    public void Address_WithApartment_MasksNumberAndApartment()
    {
        var address = Address.Parse("Storgatan 12 lgh 1201, 114 53 Stockholm");
        var masked = address.ToMaskedString();
        Assert.Contains("Storgatan **", masked);
        Assert.Contains("lgh ****", masked);
        Assert.Contains("Stockholm", masked);
    }

    [Fact]
    public void Address_WithCareOf_MasksCareOfName()
    {
        var address = Address.Parse("c/o Svensson, Storgatan 12, 114 53 Stockholm");
        var masked = address.ToMaskedString();
        Assert.StartsWith("c/o S", masked);
        Assert.Contains("Storgatan **", masked);
    }

    [Fact]
    public void Address_WithCountry_IncludesCountry()
    {
        var address = Address.Parse("Storgatan 12, 114 53 Stockholm, Sverige");
        var masked = address.ToMaskedString();
        Assert.EndsWith("Sverige", masked);
    }
}
