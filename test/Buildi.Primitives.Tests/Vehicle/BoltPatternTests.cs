using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class BoltPatternTests
{
    [Theory]
    [InlineData("5x114.3")]
    [InlineData("4x100")]
    [InlineData("6x139.7")]
    [InlineData("5x112")]
    [InlineData("4x108")]
    [InlineData("5 x 114.3")]
    [InlineData("5X114.3")]
    [InlineData("5×114.3")]
    [InlineData("5x114,3")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(BoltPattern.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("2x100")]
    [InlineData("11x100")]
    [InlineData("5x10")]
    [InlineData("5x300")]
    [InlineData("5")]
    [InlineData("x114.3")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(BoltPattern.IsValid(input));
    }

    [Theory]
    [InlineData("5x114.3", 5, 114.3)]
    [InlineData("4x100", 4, 100)]
    [InlineData("6x139.7", 6, 139.7)]
    [InlineData("5x112", 5, 112)]
    [InlineData("5 x 114.3", 5, 114.3)]
    [InlineData("5x114,3", 5, 114.3)]
    public void TryParse_ReturnsExpectedProperties(string input, int expectedBolts, double expectedPcd)
    {
        Assert.True(BoltPattern.TryParse(input, out var result));
        Assert.Equal(expectedBolts, result!.BoltCount);
        Assert.Equal((decimal)expectedPcd, result.PitchCircleDiameter);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(BoltPattern.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("2x100")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => BoltPattern.Parse(input));
    }

    [Theory]
    [InlineData("5x114.3", "5 x 114.3")]
    [InlineData("4x100", "4 x 100")]
    [InlineData("5 x 114.3", "5 x 114.3")]
    [InlineData("5X114,3", "5 x 114.3")]
    [InlineData("  5x114.3  ", "5 x 114.3")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, BoltPattern.Format(input));
    }

    [Theory]
    [InlineData("5x114.3", "5x114.3")]
    [InlineData("5 x 114.3", "5x114.3")]
    [InlineData("5X114,3", "5x114.3")]
    [InlineData("4x100", "4x100")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, BoltPattern.Normalize(input));
    }

    [Theory]
    [InlineData("5x114.3", "5 x 114.3")]
    [InlineData("4x100", "4 x 100")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var pattern = BoltPattern.Parse(input);
        Assert.Equal(expected, pattern.ToString());
    }

    [Theory]
    [InlineData("5x114.3", "5x114.3")]
    [InlineData("4x100", "4x100")]
    public void ToNormalizedString_ReturnsCompactForm(string input, string expected)
    {
        var pattern = BoltPattern.Parse(input);
        Assert.Equal(expected, pattern.ToNormalizedString());
    }

    [Fact]
    public void IsNormalized_True_ForNormalizedInput()
    {
        Assert.True(BoltPattern.IsNormalized("5x114.3"));
        Assert.True(BoltPattern.IsNormalized("4x100"));
    }

    [Fact]
    public void IsNormalized_False_ForUnnormalizedInput()
    {
        Assert.False(BoltPattern.IsNormalized("5 x 114.3"));
        Assert.False(BoltPattern.IsNormalized("5X114.3"));
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = BoltPattern.Parse("4x100");
        var b = BoltPattern.Parse("5x114.3");
        Assert.True(a < b);
        Assert.True(b > a);
    }

    [Fact]
    public void Equality_SameValue()
    {
        var a = BoltPattern.Parse("5x114.3");
        var b = BoltPattern.Parse("5 x 114.3");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue()
    {
        var a = BoltPattern.Parse("5x114.3");
        var b = BoltPattern.Parse("4x100");
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Format_Fallback_ReturnsNull_ForInvalid()
    {
        Assert.Null(BoltPattern.Format("invalid"));
    }

    [Fact]
    public void Format_Fallback_ReturnsTrimmedInput_WhenEnabled()
    {
        Assert.Equal("invalid", BoltPattern.Format("  invalid  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_Fallback_ReturnsNull_ForNull()
    {
        Assert.Null(BoltPattern.Normalize(null));
    }

    [Fact]
    public void Normalize_Fallback_ReturnsTrimmedInput_WhenEnabled()
    {
        Assert.Equal("invalid", BoltPattern.Normalize("  invalid  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("3x50", 3, 50)]
    [InlineData("10x250", 10, 250)]
    public void BoundaryValues_AreValid(string input, int expectedBolts, int expectedPcd)
    {
        Assert.True(BoltPattern.TryParse(input, out var result));
        Assert.Equal(expectedBolts, result!.BoltCount);
        Assert.Equal((decimal)expectedPcd, result.PitchCircleDiameter);
    }
}
