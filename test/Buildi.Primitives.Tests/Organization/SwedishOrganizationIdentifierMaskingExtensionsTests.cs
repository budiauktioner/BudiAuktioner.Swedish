using Buildi.Primitives.Organization;

namespace Buildi.Primitives.Tests.Organization;

public class SwedishOrganizationIdentifierMaskingExtensionsTests
{
    [Fact]
    public void LeiCode_ShowsLouPrefixAndMasksRest()
    {
        var lei = LeiCode.Parse("5493001KJTIIGC8Y1R12");
        var masked = lei.ToMaskedString();
        Assert.Equal("5493****************", masked);
    }

    [Fact]
    public void LeiCode_MaskedLengthEquals20()
    {
        var lei = LeiCode.Parse("5493001KJTIIGC8Y1R12");
        var masked = lei.ToMaskedString();
        Assert.Equal(20, masked.Length);
    }

    [Fact]
    public void LeiCode_PreservesLouPrefix()
    {
        var lei = LeiCode.Parse("529900T8BM49AURSDO55");
        var masked = lei.ToMaskedString();
        Assert.StartsWith("5299", masked);
        Assert.All(masked[4..], c => Assert.Equal('*', c));
    }

    [Fact]
    public void DunsNumber_MasksAllDigits()
    {
        var duns = DunsNumber.Parse("362498394");
        var masked = duns.ToMaskedString();
        Assert.Equal("*********", masked);
    }

    [Fact]
    public void DunsNumber_MaskedLengthEquals9()
    {
        var duns = DunsNumber.Parse("123456789");
        var masked = duns.ToMaskedString();
        Assert.Equal(9, masked.Length);
        Assert.All(masked, c => Assert.Equal('*', c));
    }
}
