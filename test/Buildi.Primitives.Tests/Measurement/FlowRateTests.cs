using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class FlowRateTests
{
    [Theory]
    [InlineData("100 L/min")]
    [InlineData("50 l/min")]
    [InlineData("200 L/h")]
    [InlineData("10 m³/h")]
    [InlineData("5 L/s")]
    [InlineData("2.5 gal/min")]
    [InlineData("0.5 m³/min")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(FlowRate.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(FlowRate.IsValid(input));
    }

    [Theory]
    [InlineData("60 L/min", 60)]
    [InlineData("1 L/s", 60)]
    [InlineData("60 L/h", 1)]
    [InlineData("1 m³/min", 1000)]
    public void TryParse_ReturnsExpected_LitersPerMinute(string input, double expectedLpm)
    {
        Assert.True(FlowRate.TryParse(input, out var result));
        Assert.Equal((decimal)expectedLpm, result!.LitersPerMinute);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(FlowRate.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => FlowRate.Parse(input));
    }

    [Theory]
    [InlineData("100 L/min", "100 L/min")]
    [InlineData("50 l/min", "50 L/min")]
    [InlineData("200 L/h", "200 L/h")]
    [InlineData("10 m³/h", "10 m³/h")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, FlowRate.Format(input));
    }

    [Theory]
    [InlineData("60 L/h", "1 L/min")]
    [InlineData("1 L/s", "60 L/min")]
    [InlineData("100 L/min", "100 L/min")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, FlowRate.Normalize(input));
    }

    [Theory]
    [InlineData("100 L/min", "100 L/min")]
    [InlineData("5 L/s", "5 L/s")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var flow = FlowRate.Parse(input);
        Assert.Equal(expected, flow.ToString());
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var flow = FlowRate.FromLitersPerMinute(60);
        Assert.Equal(60m, flow.LitersPerMinute);
        Assert.Equal(3600m, flow.LitersPerHour);
        Assert.Equal(1m, flow.LitersPerSecond);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var flow = FlowRate.FromLitersPerMinute(100);
        Assert.Equal(6000m, flow.In(FlowRateUnit.LitersPerHour));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = FlowRate.FromLitersPerMinute(10);
        var b = FlowRate.FromLitersPerMinute(20);
        Assert.Equal(30m, (a + b).LitersPerMinute);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = FlowRate.FromLitersPerMinute(10);
        var b = FlowRate.FromLitersPerMinute(20);
        Assert.True(a < b);
        Assert.True(b > a);
    }

    [Fact]
    public void Equality_SameValue()
    {
        var a = FlowRate.FromLitersPerMinute(60);
        var b = FlowRate.FromLitersPerSecond(1);
        Assert.True(a == b);
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(FlowRate.IsNormalized("100 L/min"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(FlowRate.IsNormalized("5 L/s"));
    }

    [Theory]
    [InlineData("L/min", "L/min")]
    [InlineData("l/min", "L/min")]
    [InlineData("L/h", "L/h")]
    [InlineData("l/h", "L/h")]
    [InlineData("L/s", "L/s")]
    [InlineData("m³/h", "m³/h")]
    [InlineData("m3/h", "m³/h")]
    [InlineData("m³/min", "m³/min")]
    [InlineData("gal/min", "gal/min")]
    [InlineData("gpm", "gal/min")]
    public void FlowRateUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(FlowRateUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 L/min")]
    [InlineData("2.5 m³/h")]
    [InlineData("1 000 L/h")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(FlowRate.IsValid(input));
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var flow = FlowRate.Parse("100 L/min");
        Assert.Equal("*** L/min", flow.ToMaskedString());
    }

    [Fact]
    public void CubicMetersPerHour_ConvertsCorrectly()
    {
        var flow = FlowRate.Parse("60 m³/h");
        Assert.Equal(1000m, flow.LitersPerMinute);
    }
}
