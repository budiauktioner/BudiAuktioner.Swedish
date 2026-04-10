using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class AdultShoeSizeTests
{
    [Theory]
    [InlineData("EU 42", 42, ShoeSizeSystem.EU)]
    [InlineData("eu 42.5", 42.5, ShoeSizeSystem.EU)]
    [InlineData("EU35", 35, ShoeSizeSystem.EU)]
    [InlineData("42", 42, ShoeSizeSystem.EU)]
    [InlineData("50", 50, ShoeSizeSystem.EU)]
    [InlineData("US 9", 42, ShoeSizeSystem.USMen)]
    [InlineData("us 9.5", 42.5, ShoeSizeSystem.USMen)]
    [InlineData("US M 9", 42, ShoeSizeSystem.USMen)]
    [InlineData("US MEN 9", 42, ShoeSizeSystem.USMen)]
    [InlineData("US men 9.5", 42.5, ShoeSizeSystem.USMen)]
    [InlineData("US W 10.5", 42, ShoeSizeSystem.USWomen)]
    [InlineData("US women 10.5", 42, ShoeSizeSystem.USWomen)]
    [InlineData("US W10.5", 42, ShoeSizeSystem.USWomen)]
    [InlineData("UK 8.5", 42, ShoeSizeSystem.UK)]
    [InlineData("uk 8", 41.5, ShoeSizeSystem.UK)]
    public void TryParse_ReturnsExpectedEuAndSystem(string input, decimal expectedEu, ShoeSizeSystem expectedSystem)
    {
        var ok = AdultShoeSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectedEu, size!.EuSize);
        Assert.Equal(expectedSystem, size.System);
    }

    [Fact]
    public void TryParse_Eu42_HasExpectedConversions()
    {
        var ok = AdultShoeSize.TryParse("EU 42", out var size);

        Assert.True(ok);
        Assert.Equal(9m, size!.UsMenSize);
        Assert.Equal(10.5m, size.UsWomenSize);
        Assert.Equal(8.5m, size.UkSize);
        Assert.Equal("EU 42", size.Value);
    }

    [Fact]
    public void TryParse_Eu42Half_HasExpectedConversions()
    {
        var ok = AdultShoeSize.TryParse("42.5", out var size);

        Assert.True(ok);
        Assert.Equal(9.5m, size!.UsMenSize);
        Assert.Equal(11m, size.UsWomenSize);
        Assert.Equal(9m, size.UkSize);
    }

    [Theory]
    [InlineData("EU 42", "EU 42")]
    [InlineData("us 9", "EU 42")]
    [InlineData("UK 8.5", "EU 42")]
    [InlineData(" 42.5 ", "EU 42.5")]
    public void Format_ReturnsEuDisplay(string input, string expected)
    {
        Assert.Equal(expected, AdultShoeSize.Format(input));
    }

    [Theory]
    [InlineData("EU 42", "EU 42")]
    [InlineData("US W 10.5", "EU 42")]
    public void Normalize_ReturnsSameAsFormat_ForValid(string input, string expected)
    {
        Assert.Equal(expected, AdultShoeSize.Normalize(input));
    }

    [Theory]
    [InlineData("EU 42")]
    [InlineData("EU 42.5")]
    public void IsNormalized_ReturnsTrue_ForCanonicalEu(string input)
    {
        Assert.True(AdultShoeSize.IsNormalized(input));
    }

    [Theory]
    [InlineData("eu 42")]
    [InlineData("US 9")]
    [InlineData(null)]
    [InlineData("")]
    public void IsNormalized_ReturnsFalse_WhenNotCanonicalOrInvalid(string? input)
    {
        Assert.False(AdultShoeSize.IsNormalized(input!));
    }

    [Fact]
    public void ToString_And_ToNormalizedString_MatchValue()
    {
        var size = AdultShoeSize.Parse("EU 42.5");

        Assert.Equal("EU 42.5", size.ToString());
        Assert.Equal("EU 42.5", size.ToNormalizedString());
        Assert.Equal(size.Value, size.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("EU 34")]
    [InlineData("34")]
    [InlineData("EU 51")]
    [InlineData("EU 42.25")]
    [InlineData("US 9.25")]
    [InlineData("not a size")]
    [InlineData("US")]
    [InlineData("US X 9")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(AdultShoeSize.IsValid(input));
    }

    [Theory]
    [InlineData("EU 35")]
    [InlineData("50")]
    [InlineData("US 17")]
    [InlineData("UK 16.5")]
    public void IsValid_ReturnsTrue_ForBoundaryEu(string input)
    {
        Assert.True(AdultShoeSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EU 34")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = AdultShoeSize.TryParse(input, out var size);

        Assert.False(ok);
        Assert.Null(size);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => AdultShoeSize.Parse("EU 34"));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("nope", AdultShoeSize.Format("  nope  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Equality_SameSize()
    {
        var a = AdultShoeSize.Parse("EU 42");
        var b = AdultShoeSize.Parse("42");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Comparison_SmallerToLarger()
    {
        var s = AdultShoeSize.Parse("EU 38");
        var l = AdultShoeSize.Parse("EU 44");
        Assert.True(s < l);
        Assert.True(l > s);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = AdultShoeSize.Parse("38");
        var b = AdultShoeSize.Parse("44");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = AdultShoeSize.Parse("42");
        Assert.Equal(1, a.CompareTo(null));
    }
}
