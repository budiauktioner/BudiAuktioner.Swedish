using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ClothingFitTests
{
    [Fact]
    public void All_HasExpectedCount() =>
        Assert.Equal(5, ClothingFit.All.Count);

    [Theory]
    [InlineData("Slim")]
    [InlineData("slim")]
    [InlineData("SLIM")]
    [InlineData("Smal")]
    [InlineData("Slim fit")]
    [InlineData("Skinny")]
    [InlineData("Figurnära")]
    [InlineData("Åtsittande")]
    [InlineData("Regular")]
    [InlineData("Normal")]
    [InlineData("Standard")]
    [InlineData("Straight")]
    [InlineData("Klassisk passform")]
    [InlineData("Loose")]
    [InlineData("Lös")]
    [InlineData("Lös passform")]
    [InlineData("Relaxed")]
    [InlineData("Vid")]
    [InlineData("Comfort fit")]
    [InlineData("Oversized")]
    [InlineData("Oversize")]
    [InlineData("Boxy")]
    [InlineData("Överdimensionerad")]
    [InlineData("Tailored")]
    [InlineData("Skräddarsydd")]
    [InlineData("Tailored fit")]
    [InlineData("Slim-tailored")]
    [InlineData("  Slim  ")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(ClothingFit.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("nope")]
    [InlineData("athletic")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(ClothingFit.IsValid(input));

    [Theory]
    [InlineData("Smal", "Slim")]
    [InlineData("Skinny", "Slim")]
    [InlineData("Normal", "Regular")]
    [InlineData("Lös", "Loose")]
    [InlineData("Relaxed", "Loose")]
    [InlineData("Boxy", "Oversized")]
    [InlineData("Skräddarsydd", "Tailored")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, ClothingFit.Normalize(input));

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(ClothingFit.Slim, ClothingFit.Parse("smal"));

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => ClothingFit.Parse("nope"));

    [Fact]
    public void CompareTo_OrdersBySortOrder()
    {
        Assert.True(ClothingFit.Slim < ClothingFit.Regular);
        Assert.True(ClothingFit.Regular < ClothingFit.Loose);
        Assert.True(ClothingFit.Loose < ClothingFit.Oversized);
        Assert.True(ClothingFit.Oversized < ClothingFit.Tailored);
    }

    [Fact]
    public void ToMaskedString_ReturnsStars() =>
        Assert.Equal("****", ClothingFit.Slim.ToMaskedString());

    [Fact]
    public void Equality()
    {
        var a = ClothingFit.Parse("Slim");
        var b = ClothingFit.Parse("smal");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Clothing Fit", ClothingFit.TypeInfo.EnglishName);
        Assert.Equal("Passform", ClothingFit.TypeInfo.LocalizedName);
    }
}
