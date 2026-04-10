using Buildi.Primitives.Property;

namespace Buildi.Primitives.Tests.Property;

public class PropertyMaskingExtensionsTests
{
    [Fact]
    public void PropertyDesignation_PreservesNameMasksNumbers()
    {
        var prop = SwedishPropertyDesignation.Parse("Stockholm Söder 75:2");
        var masked = prop.ToMaskedString();
        Assert.Equal("Stockholm Söder **:*", masked);
    }

    [Fact]
    public void PropertyDesignation_ContainsDesignationName()
    {
        var prop = SwedishPropertyDesignation.Parse("Stockholm Söder 75:2");
        var masked = prop.ToMaskedString();
        Assert.StartsWith(prop.DesignationName, masked);
    }

    [Fact]
    public void PropertyDesignation_MaskCharCountMatchesDigitCount()
    {
        var prop = SwedishPropertyDesignation.Parse("Stockholm Söder 75:2");
        var masked = prop.ToMaskedString();
        var blockMaskCount = prop.BlockNumber.ToString().Length;
        var unitMaskCount = prop.UnitNumber.ToString().Length;
        Assert.Equal(blockMaskCount + unitMaskCount, masked.Count(c => c == '*'));
    }

    [Fact]
    public void PropertyDesignation_PreservesColonSeparator()
    {
        var prop = SwedishPropertyDesignation.Parse("Stockholm Söder 75:2");
        var masked = prop.ToMaskedString();
        Assert.Contains(":", masked);
    }
}
