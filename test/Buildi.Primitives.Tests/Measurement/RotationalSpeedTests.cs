using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class RotationalSpeedTests
{
    [Theory]
    [InlineData("5200 rpm")]
    [InlineData("100 rps")]
    [InlineData("523.6 rad/s")]
    [InlineData("3000 RPM")]
    [InlineData("5200 varv/min")]
    [InlineData("5200")]
    [InlineData("100 r/min")]
    [InlineData("50 rev/min")]
    [InlineData("10 rev/s")]
    [InlineData("-100 rpm")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(RotationalSpeed.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("100 xyz")]
    [InlineData("rpm 100")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(RotationalSpeed.IsValid(input));
    }

    [Theory]
    [InlineData("6000 rpm", 6000)]
    [InlineData("100 rps", 6000)]
    [InlineData("3000", 3000)]
    [InlineData("1 rps", 60)]
    public void TryParse_ReturnsExpected_Rpm(string input, double expectedRpm)
    {
        Assert.True(RotationalSpeed.TryParse(input, out var result));
        Assert.Equal((decimal)expectedRpm, result!.Rpm);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(RotationalSpeed.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("100 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => RotationalSpeed.Parse(input));
    }

    [Fact]
    public void Conversion_RpmToRps()
    {
        var speed = RotationalSpeed.FromRpm(6000m);
        Assert.Equal(100m, speed.Rps);
    }

    [Fact]
    public void Conversion_RpsToRpm()
    {
        var speed = RotationalSpeed.FromRps(100m);
        Assert.Equal(6000m, speed.Rpm);
    }

    [Fact]
    public void Conversion_RpmToRadiansPerSecond()
    {
        var speed = RotationalSpeed.FromRpm(6000m);
        var radPerSec = speed.RadiansPerSecond;
        Assert.InRange(radPerSec, 628.3m, 628.4m);
    }

    [Fact]
    public void Conversion_RadiansPerSecondToRpm()
    {
        var speed = RotationalSpeed.FromRadiansPerSecond(628.318m);
        Assert.InRange(speed.Rpm, 5999m, 6001m);
    }

    [Theory]
    [InlineData("5200 rpm", "5200 rpm")]
    [InlineData("100 rps", "100 rps")]
    [InlineData("  5200  rpm  ", "5200 rpm")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, RotationalSpeed.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("100 rps", RotationalSpeed.Format("6000 rpm", unit: RotationalSpeedUnit.Rps));
        Assert.Equal("6000 rpm", RotationalSpeed.Format("100 rps", unit: RotationalSpeedUnit.Rpm));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("5201 rpm", RotationalSpeed.Format("5200.567 rpm", decimals: 0));
        Assert.Equal("5200.6 rpm", RotationalSpeed.Format("5200.567 rpm", decimals: 1));
    }

    [Fact]
    public void Format_FallbackToTrimmedInput_WhenInvalid()
    {
        Assert.Equal("abc", RotationalSpeed.Format("  abc  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(RotationalSpeed.Format("abc"));
    }

    [Theory]
    [InlineData("5200 rpm", "5200 rpm")]
    [InlineData("100 rps", "6000 rpm")]
    [InlineData("5200", "5200 rpm")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, RotationalSpeed.Normalize(input));
    }

    [Theory]
    [InlineData("5200 rpm", true)]
    [InlineData("100 rps", false)]
    [InlineData("  5200  rpm  ", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, RotationalSpeed.IsNormalized(input));
    }

    [Theory]
    [InlineData("5200 rpm", "5200 rpm")]
    [InlineData("100 rps", "100 rps")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var speed = RotationalSpeed.Parse(input);
        Assert.Equal(expected, speed.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var speed = RotationalSpeed.FromRpm(6000m);
        Assert.Equal("100 rps", speed.ToString(RotationalSpeedUnit.Rps));
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var speed = RotationalSpeed.FromRpm(6000m);
        Assert.Equal(100m, speed.In(RotationalSpeedUnit.Rps));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var speed = RotationalSpeed.Parse("100 rps");
        Assert.Same(RotationalSpeedUnit.Rps, speed.OriginalUnit);
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = RotationalSpeed.FromRpm(1000);
        var b = RotationalSpeed.FromRpm(2000);
        Assert.Equal(3000m, (a + b).Rpm);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = RotationalSpeed.FromRpm(3000);
        var b = RotationalSpeed.FromRpm(1000);
        Assert.Equal(2000m, (a - b).Rpm);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = RotationalSpeed.FromRpm(1000);
        Assert.Equal(3000m, (a * 3).Rpm);
        Assert.Equal(3000m, (3 * a).Rpm);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = RotationalSpeed.FromRpm(3000);
        Assert.Equal(1000m, (a / 3).Rpm);
    }

    [Fact]
    public void Arithmetic_UnaryNegation()
    {
        var a = RotationalSpeed.FromRpm(1000);
        Assert.Equal(-1000m, (-a).Rpm);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = RotationalSpeed.FromRpm(1000);
        var b = RotationalSpeed.FromRpm(2000);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a <= b);
        Assert.True(b >= a);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equality_SameValue()
    {
        var a = RotationalSpeed.FromRpm(6000);
        var b = RotationalSpeed.FromRps(100);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = RotationalSpeed.Parse("1000 rpm");
        var b = RotationalSpeed.Parse("2000 rpm");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = RotationalSpeed.Parse("1000 rpm");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(RotationalSpeed.TryParse("99999999999999999999999999999 rps", out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("5,5 rpm", 5.5)]
    [InlineData("1 000 rpm", 1000)]
    public void TryParse_HandlesEuropeanNumberFormats(string input, double expectedRpm)
    {
        Assert.True(RotationalSpeed.TryParse(input, out var result));
        Assert.Equal((decimal)expectedRpm, result!.Rpm);
    }

    [Theory]
    [InlineData("rpm", "rpm")]
    [InlineData("RPM", "rpm")]
    [InlineData("r/min", "rpm")]
    [InlineData("rev/min", "rpm")]
    [InlineData("varv/min", "rpm")]
    [InlineData("v/min", "rpm")]
    [InlineData("vpm", "rpm")]
    [InlineData("rps", "rps")]
    [InlineData("RPS", "rps")]
    [InlineData("r/s", "rps")]
    [InlineData("rev/s", "rps")]
    [InlineData("varv/s", "rps")]
    [InlineData("rad/s", "rad/s")]
    [InlineData("rad/sec", "rad/s")]
    [InlineData("revolution per minute", "rpm")]
    [InlineData("revolutions per minute", "rpm")]
    [InlineData("varv per minut", "rpm")]
    [InlineData("revolution per second", "rps")]
    [InlineData("revolutions per second", "rps")]
    [InlineData("varv per sekund", "rps")]
    [InlineData("radian per second", "rad/s")]
    [InlineData("radianer per sekund", "rad/s")]
    public void RotationalSpeedUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(RotationalSpeedUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("xyz")]
    public void RotationalSpeedUnit_TryParse_ReturnsFalse_ForInvalid(string? input)
    {
        Assert.False(RotationalSpeedUnit.TryParse(input, out _));
    }

    [Fact]
    public void RotationalSpeedUnit_Parse_Throws_ForInvalid()
    {
        Assert.Throws<ArgumentException>(() => RotationalSpeedUnit.Parse("xyz"));
    }

    [Fact]
    public void RotationalSpeedUnit_All_ContainsAllUnits()
    {
        Assert.Equal(3, RotationalSpeedUnit.All.Count);
        Assert.Contains(RotationalSpeedUnit.Rpm, RotationalSpeedUnit.All);
        Assert.Contains(RotationalSpeedUnit.Rps, RotationalSpeedUnit.All);
        Assert.Contains(RotationalSpeedUnit.RadiansPerSecond, RotationalSpeedUnit.All);
    }

    [Fact]
    public void RotationalSpeedUnit_BaseUnit_IsRpm()
    {
        Assert.Same(RotationalSpeedUnit.Rpm, RotationalSpeedUnit.BaseUnit);
    }
}
