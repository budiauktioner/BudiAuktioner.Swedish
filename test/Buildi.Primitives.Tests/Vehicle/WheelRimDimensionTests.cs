using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class WheelRimDimensionTests
{
    [Theory]
    [InlineData("18X7J")]
    [InlineData("16x6.5J")]
    [InlineData("22.5x9.00")]
    [InlineData("7Jx16")]
    [InlineData("15x6JJ")]
    [InlineData("  18x7J  ")]
    [InlineData("18,0x7J")]
    [InlineData("20x8")]
    [InlineData("18x7j")]
    [InlineData("10x3")]
    [InlineData("26x16")]
    [InlineData("6.5Jx15")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(WheelRimDimension.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("5x1J")]
    [InlineData("30x7J")]
    [InlineData("18x1J")]
    [InlineData("18x20J")]
    [InlineData("9x7J")]
    [InlineData("27x7J")]
    [InlineData("18x2J")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(WheelRimDimension.IsValid(input));
    }

    [Theory]
    [InlineData("18X7J", 18, 7, "J", "18x7J", "18 x 7 J")]
    [InlineData("16x6.5J", 16, 6.5, "J", "16x6.5J", "16 x 6.5 J")]
    [InlineData("22.5x9.00", 22.5, 9, "", "22.5x9", "22.5 x 9")]
    [InlineData("7Jx16", 16, 7, "J", "16x7J", "16 x 7 J")]
    [InlineData("15x6JJ", 15, 6, "JJ", "15x6JJ", "15 x 6 JJ")]
    [InlineData("20x8", 20, 8, "", "20x8", "20 x 8")]
    [InlineData("18x7j", 18, 7, "J", "18x7J", "18 x 7 J")]
    [InlineData("18,0x7J", 18, 7, "J", "18x7J", "18 x 7 J")]
    [InlineData("6.5Jx15", 15, 6.5, "J", "15x6.5J", "15 x 6.5 J")]
    public void TryParse_ReturnsExpectedProperties(
        string input,
        double expectedDiameter,
        double expectedWidth,
        string expectedFlange,
        string expectedValue,
        string expectedFormatted)
    {
        var ok = WheelRimDimension.TryParse(input, out var dim);

        Assert.True(ok);
        Assert.NotNull(dim);
        Assert.Equal((decimal)expectedDiameter, dim.DiameterInches);
        Assert.Equal((decimal)expectedWidth, dim.WidthInches);
        Assert.Equal(expectedFlange, dim.FlangeType);
        Assert.Equal(expectedValue, dim.Value);
        Assert.Equal(expectedFormatted, dim.ToString());
        Assert.Equal(expectedValue, dim.ToNormalizedString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("5x1J")]
    [InlineData("30x7J")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = WheelRimDimension.TryParse(input, out var dim);

        Assert.False(ok);
        Assert.Null(dim);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => WheelRimDimension.Parse("not-a-rim"));
    }

    [Theory]
    [InlineData("18X7J", "18 x 7 J")]
    [InlineData("22.5x9.00", "22.5 x 9")]
    [InlineData("7Jx16", "16 x 7 J")]
    [InlineData(null, null)]
    [InlineData("bad", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, WheelRimDimension.Format(input));
    }

    [Theory]
    [InlineData("bad", "bad")]
    [InlineData("  x  ", "x")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string input, string expected)
    {
        Assert.Equal(expected, WheelRimDimension.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("18X7J", "18x7J")]
    [InlineData("22.5x9.00", "22.5x9")]
    [InlineData("7Jx16", "16x7J")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, WheelRimDimension.Normalize(input));
    }

    [Theory]
    [InlineData("18x7J", true)]
    [InlineData("18X7J", false)]
    [InlineData("7Jx16", false)]
    [InlineData("22.5x9", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, WheelRimDimension.IsNormalized(input));
    }

    [Fact]
    public void Equality_SameDimension()
    {
        var a = WheelRimDimension.Parse("18X7J");
        var b = WheelRimDimension.Parse("18x7J");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_ReversedFormat_SameDimension()
    {
        var a = WheelRimDimension.Parse("16x7J");
        var b = WheelRimDimension.Parse("7Jx16");
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentDimensions()
    {
        var a = WheelRimDimension.Parse("18x7J");
        var b = WheelRimDimension.Parse("16x6.5J");
        Assert.True(a != b);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = WheelRimDimension.Parse("15x6J");
        var b = WheelRimDimension.Parse("18x7J");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = WheelRimDimension.Parse("18x7J");
        Assert.Equal(1, a.CompareTo(null));
    }
}
