using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class SoundLevelTests
{
    [Theory]
    [InlineData("69 dB")]
    [InlineData("69 dB(A)")]
    [InlineData("69 dBA")]
    [InlineData("69")]
    [InlineData("85.5 dBC")]
    [InlineData("85.5 dB(C)")]
    [InlineData("100 dBB")]
    [InlineData("100 dB(B)")]
    [InlineData("40 dBZ")]
    [InlineData("40 dB(Z)")]
    [InlineData("0 dB")]
    [InlineData("-3 dB(A)")]
    [InlineData("  69  dB(A)  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SoundLevel.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("dB")]
    [InlineData("dB(A)")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SoundLevel.IsValid(input));
    }

    [Theory]
    [InlineData("69 dB", 69, SoundWeighting.Unweighted)]
    [InlineData("69 dB(A)", 69, SoundWeighting.A)]
    [InlineData("69 dBA", 69, SoundWeighting.A)]
    [InlineData("69 dB(B)", 69, SoundWeighting.B)]
    [InlineData("69 dBB", 69, SoundWeighting.B)]
    [InlineData("69 dB(C)", 69, SoundWeighting.C)]
    [InlineData("69 dBC", 69, SoundWeighting.C)]
    [InlineData("69 dB(Z)", 69, SoundWeighting.Z)]
    [InlineData("69 dBZ", 69, SoundWeighting.Z)]
    [InlineData("69", 69, SoundWeighting.Unweighted)]
    [InlineData("-3 dB(A)", -3, SoundWeighting.A)]
    [InlineData("85.5 dBC", 85.5, SoundWeighting.C)]
    public void TryParse_ReturnsExpectedProperties(string input, double expectedDecibels, SoundWeighting expectedWeighting)
    {
        Assert.True(SoundLevel.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal((decimal)expectedDecibels, result!.Decibels);
        Assert.Equal(expectedWeighting, result.Weighting);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(SoundLevel.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("dB")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SoundLevel.Parse(input));
    }

    [Theory]
    [InlineData("69 dB", "69 dB")]
    [InlineData("69 dB(A)", "69 dB(A)")]
    [InlineData("69 dBA", "69 dB(A)")]
    [InlineData("  69  dB  ", "69 dB")]
    [InlineData("85.5 dBC", "85.5 dB(C)")]
    [InlineData("69", "69 dB")]
    [InlineData(null, null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SoundLevel.Format(input));
    }

    [Fact]
    public void Format_FallbackToTrimmedInput_WhenInvalid()
    {
        Assert.Equal("abc", SoundLevel.Format("  abc  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(SoundLevel.Format("abc"));
    }

    [Theory]
    [InlineData("69 dB", "69 dB")]
    [InlineData("69 dBA", "69 dB(A)")]
    [InlineData("85.5 dBC", "85.5 dB(C)")]
    [InlineData("69", "69 dB")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SoundLevel.Normalize(input));
    }

    [Theory]
    [InlineData("69 dB", true)]
    [InlineData("69 dB(A)", true)]
    [InlineData("69 dBA", false)]
    [InlineData("  69  dB  ", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, SoundLevel.IsNormalized(input));
    }

    [Theory]
    [InlineData("69 dB", "69 dB")]
    [InlineData("69 dB(A)", "69 dB(A)")]
    [InlineData("85.5 dB(C)", "85.5 dB(C)")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var sound = SoundLevel.Parse(input);
        Assert.Equal(expected, sound.ToString());
    }

    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var sound = SoundLevel.Create(69m, SoundWeighting.A);
        Assert.Equal(69m, sound.Decibels);
        Assert.Equal(SoundWeighting.A, sound.Weighting);
        Assert.Equal("69 dB(A)", sound.Value);
    }

    [Fact]
    public void FromDecibels_CreatesUnweighted()
    {
        var sound = SoundLevel.FromDecibels(85m);
        Assert.Equal(85m, sound.Decibels);
        Assert.Equal(SoundWeighting.Unweighted, sound.Weighting);
        Assert.Equal("85 dB", sound.Value);
    }

    [Fact]
    public void FromDecibelA_CreatesAWeighted()
    {
        var sound = SoundLevel.FromDecibelA(69m);
        Assert.Equal(69m, sound.Decibels);
        Assert.Equal(SoundWeighting.A, sound.Weighting);
        Assert.Equal("69 dB(A)", sound.Value);
    }

    [Fact]
    public void Equality_SameValueAndWeighting()
    {
        var a = SoundLevel.Create(69m, SoundWeighting.A);
        var b = SoundLevel.Create(69m, SoundWeighting.A);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentWeighting_NotEqual()
    {
        var a = SoundLevel.Create(69m, SoundWeighting.A);
        var b = SoundLevel.Create(69m, SoundWeighting.C);
        Assert.False(a == b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equality_DifferentDecibels_NotEqual()
    {
        var a = SoundLevel.Create(69m, SoundWeighting.A);
        var b = SoundLevel.Create(70m, SoundWeighting.A);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Comparison_OrdersByDecibels()
    {
        var low = SoundLevel.Create(40m, SoundWeighting.A);
        var high = SoundLevel.Create(85m, SoundWeighting.A);
        Assert.True(low < high);
        Assert.True(high > low);
        Assert.True(low <= high);
        Assert.True(high >= low);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var sound = SoundLevel.Create(69m, SoundWeighting.A);
        Assert.Equal(1, sound.CompareTo(null));
    }

    [Theory]
    [InlineData("69 db(a)", 69, SoundWeighting.A)]
    [InlineData("69 Db(A)", 69, SoundWeighting.A)]
    [InlineData("69 DB", 69, SoundWeighting.Unweighted)]
    public void TryParse_IsCaseInsensitive(string input, double expectedDecibels, SoundWeighting expectedWeighting)
    {
        Assert.True(SoundLevel.TryParse(input, out var result));
        Assert.Equal((decimal)expectedDecibels, result!.Decibels);
        Assert.Equal(expectedWeighting, result.Weighting);
    }

    [Theory]
    [InlineData("5,5 dB(A)", 5.5, SoundWeighting.A)]
    [InlineData("1 000 dB", 1000, SoundWeighting.Unweighted)]
    public void TryParse_HandlesEuropeanNumberFormats(string input, double expectedDecibels, SoundWeighting expectedWeighting)
    {
        Assert.True(SoundLevel.TryParse(input, out var result));
        Assert.Equal((decimal)expectedDecibels, result!.Decibels);
        Assert.Equal(expectedWeighting, result.Weighting);
    }
}
