using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class BoatCeDesignCategoryTests
{
    [Fact]
    public void All_HasFourCategories() =>
        Assert.Equal(4, BoatCeDesignCategory.All.Count);

    [Theory]
    [InlineData("A")]
    [InlineData("a")]
    [InlineData("B")]
    [InlineData("CE-C")]
    [InlineData("CE D")]
    [InlineData("Kategori A")]
    [InlineData("Category B")]
    [InlineData("Ocean")]
    [InlineData("Hav")]
    [InlineData("Inomskärs")]
    [InlineData("Sheltered waters")]
    public void IsValid_ReturnsTrue_ForKnownInputs(string input) =>
        Assert.True(BoatCeDesignCategory.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("E")]
    [InlineData("nope")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(BoatCeDesignCategory.IsValid(input));

    [Theory]
    [InlineData("A", "A")]
    [InlineData("a", "A")]
    [InlineData("Ocean", "A")]
    [InlineData("Hav", "A")]
    [InlineData("CE-D", "D")]
    [InlineData("Sheltered waters", "D")]
    [InlineData(null, null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, BoatCeDesignCategory.Normalize(input));

    [Fact]
    public void Parse_A_HasOceanProperties()
    {
        var c = BoatCeDesignCategory.Parse("A");
        Assert.Equal("Ocean", c.EnglishName);
        Assert.Equal("Hav", c.LocalizedName);
        Assert.True(c.MaxBeaufortWindForce >= 9);
        Assert.True(c.MaxSignificantWaveHeightM >= 4);
    }

    [Fact]
    public void Parse_D_HasShelteredProperties()
    {
        var c = BoatCeDesignCategory.Parse("D");
        Assert.Equal("Sheltered waters", c.EnglishName);
        Assert.True(c.MaxSignificantWaveHeightM <= 0.5m);
    }

    [Fact]
    public void Parse_ReturnsSameInstance() =>
        Assert.Same(BoatCeDesignCategory.A, BoatCeDesignCategory.Parse("A"));

    [Fact]
    public void ToMaskedString_ReturnsStar() =>
        Assert.Equal("*", BoatCeDesignCategory.A.ToMaskedString());

    [Fact]
    public void Equality()
    {
        Assert.True(BoatCeDesignCategory.A == BoatCeDesignCategory.Parse("ocean"));
        Assert.True(BoatCeDesignCategory.A != BoatCeDesignCategory.B);
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata() =>
        Assert.Equal("⛵", BoatCeDesignCategory.TypeInfo.Emoji);
}
