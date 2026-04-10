using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class PowerTests
{
    [Theory]
    [InlineData("100 W")]
    [InlineData("2.5 kW")]
    [InlineData("500 mW")]
    [InlineData("1 MW")]
    [InlineData("0.001 GW")]
    [InlineData("150 HP")]
    [InlineData("200 hp")]
    [InlineData("100 hk")]
    [InlineData("50 hästkraft")]
    [InlineData("100 µW")]
    [InlineData("5 TW")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Power.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("100")]
    [InlineData("100 xyz")]
    [InlineData("kW 100")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Power.IsValid(input));
    }

    [Theory]
    [InlineData("100 W", 100)]
    [InlineData("1 kW", 1000)]
    [InlineData("1000 mW", 1)]
    [InlineData("1 MW", 1_000_000)]
    [InlineData("1 GW", 1_000_000_000)]
    public void TryParse_ReturnsExpected_Watts(string input, double expectedWatts)
    {
        Assert.True(Power.TryParse(input, out var result));
        Assert.Equal((decimal)expectedWatts, result!.Watts);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Power.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("100 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Power.Parse(input));
    }

    [Theory]
    [InlineData("2.5 kW", "2.5 kW")]
    [InlineData("100 mW", "100 mW")]
    [InlineData("5.5 W", "5.5 W")]
    [InlineData("100 µW", "100 µW")]
    [InlineData("5 TW", "5 TW")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Power.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 W", Power.Format("1 kW", unit: PowerUnit.Watt));
        Assert.Equal("1 kW", Power.Format("1000 W", unit: PowerUnit.Kilowatt));
        Assert.Equal("1 kW", Power.Format("1 kW"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("3 kW", Power.Format("2.567 kW", decimals: 0));
        Assert.Equal("2.6 kW", Power.Format("2.567 kW", decimals: 1));
        Assert.Equal("2.57 kW", Power.Format("2.567 kW", decimals: 2));
        Assert.Equal("2.567 kW", Power.Format("2.567 kW"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var p = Power.Parse("2.567 kW");
        Assert.Equal("3 kW", p.ToString(PowerUnit.Kilowatt, decimals: 0));
        Assert.Equal("2567 W", p.ToString(PowerUnit.Watt, decimals: 0));
    }

    [Theory]
    [InlineData("2.5 kW", "2500 W")]
    [InlineData("1000 mW", "1 W")]
    [InlineData("5 W", "5 W")]
    [InlineData("1 MW", "1000000 W")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Power.Normalize(input));
    }

    [Theory]
    [InlineData("2.5 kW", "2.5 kW")]
    [InlineData("5 W", "5 W")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var power = Power.Parse(input);
        Assert.Equal(expected, power.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var power = Power.FromKilowatts(1);
        Assert.Equal("1000 W", power.ToString(PowerUnit.Watt));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var power = Power.FromWatts(1000);
        Assert.Equal(1000m, power.Watts);
        Assert.Equal(1m, power.Kilowatts);
        Assert.Equal(1_000_000m, power.Milliwatts);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var power = Power.FromKilowatts(1);
        Assert.Equal(1000m, power.In(PowerUnit.Watt));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Power.FromWatts(100);
        var b = Power.FromWatts(200);
        Assert.Equal(300m, (a + b).Watts);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Power.FromWatts(200);
        var b = Power.FromWatts(50);
        Assert.Equal(150m, (a - b).Watts);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Power.FromWatts(100);
        Assert.Equal(300m, (a * 3).Watts);
        Assert.Equal(300m, (3 * a).Watts);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Power.FromWatts(300);
        Assert.Equal(100m, (a / 3).Watts);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Power.FromWatts(100);
        var b = Power.FromWatts(200);
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
        var a = Power.FromWatts(1000);
        var b = Power.FromKilowatts(1);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equality_OneHorsepower_InWatts()
    {
        var a = Power.FromHorsepower(1);
        var b = Power.FromWatts(745.69987158227022m);
        Assert.True(a == b);
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var power = Power.Parse("2.5 kW");
        Assert.Same(PowerUnit.Kilowatt, power.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsPowerValues()
    {
        var text = "The motor is rated at 2.5 kW and the plant outputs 10 MW.";
        var candidates = Power.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var power = Power.Parse("2.5 kW");
        Assert.Equal("*** kW", power.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Power.IsNormalized("5 W"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Power.IsNormalized("5 kW"));
    }

    [Theory]
    [InlineData("W", "W")]
    [InlineData("kW", "kW")]
    [InlineData("mW", "mW")]
    [InlineData("MW", "MW")]
    [InlineData("GW", "GW")]
    [InlineData("kilowatt", "kW")]
    [InlineData("hk", "HP")]
    [InlineData("hästkraft", "HP")]
    [InlineData("hp", "HP")]
    [InlineData("watts", "W")]
    [InlineData("kilowatts", "kW")]
    [InlineData("milliwatts", "mW")]
    [InlineData("megawatts", "MW")]
    [InlineData("gigawatts", "GW")]
    [InlineData("hästkrafter", "HP")]
    [InlineData("horsepower", "HP")]
    [InlineData("µW", "µW")]
    [InlineData("uW", "µW")]
    [InlineData("microwatt", "µW")]
    [InlineData("microwatts", "µW")]
    [InlineData("mikrowatt", "µW")]
    [InlineData("TW", "TW")]
    [InlineData("terawatt", "TW")]
    [InlineData("terawatts", "TW")]
    public void PowerUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(PowerUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 kW")]
    [InlineData("2.5 MW")]
    [InlineData("0,5 HP")]
    [InlineData("3.14 W")]
    [InlineData("1 000 W")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Power.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 kW", 5500)]
    [InlineData("2.5 MW", 2500000)]
    [InlineData("1 000 W", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedWatts)
    {
        Assert.True(Power.TryParse(input, out var result));
        Assert.Equal((decimal)expectedWatts, result!.Watts);
    }

    [Theory]
    [InlineData("5,5 kW", "5.5 kW")]
    [InlineData("  10  W  ", "10 W")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Power.Format(input));
    }

    [Theory]
    [InlineData("2.5 kW", "2500 W")]
    [InlineData("5,5 W", "5.5 W")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Power.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Power.FromKilowatts(1.5m);
        var b = Power.FromWatts(500);
        Assert.Equal(2000m, (a + b).Watts);
    }

    [Fact]
    public void Conversions_MicrowattToWatt()
    {
        var power = Power.Parse("1000000 µW");
        Assert.Equal(1m, power.Watts);
    }

    [Fact]
    public void Conversions_TerawattToWatt()
    {
        var power = Power.Parse("1 TW");
        Assert.Equal(1_000_000_000_000m, power.Watts);
    }

    [Fact]
    public void FromFactory_Microwatts()
    {
        var power = Power.FromMicrowatts(500);
        Assert.Equal(500m, power.Microwatts);
    }

    [Fact]
    public void FromFactory_Terawatts()
    {
        var power = Power.FromTerawatts(2);
        Assert.Equal(2m, power.Terawatts);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Power.TryParse("99999999999999999 TW", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Power.Parse("100 W");
        var b = Power.Parse("200 W");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Power.Parse("100 W");
        Assert.Equal(1, a.CompareTo(null));
    }
}
