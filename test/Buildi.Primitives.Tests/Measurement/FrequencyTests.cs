using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class FrequencyTests
{
    [Theory]
    [InlineData("60 Hz")]
    [InlineData("2.4 GHz")]
    [InlineData("100 kHz")]
    [InlineData("50 MHz")]
    [InlineData("3000 rpm")]
    [InlineData("60 rev/min")]
    [InlineData("1800 varv/min")]
    [InlineData("60 r/min")]
    [InlineData("1 THz")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Frequency.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("60")]
    [InlineData("60 xyz")]
    [InlineData("Hz 60")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Frequency.IsValid(input));
    }

    [Theory]
    [InlineData("60 Hz", 60)]
    [InlineData("1 kHz", 1000)]
    [InlineData("1 MHz", 1_000_000)]
    [InlineData("1 GHz", 1_000_000_000)]
    [InlineData("60 rpm", 1)]
    public void TryParse_ReturnsExpected_Hertz(string input, double expectedHertz)
    {
        Assert.True(Frequency.TryParse(input, out var result));
        Assert.Equal((decimal)expectedHertz, result!.Hertz);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Frequency.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("60 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Frequency.Parse(input));
    }

    [Theory]
    [InlineData("2.4 GHz", "2.4 GHz")]
    [InlineData("100 kHz", "100 kHz")]
    [InlineData("50 Hz", "50 Hz")]
    [InlineData("1 THz", "1 THz")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Frequency.Format(input));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("3 kHz", Frequency.Format("2.567 kHz", decimals: 0));
        Assert.Equal("2.6 kHz", Frequency.Format("2.567 kHz", decimals: 1));
        Assert.Equal("2.567 kHz", Frequency.Format("2.567 kHz"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var f = Frequency.Parse("2.567 kHz");
        Assert.Equal("3 kHz", f.ToString(FrequencyUnit.Kilohertz, decimals: 0));
        Assert.Equal("2567 Hz", f.ToString(FrequencyUnit.Hertz, decimals: 0));
    }

    [Theory]
    [InlineData("1 kHz", "1000 Hz")]
    [InlineData("60 rpm", "1 Hz")]
    [InlineData("50 Hz", "50 Hz")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Frequency.Normalize(input));
    }

    [Theory]
    [InlineData("2.4 GHz", "2.4 GHz")]
    [InlineData("50 Hz", "50 Hz")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var frequency = Frequency.Parse(input);
        Assert.Equal(expected, frequency.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var frequency = Frequency.FromGigahertz(1);
        Assert.Equal("1000 MHz", frequency.ToString(FrequencyUnit.Megahertz));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var frequency = Frequency.FromHertz(60);
        Assert.Equal(60m, frequency.Hertz);
        Assert.Equal(0.06m, frequency.Kilohertz);
        Assert.Equal(3600m, frequency.Rpm);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var frequency = Frequency.FromKilohertz(1);
        Assert.Equal(0.001m, frequency.In(FrequencyUnit.Megahertz));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Frequency.FromHertz(10);
        var b = Frequency.FromHertz(20);
        Assert.Equal(30m, (a + b).Hertz);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Frequency.FromHertz(20);
        var b = Frequency.FromHertz(5);
        Assert.Equal(15m, (a - b).Hertz);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Frequency.FromHertz(10);
        Assert.Equal(30m, (a * 3).Hertz);
        Assert.Equal(30m, (3 * a).Hertz);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Frequency.FromHertz(30);
        Assert.Equal(10m, (a / 3).Hertz);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Frequency.FromHertz(10);
        var b = Frequency.FromHertz(20);
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
        var a = Frequency.FromHertz(60);
        var b = Frequency.FromRpm(3600);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var frequency = Frequency.Parse("2.4 GHz");
        Assert.Same(FrequencyUnit.Gigahertz, frequency.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsFrequencyValues()
    {
        var text = "CPU runs at 2.4 GHz and the motor does 3000 rpm.";
        var candidates = Frequency.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var frequency = Frequency.Parse("100 kHz");
        Assert.Equal("*** kHz", frequency.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Frequency.IsNormalized("50 Hz"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Frequency.IsNormalized("1 kHz"));
    }

    [Theory]
    [InlineData("Hz", "Hz")]
    [InlineData("kHz", "kHz")]
    [InlineData("hertz", "Hz")]
    [InlineData("kilohertz", "kHz")]
    [InlineData("rev/min", "rpm")]
    [InlineData("varv/min", "rpm")]
    [InlineData("r/min", "rpm")]
    [InlineData("megahertz", "MHz")]
    [InlineData("gigahertz", "GHz")]
    [InlineData("RPM", "rpm")]
    [InlineData("revolutions per minute", "rpm")]
    [InlineData("varv per minut", "rpm")]
    [InlineData("THz", "THz")]
    [InlineData("terahertz", "THz")]
    public void FrequencyUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(FrequencyUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 kHz")]
    [InlineData("2.5 GHz")]
    [InlineData("0,5 MHz")]
    [InlineData("3.14 Hz")]
    [InlineData("1 000 Hz")]
    [InlineData("3 000 RPM")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Frequency.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 kHz", 5500)]
    [InlineData("2.5 GHz", 2500000000)]
    [InlineData("1 000 Hz", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedHz)
    {
        Assert.True(Frequency.TryParse(input, out var result));
        Assert.Equal((decimal)expectedHz, result!.Hertz);
    }

    [Theory]
    [InlineData("5,5 kHz", "5.5 kHz")]
    [InlineData("  50  Hz  ", "50 Hz")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Frequency.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 Hz", Frequency.Format("1 kHz", unit: FrequencyUnit.Hertz));
        Assert.Equal("1 kHz", Frequency.Format("1000 Hz", unit: FrequencyUnit.Kilohertz));
        Assert.Equal("1 kHz", Frequency.Format("1 kHz"));
    }

    [Theory]
    [InlineData("2.5 kHz", "2500 Hz")]
    [InlineData("5,5 Hz", "5.5 Hz")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Frequency.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Frequency.FromKilohertz(1.5m);
        var b = Frequency.FromHertz(500);
        Assert.Equal(2000m, (a + b).Hertz);
    }

    [Fact]
    public void Conversions_TerahertzToHertz()
    {
        var freq = Frequency.Parse("1 THz");
        Assert.Equal(1_000_000_000_000m, freq.Hertz);
    }

    [Fact]
    public void FromFactory_Terahertz()
    {
        var freq = Frequency.FromTerahertz(2);
        Assert.Equal(2m, freq.Terahertz);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Frequency.TryParse("99999999999999999 THz", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Frequency.Parse("1 GHz");
        var b = Frequency.Parse("2 GHz");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Frequency.Parse("1 GHz");
        Assert.Equal(1, a.CompareTo(null));
    }
}
