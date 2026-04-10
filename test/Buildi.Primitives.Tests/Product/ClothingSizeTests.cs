using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ClothingSizeTests
{
    [Theory]
    [InlineData("M", true, false, "M")]
    [InlineData("128", false, true, "EU 128")]
    [InlineData("EU 40", true, false, "L")]
    [InlineData("EU 128", false, true, "EU 128")]
    [InlineData("120/130", false, true, "EU 122")]
    [InlineData("110-116", false, true, "EU 110")]
    public void TryParse_AutoDetects_AdultOrChild(string input, bool expectAdult, bool expectChild, string expectedValue)
    {
        var ok = ClothingSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectAdult, size.IsAdult);
        Assert.Equal(expectChild, size.IsChild);
        Assert.Equal(expectedValue, size.Value);
    }

    [Theory]
    [InlineData("adult M", "M")]
    [InlineData("ADULT  M", "M")]
    [InlineData("vuxen M", "M")]
    [InlineData("Vuxen  XL", "XL")]
    [InlineData("child 128", "EU 128")]
    [InlineData("CHILD 128", "EU 128")]
    [InlineData("barn 128", "EU 128")]
    [InlineData("Barn  128", "EU 128")]
    [InlineData("barn 120/130", "EU 122")]
    [InlineData("child 110-116", "EU 110")]
    public void TryParse_ExplicitPrefix_ParsesExpectedKind(string input, string expectedValue)
    {
        var ok = ClothingSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectedValue, size.Value);
    }

    [Fact]
    public void AsAdult_ReturnsInstance_WhenAdult()
    {
        Assert.True(ClothingSize.TryParse("M", out var cs));
        Assert.NotNull(cs);
        var adult = cs.AsAdult();
        Assert.NotNull(adult);
        Assert.Equal("M", adult.Value);
        Assert.Null(cs.AsChild());
    }

    [Fact]
    public void AsChild_ReturnsInstance_WhenChild()
    {
        Assert.True(ClothingSize.TryParse("128", out var cs));
        Assert.NotNull(cs);
        var child = cs.AsChild();
        Assert.NotNull(child);
        Assert.Equal(128, child.HeightCm);
        Assert.Null(cs.AsAdult());
    }

    [Theory]
    [InlineData("M", "M", "EU 38")]
    [InlineData("128", "EU 128", "EU 128")]
    [InlineData("EU 40", "L", "EU 40")]
    [InlineData("EU 128", "EU 128", "EU 128")]
    [InlineData("adult M", "M", "EU 38")]
    [InlineData("child 128", "EU 128", "EU 128")]
    [InlineData("120/130", "EU 122", "EU 122")]
    [InlineData("barn 120/130", "EU 122", "EU 122")]
    public void Format_And_Normalize_Delegate_ToUnderlying(string input, string? expectedFormat, string? expectedNormalize)
    {
        Assert.Equal(expectedFormat, ClothingSize.Format(input));
        Assert.Equal(expectedNormalize, ClothingSize.Normalize(input));
    }

    [Theory]
    [InlineData("EU 40")]
    [InlineData("EU 128")]
    public void IsNormalized_ReturnsTrue_WhenCanonical(string input)
    {
        Assert.True(ClothingSize.IsNormalized(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-size")]
    [InlineData("adult")]
    [InlineData("adult not-a-size")]
    [InlineData("child")]
    [InlineData("child 999")]
    [InlineData("vuxen 999")]
    [InlineData("barn 999")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ClothingSize.IsValid(input));
    }

    [Theory]
    [InlineData("not-a-size")]
    [InlineData("adult")]
    [InlineData("child 999")]
    public void TryParse_ReturnsNull_ForInvalidInput(string input)
    {
        Assert.False(ClothingSize.TryParse(input, out var size));
        Assert.Null(size);
    }

    [Theory]
    [InlineData("not-a-size")]
    [InlineData("adult xyz")]
    public void Parse_Throws_ForInvalidInput(string input)
    {
        Assert.Throws<ArgumentException>(() => ClothingSize.Parse(input));
    }

    [Theory]
    [InlineData("M", "M", "EU 38")]
    [InlineData("128", "EU 128", "EU 128")]
    public void ToString_And_ToNormalizedString_MatchUnderlying(string input, string expectedToString, string expectedNormalized)
    {
        var cs = ClothingSize.Parse(input);
        Assert.Equal(expectedToString, cs.ToString());
        Assert.Equal(expectedNormalized, cs.ToNormalizedString());
    }

    [Fact]
    public void ExplicitAdultPrefix_DoesNotFallBack_ToChild_WhenAdultParseFails()
    {
        Assert.False(ClothingSize.TryParse("adult 128", out _));
    }

    [Fact]
    public void ExplicitChildPrefix_DoesNotFallBack_ToAdult_WhenChildParseFails()
    {
        Assert.False(ClothingSize.TryParse("child M", out _));
    }

    [Fact]
    public void Equality_SameAdultSize()
    {
        var a = ClothingSize.Parse("M");
        var b = ClothingSize.Parse("EU 38");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equality_AdultVsChild_NotEqual()
    {
        var adult = ClothingSize.Parse("adult M");
        var child = ClothingSize.Parse("child 128");
        Assert.True(adult != child);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ClothingSize.Parse("110");
        var b = ClothingSize.Parse("M");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ClothingSize.Parse("M");
        Assert.Equal(1, a.CompareTo(null));
    }
}
