using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class EmissionRateTests
{
    [Theory]
    [InlineData("221 g/km")]
    [InlineData("95.7 mg/km")]
    [InlineData("350 g/mi")]
    [InlineData("221")]
    [InlineData("  221 g/km  ")]
    [InlineData("106.8 mg/km")]
    [InlineData("350 g/mile")]
    [InlineData("500 mg/mi")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EmissionRate.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("0 g/km")]
    [InlineData("-5 g/km")]
    [InlineData("0")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EmissionRate.IsValid(input));
    }

    [Fact]
    public void TryParse_GramsPerKm_ReturnsExpectedProperties()
    {
        var ok = EmissionRate.TryParse("221 g/km", out var er);

        Assert.True(ok);
        Assert.NotNull(er);
        Assert.Equal(221m, er.GramsPerKm);
        Assert.Equal("221 g/km", er.Value);
    }

    [Fact]
    public void TryParse_MilligramsPerKm_ConvertsCorrectly()
    {
        var ok = EmissionRate.TryParse("95.7 mg/km", out var er);

        Assert.True(ok);
        Assert.NotNull(er);
        Assert.Equal(0.0957m, er.GramsPerKm);
        Assert.Equal("95.7 mg/km", er.Value);
    }

    [Fact]
    public void TryParse_GramsPerMile_ConvertsCorrectly()
    {
        var ok = EmissionRate.TryParse("350 g/mi", out var er);

        Assert.True(ok);
        Assert.NotNull(er);
        Assert.Equal(Math.Round(350m / 1.60934m, 6), er.GramsPerKm);
        Assert.Equal("350 g/mi", er.Value);
    }

    [Fact]
    public void TryParse_GramsPerMile_LongForm_ConvertsCorrectly()
    {
        var ok = EmissionRate.TryParse("350 g/mile", out var er);

        Assert.True(ok);
        Assert.NotNull(er);
        Assert.Equal(Math.Round(350m / 1.60934m, 6), er.GramsPerKm);
        Assert.Equal("350 g/mi", er.Value);
    }

    [Fact]
    public void TryParse_MilligramsPerMile_ConvertsCorrectly()
    {
        var ok = EmissionRate.TryParse("500 mg/mi", out var er);

        Assert.True(ok);
        Assert.NotNull(er);
        Assert.Equal(Math.Round(500m / 1000m / 1.60934m, 6), er.GramsPerKm);
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToGramsPerKm()
    {
        var ok = EmissionRate.TryParse("221", out var er);

        Assert.True(ok);
        Assert.NotNull(er);
        Assert.Equal(221m, er.GramsPerKm);
        Assert.Equal("221 g/km", er.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0 g/km")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = EmissionRate.TryParse(input, out var er);

        Assert.False(ok);
        Assert.Null(er);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => EmissionRate.Parse("not-emission"));
    }

    [Theory]
    [InlineData("221 g/km", "221 g/km")]
    [InlineData("95.7 mg/km", "95.7 mg/km")]
    [InlineData("221", "221 g/km")]
    [InlineData(null, null)]
    [InlineData("bad", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EmissionRate.Format(input));
    }

    [Theory]
    [InlineData("bad", "bad")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string input, string expected)
    {
        Assert.Equal(expected, EmissionRate.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("221 g/km", "221 g/km")]
    [InlineData("221", "221 g/km")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EmissionRate.Normalize(input));
    }

    [Fact]
    public void Normalize_MgPerKm_ConvertsToGPerKm()
    {
        Assert.Equal("0.0957 g/km", EmissionRate.Normalize("95.7 mg/km"));
    }

    [Fact]
    public void Normalize_GPerMile_ConvertsToGPerKm()
    {
        var normalized = EmissionRate.Normalize("350 g/mi");

        Assert.NotNull(normalized);
        Assert.EndsWith(" g/km", normalized);
    }

    [Theory]
    [InlineData("221 g/km", true)]
    [InlineData("95.7 mg/km", false)]
    [InlineData("350 g/mi", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, EmissionRate.IsNormalized(input));
    }

    [Fact]
    public void MilligramsPerKm_ComputedCorrectly()
    {
        var er = EmissionRate.FromGramsPerKm(221m);
        Assert.Equal(221000m, er.MilligramsPerKm);
    }

    [Fact]
    public void GramsPerMile_ComputedCorrectly()
    {
        var er = EmissionRate.FromGramsPerKm(221m);
        Assert.Equal(221m * 1.60934m, er.GramsPerMile);
    }

    [Fact]
    public void FromGramsPerKm_CreatesCorrectly()
    {
        var er = EmissionRate.FromGramsPerKm(221m);

        Assert.Equal(221m, er.GramsPerKm);
        Assert.Equal("221 g/km", er.Value);
    }

    [Fact]
    public void FromMilligramsPerKm_CreatesCorrectly()
    {
        var er = EmissionRate.FromMilligramsPerKm(95.7m);

        Assert.Equal(0.0957m, er.GramsPerKm);
        Assert.Equal("95.7 mg/km", er.Value);
    }

    [Fact]
    public void FactoryMethods_ThrowForNonPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EmissionRate.FromGramsPerKm(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmissionRate.FromGramsPerKm(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmissionRate.FromMilligramsPerKm(0));
    }

    [Fact]
    public void Equality_SameValues()
    {
        var a = EmissionRate.FromGramsPerKm(221m);
        var b = EmissionRate.Parse("221 g/km");

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_BareNumberAndUnit_AreEqual()
    {
        var a = EmissionRate.Parse("221");
        var b = EmissionRate.Parse("221 g/km");

        Assert.True(a == b);
    }

    [Fact]
    public void Equality_DifferentValues()
    {
        var a = EmissionRate.FromGramsPerKm(221m);
        var b = EmissionRate.FromGramsPerKm(100m);

        Assert.True(a != b);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = EmissionRate.FromGramsPerKm(100m);
        var b = EmissionRate.FromGramsPerKm(221m);

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = EmissionRate.FromGramsPerKm(221m);
        Assert.Equal(1, a.CompareTo(null));
    }
}
