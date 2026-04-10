using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class PressureTests
{
    [Theory]
    [InlineData("101325 Pa")]
    [InlineData("1013 hPa")]
    [InlineData("100 kPa")]
    [InlineData("1 bar")]
    [InlineData("1013 mbar")]
    [InlineData("14.7 PSI")]
    [InlineData("1 atm")]
    [InlineData("760 mmHg")]
    [InlineData("760 mm Hg")]
    [InlineData("760 torr")]
    [InlineData("15 psi")]
    [InlineData("30 lb/in²")]
    [InlineData("200 MPa")]
    [InlineData("1 GPa")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Pressure.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("kPa 10")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Pressure.IsValid(input));
    }

    [Theory]
    [InlineData("1 bar", 100_000)]
    [InlineData("1013 hPa", 101_300)]
    [InlineData("100 kPa", 100_000)]
    [InlineData("1 atm", 101_325)]
    public void TryParse_ReturnsExpected_Pascals(string input, double expectedPascals)
    {
        Assert.True(Pressure.TryParse(input, out var result));
        Assert.Equal((decimal)expectedPascals, result!.Pascals);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Pressure.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Pressure.Parse(input));
    }

    [Theory]
    [InlineData("1013 hPa", "1013 hPa")]
    [InlineData("1 bar", "1 bar")]
    [InlineData("100 kPa", "100 kPa")]
    [InlineData("200 MPa", "200 MPa")]
    [InlineData("1 GPa", "1 GPa")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Pressure.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("100000 Pa", Pressure.Format("1 bar", unit: PressureUnit.Pascal));
        Assert.Equal("1 bar", Pressure.Format("100000 Pa", unit: PressureUnit.Bar));
        Assert.Equal("1 bar", Pressure.Format("1 bar"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("2 bar", Pressure.Format("1.567 bar", decimals: 0));
        Assert.Equal("1.6 bar", Pressure.Format("1.567 bar", decimals: 1));
        Assert.Equal("1.57 bar", Pressure.Format("1.567 bar", decimals: 2));
        Assert.Equal("1.567 bar", Pressure.Format("1.567 bar"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var p = Pressure.Parse("1.567 bar");
        Assert.Equal("2 bar", p.ToString(PressureUnit.Bar, decimals: 0));
        Assert.Equal("1.6 bar", p.ToString(PressureUnit.Bar, decimals: 1));
    }

    [Theory]
    [InlineData("1013 hPa", "101300 Pa")]
    [InlineData("1 bar", "100000 Pa")]
    [InlineData("100 Pa", "100 Pa")]
    [InlineData("100 kPa", "100000 Pa")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Pressure.Normalize(input));
    }

    [Theory]
    [InlineData("1013 hPa", "1013 hPa")]
    [InlineData("1 bar", "1 bar")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var pressure = Pressure.Parse(input);
        Assert.Equal(expected, pressure.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var pressure = Pressure.FromBars(1);
        Assert.Equal("100000 Pa", pressure.ToString(PressureUnit.Pascal));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var pressure = Pressure.FromBars(1);
        Assert.Equal(100_000m, pressure.Pascals);
        Assert.Equal(1000m, pressure.Millibars);
        Assert.Equal(1m, pressure.Bars);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var pressure = Pressure.FromKilopascals(100);
        Assert.Equal(100_000m, pressure.In(PressureUnit.Pascal));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Pressure.FromPascals(10_000);
        var b = Pressure.FromPascals(20_000);
        Assert.Equal(30_000m, (a + b).Pascals);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Pressure.FromPascals(20_000);
        var b = Pressure.FromPascals(5_000);
        Assert.Equal(15_000m, (a - b).Pascals);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Pressure.FromPascals(10_000);
        Assert.Equal(30_000m, (a * 3).Pascals);
        Assert.Equal(30_000m, (3 * a).Pascals);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Pressure.FromPascals(30_000);
        Assert.Equal(10_000m, (a / 3).Pascals);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Pressure.FromPascals(10_000);
        var b = Pressure.FromPascals(20_000);
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
        var a = Pressure.FromBars(1);
        var b = Pressure.FromPascals(100_000);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var pressure = Pressure.Parse("1013 hPa");
        Assert.Same(PressureUnit.Hectopascal, pressure.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsPressureValues()
    {
        var text = "Tyre pressure 2.5 bar and weather 1013 hPa.";
        var candidates = Pressure.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var pressure = Pressure.Parse("1013 hPa");
        Assert.Equal("*** hPa", pressure.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Pressure.IsNormalized("100 Pa"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Pressure.IsNormalized("1013 hPa"));
    }

    [Theory]
    [InlineData("Pa", "Pa")]
    [InlineData("hPa", "hPa")]
    [InlineData("kPa", "kPa")]
    [InlineData("bar", "bar")]
    [InlineData("mbar", "mbar")]
    [InlineData("PSI", "PSI")]
    [InlineData("psi", "PSI")]
    [InlineData("atm", "atm")]
    [InlineData("mmHg", "mmHg")]
    [InlineData("mm Hg", "mmHg")]
    [InlineData("torr", "mmHg")]
    [InlineData("hektopascal", "hPa")]
    [InlineData("atmosfär", "atm")]
    [InlineData("lb/in²", "PSI")]
    [InlineData("pascals", "Pa")]
    [InlineData("hectopascals", "hPa")]
    [InlineData("kilopascals", "kPa")]
    [InlineData("bars", "bar")]
    [InlineData("millibars", "mbar")]
    [InlineData("atmospheres", "atm")]
    [InlineData("atmosfärer", "atm")]
    [InlineData("pounds per square inch", "PSI")]
    [InlineData("millimeters of mercury", "mmHg")]
    [InlineData("millimetre of mercury", "mmHg")]
    [InlineData("millimetres of mercury", "mmHg")]
    [InlineData("MPa", "MPa")]
    [InlineData("megapascal", "MPa")]
    [InlineData("megapascals", "MPa")]
    [InlineData("GPa", "GPa")]
    [InlineData("gigapascal", "GPa")]
    [InlineData("gigapascals", "GPa")]
    public void PressureUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(PressureUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 bar")]
    [InlineData("2.5 atm")]
    [InlineData("0,5 PSI")]
    [InlineData("3.14 kPa")]
    [InlineData("1 000 Pa")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Pressure.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 bar", 550000)]
    [InlineData("2.5 kPa", 2500)]
    [InlineData("1 000 Pa", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedPascals)
    {
        Assert.True(Pressure.TryParse(input, out var result));
        Assert.Equal((decimal)expectedPascals, result!.Pascals);
    }

    [Theory]
    [InlineData("5,5 bar", "5.5 bar")]
    [InlineData("  1013  hPa  ", "1013 hPa")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Pressure.Format(input));
    }

    [Theory]
    [InlineData("2.5 kPa", "2500 Pa")]
    [InlineData("5,5 Pa", "5.5 Pa")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Pressure.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Pressure.FromBars(1.5m);
        var b = Pressure.FromPascals(50000);
        Assert.Equal(200000m, (a + b).Pascals);
    }

    [Fact]
    public void Conversions_MegapascalToPascal()
    {
        var pressure = Pressure.Parse("1 MPa");
        Assert.Equal(1_000_000m, pressure.Pascals);
    }

    [Fact]
    public void Conversions_GigapascalToPascal()
    {
        var pressure = Pressure.Parse("1 GPa");
        Assert.Equal(1_000_000_000m, pressure.Pascals);
    }

    [Fact]
    public void FromFactory_Megapascals()
    {
        var pressure = Pressure.FromMegapascals(2);
        Assert.Equal(2m, pressure.Megapascals);
    }

    [Fact]
    public void FromFactory_Gigapascals()
    {
        var pressure = Pressure.FromGigapascals(1);
        Assert.Equal(1m, pressure.Gigapascals);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Pressure.TryParse("99999999999999999999999 GPa", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Pressure.Parse("1 bar");
        var b = Pressure.Parse("2 bar");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Pressure.Parse("1 bar");
        Assert.Equal(1, a.CompareTo(null));
    }
}
