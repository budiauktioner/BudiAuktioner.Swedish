using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ChildShoeSizeTests
{
    [Theory]
    [InlineData("EU 28", 28, ShoeSizeSystem.EU)]
    [InlineData("eu 28.5", 28.5, ShoeSizeSystem.EU)]
    [InlineData("EU16", 16, ShoeSizeSystem.EU)]
    [InlineData("child EU 28", 28, ShoeSizeSystem.EU)]
    [InlineData("CHILD eu 39", 39, ShoeSizeSystem.EU)]
    [InlineData("barn 28", 28, ShoeSizeSystem.EU)]
    [InlineData("BARN 16", 16, ShoeSizeSystem.EU)]
    [InlineData("28", 28, ShoeSizeSystem.EU)]
    [InlineData("39", 39, ShoeSizeSystem.EU)]
    [InlineData("US 10.5C", 28, ShoeSizeSystem.USMen)]
    [InlineData("us 11C", 29, ShoeSizeSystem.USMen)]
    [InlineData("US 2Y", 34, ShoeSizeSystem.USMen)]
    [InlineData("US 3.5Y", 36, ShoeSizeSystem.USMen)]
    [InlineData("UK 11.5", 28, ShoeSizeSystem.UK)]
    [InlineData("uk 10", 26.5, ShoeSizeSystem.UK)]
    [InlineData("UK -0.5", 33, ShoeSizeSystem.UK)]
    public void TryParse_ReturnsExpectedEuAndSystem(string input, decimal expectedEu, ShoeSizeSystem expectedSystem)
    {
        var ok = ChildShoeSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectedEu, size!.EuSize);
        Assert.Equal(expectedSystem, size.System);
    }

    [Fact]
    public void TryParse_UkYouthTriedBeforeToddler_WhenBothValid()
    {
        var ok = ChildShoeSize.TryParse("UK 1", out var size);

        Assert.True(ok);
        Assert.Equal(34.5m, size!.EuSize);
        Assert.Equal(ShoeSizeSystem.UK, size.System);
    }

    [Fact]
    public void TryParse_Eu28_HasExpectedConversions()
    {
        var ok = ChildShoeSize.TryParse("EU 28", out var size);

        Assert.True(ok);
        Assert.Equal("10.5C", size!.UsSize);
        Assert.Equal("11.5", size.UkSize);
        Assert.Equal("EU 28", size.Value);
    }

    [Fact]
    public void TryParse_Eu35_HasExpectedUsAndUk()
    {
        var ok = ChildShoeSize.TryParse("EU 35", out var size);

        Assert.True(ok);
        Assert.Equal("2.5Y", size!.UsSize);
        Assert.Equal("1.5", size.UkSize);
    }

    [Fact]
    public void TryParse_Eu33_HasNegativeUkLabel()
    {
        var ok = ChildShoeSize.TryParse("EU 33", out var size);

        Assert.True(ok);
        Assert.Equal("1Y", size!.UsSize);
        Assert.Equal("-0.5", size.UkSize);
    }

    [Fact]
    public void TryParse_Eu32Point5_HasBlendedUsLabelAcrossCAndY()
    {
        var ok = ChildShoeSize.TryParse("EU 32.5", out var size);

        Assert.True(ok);
        Assert.Equal("13C/1Y", size!.UsSize);
    }

    [Theory]
    [InlineData("EU 28", "EU 28")]
    [InlineData("us 11C", "EU 29")]
    [InlineData("UK 11.5", "EU 28")]
    [InlineData(" 28.5 ", "EU 28.5")]
    [InlineData("barn 20", "EU 20")]
    public void Format_ReturnsEuDisplay(string input, string expected)
    {
        Assert.Equal(expected, ChildShoeSize.Format(input));
    }

    [Theory]
    [InlineData("EU 28", "EU 28")]
    [InlineData("US 10.5C", "EU 28")]
    public void Normalize_ReturnsSameAsFormat_ForValid(string input, string expected)
    {
        Assert.Equal(expected, ChildShoeSize.Normalize(input));
    }

    [Theory]
    [InlineData("EU 28")]
    [InlineData("EU 28.5")]
    public void IsNormalized_ReturnsTrue_ForCanonicalEu(string input)
    {
        Assert.True(ChildShoeSize.IsNormalized(input));
    }

    [Theory]
    [InlineData("eu 28")]
    [InlineData("US 11C")]
    [InlineData(null)]
    [InlineData("")]
    public void IsNormalized_ReturnsFalse_WhenNotCanonicalOrInvalid(string? input)
    {
        Assert.False(ChildShoeSize.IsNormalized(input));
    }

    [Fact]
    public void ToString_And_ToNormalizedString_MatchValue()
    {
        var size = ChildShoeSize.Parse("EU 28.5");

        Assert.Equal("EU 28.5", size.ToString());
        Assert.Equal("EU 28.5", size.ToNormalizedString());
        Assert.Equal(size.Value, size.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("EU 15")]
    [InlineData("15")]
    [InlineData("EU 40")]
    [InlineData("40")]
    [InlineData("EU 28.25")]
    [InlineData("not a size")]
    [InlineData("US")]
    [InlineData("US 11")]
    [InlineData("US 99C")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ChildShoeSize.IsValid(input));
    }

    [Theory]
    [InlineData("EU 16")]
    [InlineData("39")]
    [InlineData("US 6Y")]
    [InlineData("UK 5.5")]
    public void IsValid_ReturnsTrue_ForBoundaryEu(string input)
    {
        Assert.True(ChildShoeSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EU 15")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = ChildShoeSize.TryParse(input, out var size);

        Assert.False(ok);
        Assert.Null(size);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => ChildShoeSize.Parse("EU 15"));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("nope", ChildShoeSize.Format("  nope  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Equality_SameSize()
    {
        var a = ChildShoeSize.Parse("EU 28");
        var b = ChildShoeSize.Parse("28");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Comparison_SmallerToLarger()
    {
        var s = ChildShoeSize.Parse("EU 22");
        var l = ChildShoeSize.Parse("EU 32");
        Assert.True(s < l);
        Assert.True(l > s);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ChildShoeSize.Parse("EU 24");
        var b = ChildShoeSize.Parse("EU 32");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ChildShoeSize.Parse("EU 28");
        Assert.Equal(1, a.CompareTo(null));
    }
}
