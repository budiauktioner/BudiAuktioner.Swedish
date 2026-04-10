using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class VoltageTests
{
    [Theory]
    [InlineData("3.3 V")]
    [InlineData("500 mV")]
    [InlineData("230 kV")]
    [InlineData("1 V")]
    [InlineData("1000 mV")]
    [InlineData("100 µV")]
    [InlineData("5 MV")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Voltage.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("kV 10")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Voltage.IsValid(input));
    }

    [Theory]
    [InlineData("3.3 V", 3.3)]
    [InlineData("1 kV", 1000)]
    [InlineData("1000 mV", 1)]
    [InlineData("500 mV", 0.5)]
    public void TryParse_ReturnsExpected_Volts(string input, double expectedVolts)
    {
        Assert.True(Voltage.TryParse(input, out var result));
        Assert.Equal((decimal)expectedVolts, result!.Volts);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Voltage.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Voltage.Parse(input));
    }

    [Theory]
    [InlineData("230 kV", "230 kV")]
    [InlineData("500 mV", "500 mV")]
    [InlineData("3.3 V", "3.3 V")]
    [InlineData("100 µV", "100 µV")]
    [InlineData("5 MV", "5 MV")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Voltage.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 V", Voltage.Format("1 kV", unit: VoltageUnit.Volt));
        Assert.Equal("1 kV", Voltage.Format("1000 V", unit: VoltageUnit.Kilovolt));
        Assert.Equal("1 kV", Voltage.Format("1 kV"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("231 V", Voltage.Format("230.567 V", decimals: 0));
        Assert.Equal("230.6 V", Voltage.Format("230.567 V", decimals: 1));
        Assert.Equal("230.57 V", Voltage.Format("230.567 V", decimals: 2));
        Assert.Equal("230.567 V", Voltage.Format("230.567 V"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var v = Voltage.Parse("230.567 V");
        Assert.Equal("231 V", v.ToString(VoltageUnit.Volt, decimals: 0));
        Assert.Equal("0.2 kV", v.ToString(VoltageUnit.Kilovolt, decimals: 1));
    }

    [Theory]
    [InlineData("230 kV", "230000 V")]
    [InlineData("500 mV", "0.5 V")]
    [InlineData("5 V", "5 V")]
    [InlineData("1000 mV", "1 V")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Voltage.Normalize(input));
    }

    [Theory]
    [InlineData("230 kV", "230 kV")]
    [InlineData("5 V", "5 V")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var voltage = Voltage.Parse(input);
        Assert.Equal(expected, voltage.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var voltage = Voltage.FromKilovolts(1);
        Assert.Equal("1000 V", voltage.ToString(VoltageUnit.Volt));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var voltage = Voltage.FromVolts(1);
        Assert.Equal(1m, voltage.Volts);
        Assert.Equal(1000m, voltage.Millivolts);
        Assert.Equal(0.001m, voltage.Kilovolts);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var voltage = Voltage.FromKilovolts(1);
        Assert.Equal(1000m, voltage.In(VoltageUnit.Volt));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Voltage.FromVolts(10);
        var b = Voltage.FromVolts(20);
        Assert.Equal(30m, (a + b).Volts);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Voltage.FromVolts(20);
        var b = Voltage.FromVolts(5);
        Assert.Equal(15m, (a - b).Volts);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Voltage.FromVolts(10);
        Assert.Equal(30m, (a * 3).Volts);
        Assert.Equal(30m, (3 * a).Volts);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Voltage.FromVolts(30);
        Assert.Equal(10m, (a / 3).Volts);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Voltage.FromVolts(10);
        var b = Voltage.FromVolts(20);
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
        var a = Voltage.FromVolts(10);
        var b = Voltage.FromMillivolts(10000);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var voltage = Voltage.Parse("5 kV");
        Assert.Same(VoltageUnit.Kilovolt, voltage.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsVoltageValues()
    {
        var text = "The bus uses 230 kV and the board runs at 3.3 V.";
        var candidates = Voltage.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var voltage = Voltage.Parse("230 kV");
        Assert.Equal("*** kV", voltage.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Voltage.IsNormalized("5 V"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Voltage.IsNormalized("5 kV"));
    }

    [Theory]
    [InlineData("V", "V")]
    [InlineData("kV", "kV")]
    [InlineData("mV", "mV")]
    [InlineData("volt", "V")]
    [InlineData("kilovolt", "kV")]
    [InlineData("millivolt", "mV")]
    [InlineData("volts", "V")]
    [InlineData("kilovolts", "kV")]
    [InlineData("millivolts", "mV")]
    [InlineData("µV", "µV")]
    [InlineData("uV", "µV")]
    [InlineData("microvolt", "µV")]
    [InlineData("microvolts", "µV")]
    [InlineData("mikrovolt", "µV")]
    [InlineData("MV", "MV")]
    [InlineData("megavolt", "MV")]
    [InlineData("megavolts", "MV")]
    public void VoltageUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(VoltageUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 V")]
    [InlineData("2.5 kV")]
    [InlineData("0,5 mV")]
    [InlineData("3.14 V")]
    [InlineData("1 000 mV")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Voltage.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 V", 5.5)]
    [InlineData("2.5 kV", 2500)]
    [InlineData("1 000 mV", 1)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedVolts)
    {
        Assert.True(Voltage.TryParse(input, out var result));
        Assert.Equal((decimal)expectedVolts, result!.Volts);
    }

    [Theory]
    [InlineData("5,5 V", "5.5 V")]
    [InlineData("  230  V  ", "230 V")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Voltage.Format(input));
    }

    [Theory]
    [InlineData("2.5 kV", "2500 V")]
    [InlineData("5,5 V", "5.5 V")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Voltage.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Voltage.FromVolts(1.5m);
        var b = Voltage.FromMillivolts(500);
        Assert.Equal(2m, (a + b).Volts);
    }

    [Fact]
    public void Conversions_MicrovoltToVolt()
    {
        var voltage = Voltage.Parse("1000000 µV");
        Assert.Equal(1m, voltage.Volts);
    }

    [Fact]
    public void Conversions_MegavoltToVolt()
    {
        var voltage = Voltage.Parse("1 MV");
        Assert.Equal(1_000_000m, voltage.Volts);
    }

    [Fact]
    public void FromFactory_Microvolts()
    {
        var voltage = Voltage.FromMicrovolts(500);
        Assert.Equal(500m, voltage.Microvolts);
    }

    [Fact]
    public void FromFactory_Megavolts()
    {
        var voltage = Voltage.FromMegavolts(2);
        Assert.Equal(2m, voltage.Megavolts);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Voltage.TryParse("99999999999999999999999999 MV", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Voltage.Parse("12 V");
        var b = Voltage.Parse("220 V");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Voltage.Parse("12 V");
        Assert.Equal(1, a.CompareTo(null));
    }
}
