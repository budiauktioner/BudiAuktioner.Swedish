using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class TemperatureTests
{
    [Theory]
    [InlineData("20 °C")]
    [InlineData("68 °F")]
    [InlineData("293.15 K")]
    [InlineData("-5 C")]
    [InlineData("100 grader")]
    public void TryParse_AcceptsCommonInputs(string input)
    {
        Assert.True(Temperature.TryParse(input, out var t));
        Assert.NotNull(t);
    }

    [Fact]
    public void TryParse_PreservesOriginalUnit_Celsius()
    {
        Assert.True(Temperature.TryParse("20 °C", out var t));
        Assert.Same(TemperatureUnit.Celsius, t!.OriginalUnit);
        Assert.Equal(20m, t.In(TemperatureUnit.Celsius));
    }

    [Fact]
    public void TryParse_PreservesOriginalUnit_Fahrenheit()
    {
        Assert.True(Temperature.TryParse("68 °F", out var t));
        Assert.Same(TemperatureUnit.Fahrenheit, t!.OriginalUnit);
        Assert.Equal(68m, t.In(TemperatureUnit.Fahrenheit));
    }

    [Fact]
    public void TryParse_PreservesOriginalUnit_Kelvin()
    {
        Assert.True(Temperature.TryParse("293.15 K", out var t));
        Assert.Same(TemperatureUnit.Kelvin, t!.OriginalUnit);
        Assert.Equal(293.15m, t.In(TemperatureUnit.Kelvin));
    }

    [Fact]
    public void Conversions_WaterReferencePoints()
    {
        var ice = Temperature.FromCelsius(0);
        Assert.Equal(273.15m, ice.Kelvin);
        Assert.Equal(32m, ice.Fahrenheit);

        var boil = Temperature.FromCelsius(100);
        Assert.Equal(373.15m, boil.Kelvin);
        Assert.Equal(212m, boil.Fahrenheit);
    }

    [Fact]
    public void Arithmetic_TemperaturePlusDelta()
    {
        var t = Temperature.FromCelsius(20);
        var d = TemperatureDelta.FromKelvin(5);
        Assert.Equal(25m, (t + d).Celsius);
        Assert.Same(TemperatureUnit.Celsius, (t + d).OriginalUnit);
    }

    [Fact]
    public void Arithmetic_TemperatureMinusDelta()
    {
        var t = Temperature.FromCelsius(20);
        var d = TemperatureDelta.FromCelsius(3);
        Assert.Equal(17m, (t - d).Celsius);
    }

    [Fact]
    public void Arithmetic_TemperatureMinusTemperature_IsDelta()
    {
        var a = Temperature.FromCelsius(25);
        var b = Temperature.FromCelsius(20);
        var d = a - b;
        Assert.Equal(5m, d.Kelvin);
        Assert.Equal(5m, d.Celsius);
        Assert.Equal(9m, d.Fahrenheit);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var cold = Temperature.FromCelsius(0);
        var hot = Temperature.FromCelsius(30);
        Assert.True(cold < hot);
        Assert.True(hot > cold);
        Assert.True(cold <= hot);
        Assert.True(hot >= cold);
        Assert.False(cold == hot);
        Assert.True(cold != hot);
    }

    [Fact]
    public void Equality_SameThermodynamicState()
    {
        var a = Temperature.FromCelsius(0);
        var b = Temperature.FromFahrenheit(32);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void FindCandidatesInText_FindsTemperatureValues()
    {
        var text = "Outside it is 20 °C but the oven runs at 400 F.";
        var candidates = Temperature.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var t = Temperature.Parse("22 °C");
        Assert.Equal("*** °C", t.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForKelvinBaseForm()
    {
        Assert.True(Temperature.IsNormalized("273.15 K"));
    }

    [Fact]
    public void IsNormalized_FalseForCelsius()
    {
        Assert.False(Temperature.IsNormalized("0 °C"));
    }

    [Theory]
    [InlineData("°C", "°C")]
    [InlineData("C", "°C")]
    [InlineData("celsius", "°C")]
    [InlineData("grader", "°C")]
    [InlineData("°F", "°F")]
    [InlineData("F", "°F")]
    [InlineData("fahrenheit", "°F")]
    [InlineData("K", "K")]
    [InlineData("kelvin", "K")]
    [InlineData("degrees Celsius", "°C")]
    [InlineData("grader Celsius", "°C")]
    [InlineData("degrees Fahrenheit", "°F")]
    [InlineData("grader Fahrenheit", "°F")]
    [InlineData("degree Celsius", "°C")]
    [InlineData("degree Fahrenheit", "°F")]
    public void TemperatureUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(TemperatureUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Fact]
    public void TemperatureDelta_FahrenheitInterval_ScalesByNineFifths()
    {
        var d = TemperatureDelta.FromKelvin(10);
        Assert.Equal(18m, d.Fahrenheit);
    }

    [Theory]
    [InlineData("5,5 °C")]
    [InlineData("2.5 °F")]
    [InlineData("0,5 K")]
    [InlineData("3.14 °C")]
    [InlineData("-5,5 °C")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Temperature.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 °C", 278.65)]
    [InlineData("-5,5 °C", 267.65)]
    [InlineData("3.14 °C", 276.29)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedKelvin)
    {
        Assert.True(Temperature.TryParse(input, out var result));
        Assert.Equal((decimal)expectedKelvin, result!.Kelvin);
    }

    [Theory]
    [InlineData("5,5 °C", "5.5 °C")]
    [InlineData("  20  °C  ", "20 °C")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Temperature.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("32 °F", Temperature.Format("0 °C", unit: TemperatureUnit.Fahrenheit));
        Assert.Equal("0 °C", Temperature.Format("32 °F", unit: TemperatureUnit.Celsius));
        Assert.Equal("20 °C", Temperature.Format("20 °C"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("21 °C", Temperature.Format("20.567 °C", decimals: 0));
        Assert.Equal("20.6 °C", Temperature.Format("20.567 °C", decimals: 1));
        Assert.Equal("20.567 °C", Temperature.Format("20.567 °C"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var t = Temperature.Parse("20.567 °C");
        Assert.Equal("21 °C", t.ToString(TemperatureUnit.Celsius, decimals: 0));
        Assert.Equal("20.6 °C", t.ToString(TemperatureUnit.Celsius, decimals: 1));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Temperature.FromCelsius(20.5m);
        var delta = TemperatureDelta.FromCelsius(1.5m);
        Assert.Equal(22m, (a + delta).Celsius);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Temperature.TryParse("99999999999999999999999999999 °F", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Temperature.Parse("10 °C");
        var b = Temperature.Parse("20 °C");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Temperature.Parse("10 °C");
        Assert.Equal(1, a.CompareTo(null));
    }
}
