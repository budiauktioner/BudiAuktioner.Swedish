using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class AspectRatioTests
{
    [Theory]
    [InlineData("16:9")]
    [InlineData("4:3")]
    [InlineData("21:9")]
    [InlineData("32:9")]
    [InlineData("1:1")]
    [InlineData("16/9")]
    [InlineData("16x9")]
    [InlineData(" 16:9 ")]
    [InlineData("32:18")] // reducible to 16:9
    [InlineData("8:6")]   // reducible to 4:3
    [InlineData("1.78")]  // close to 16:9
    [InlineData("1.78:1")]
    [InlineData("1,33")]  // close to 4:3
    public void IsValid_ReturnsTrue_ForKnownAspectRatios(string input) =>
        Assert.True(AspectRatio.IsValid(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("0:0")]
    [InlineData("16:0")]
    [InlineData("11:7")] // not in canonical list, decimal not close enough
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input) =>
        Assert.False(AspectRatio.IsValid(input));

    [Theory]
    [InlineData("16:9", "16:9")]
    [InlineData("32:18", "16:9")]
    [InlineData("16/9", "16:9")]
    [InlineData("16x9", "16:9")]
    [InlineData("4:3", "4:3")]
    [InlineData("8:6", "4:3")]
    [InlineData("1.78", "16:9")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsCanonical(string? input, string? expected) =>
        Assert.Equal(expected, AspectRatio.Normalize(input));

    [Fact]
    public void Parse_16_9_HasExpectedProperties()
    {
        var r = AspectRatio.Parse("16:9");
        Assert.Equal(16, r.Width);
        Assert.Equal(9, r.Height);
        Assert.Equal("Widescreen", r.CommonName);
    }

    [Fact]
    public void Ratio_IsCorrect()
    {
        var r = AspectRatio.Parse("16:9");
        Assert.True(Math.Abs((double)(r.Ratio - (16m / 9m))) < 0.0001);
    }

    [Fact]
    public void ToMaskedString_PreservesShape() =>
        Assert.Equal("**:*", AspectRatio.SixteenNine.ToMaskedString());

    [Theory]
    [InlineData("16:9", true)]
    [InlineData("16/9", false)]
    [InlineData("32:18", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected) =>
        Assert.Equal(expected, AspectRatio.IsNormalized(input));

    [Fact]
    public void Equality_AndComparison()
    {
        var a = AspectRatio.Parse("16:9");
        var b = AspectRatio.Parse("32:18");
        var c = AspectRatio.Parse("21:9");
        Assert.True(a == b);
        Assert.True(a < c);
    }

    [Fact]
    public void Parse_Throws_ForInvalid() =>
        Assert.Throws<ArgumentException>(() => AspectRatio.Parse("nope"));
}
