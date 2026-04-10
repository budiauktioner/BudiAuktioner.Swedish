using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ProductMaskingExtensionsTests
{
    [Fact]
    public void Gtin13_ShowsGs1PrefixAndMasksRest()
    {
        var gtin = Gtin13.Parse("5901234123457");
        var masked = gtin.ToMaskedString();
        Assert.Equal("590**********", masked);
    }

    [Fact]
    public void Gtin13_MaskedLengthEquals13()
    {
        var gtin = Gtin13.Parse("5901234123457");
        var masked = gtin.ToMaskedString();
        Assert.Equal(13, masked.Length);
    }

    [Fact]
    public void Gtin13_PreservesGs1Prefix()
    {
        var gtin = Gtin13.Parse("5901234123457");
        var masked = gtin.ToMaskedString();
        Assert.StartsWith(gtin.Gs1Prefix, masked);
    }

    [Fact]
    public void Gtin8_MasksAllDigits()
    {
        var gtin = Gtin8.Parse("96385074");
        var masked = gtin.ToMaskedString();
        Assert.Equal("********", masked);
    }

    [Fact]
    public void Gtin8_MaskedLengthEquals8()
    {
        var gtin = Gtin8.Parse("96385074");
        var masked = gtin.ToMaskedString();
        Assert.Equal(8, masked.Length);
        Assert.All(masked, c => Assert.Equal('*', c));
    }
}
