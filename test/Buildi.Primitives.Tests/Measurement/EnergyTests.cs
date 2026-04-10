using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class EnergyTests
{
    [Theory]
    [InlineData("100 J")]
    [InlineData("5.5 kJ")]
    [InlineData("2 MJ")]
    [InlineData("10 Wh")]
    [InlineData("1 kWh")]
    [InlineData("0.001 MWh")]
    [InlineData("1000 cal")]
    [InlineData("2 kcal")]
    [InlineData("10 BTU")]
    [InlineData("5 GJ")]
    [InlineData("1 TJ")]
    [InlineData("10 GWh")]
    [InlineData("1 TWh")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Energy.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("100")]
    [InlineData("100 xyz")]
    [InlineData("kWh 1")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Energy.IsValid(input));
    }

    [Theory]
    [InlineData("1 kWh", 3_600_000)]
    [InlineData("1000 Wh", 3_600_000)]
    [InlineData("1 kJ", 1000)]
    [InlineData("1 MJ", 1_000_000)]
    [InlineData("1 J", 1)]
    public void TryParse_ReturnsExpected_Joules(string input, double expectedJoules)
    {
        Assert.True(Energy.TryParse(input, out var result));
        Assert.Equal((decimal)expectedJoules, result!.Joules);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Energy.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("100 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Energy.Parse(input));
    }

    [Theory]
    [InlineData("1 kWh", "1 kWh")]
    [InlineData("100 kJ", "100 kJ")]
    [InlineData("5.5 MJ", "5.5 MJ")]
    [InlineData("5 GJ", "5 GJ")]
    [InlineData("1 TJ", "1 TJ")]
    [InlineData("10 GWh", "10 GWh")]
    [InlineData("1 TWh", "1 TWh")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Energy.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("3600000 J", Energy.Format("1 kWh", unit: EnergyUnit.Joule));
        Assert.Equal("1 kWh", Energy.Format("3600000 J", unit: EnergyUnit.KilowattHour));
        Assert.Equal("1 kWh", Energy.Format("1 kWh"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("3 kWh", Energy.Format("2.567 kWh", decimals: 0));
        Assert.Equal("2.6 kWh", Energy.Format("2.567 kWh", decimals: 1));
        Assert.Equal("2.57 kWh", Energy.Format("2.567 kWh", decimals: 2));
        Assert.Equal("2.567 kWh", Energy.Format("2.567 kWh"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var e = Energy.Parse("2.567 kWh");
        Assert.Equal("3 kWh", e.ToString(EnergyUnit.KilowattHour, decimals: 0));
        Assert.Equal("2.6 kWh", e.ToString(EnergyUnit.KilowattHour, decimals: 1));
    }

    [Theory]
    [InlineData("1 kWh", "3600000 J")]
    [InlineData("1000 kJ", "1000000 J")]
    [InlineData("5 J", "5 J")]
    [InlineData("3600 kJ", "3600000 J")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Energy.Normalize(input));
    }

    [Theory]
    [InlineData("1 kWh", "1 kWh")]
    [InlineData("5 kJ", "5 kJ")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var energy = Energy.Parse(input);
        Assert.Equal(expected, energy.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var energy = Energy.FromKilowattHours(1);
        Assert.Equal("3600000 J", energy.ToString(EnergyUnit.Joule));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var energy = Energy.FromJoules(3_600_000);
        Assert.Equal(3_600_000m, energy.Joules);
        Assert.Equal(3600m, energy.Kilojoules);
        Assert.Equal(3.6m, energy.Megajoules);
        Assert.Equal(1000m, energy.WattHours);
        Assert.Equal(1m, energy.KilowattHours);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var energy = Energy.FromKilowattHours(1);
        Assert.Equal(3_600_000m, energy.In(EnergyUnit.Joule));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Energy.FromJoules(1_000);
        var b = Energy.FromJoules(2_000);
        Assert.Equal(3000m, (a + b).Joules);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Energy.FromJoules(5_000);
        var b = Energy.FromJoules(1_000);
        Assert.Equal(4000m, (a - b).Joules);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Energy.FromJoules(1000);
        Assert.Equal(3000m, (a * 3).Joules);
        Assert.Equal(3000m, (3 * a).Joules);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Energy.FromJoules(3000);
        Assert.Equal(1000m, (a / 3).Joules);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Energy.FromJoules(1000);
        var b = Energy.FromJoules(2000);
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
        var a = Energy.FromKilowattHours(1);
        var b = Energy.FromJoules(3_600_000);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var energy = Energy.Parse("5 kWh");
        Assert.Same(EnergyUnit.KilowattHour, energy.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsEnergyValues()
    {
        var text = "Usage was 250 kWh last month and peak 10 MJ.";
        var candidates = Energy.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var energy = Energy.Parse("5 kWh");
        Assert.Equal("*** kWh", energy.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Energy.IsNormalized("5 J"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Energy.IsNormalized("5 kWh"));
    }

    [Theory]
    [InlineData("J", "J")]
    [InlineData("kJ", "kJ")]
    [InlineData("kilojoule", "kJ")]
    [InlineData("kilowattimme", "kWh")]
    [InlineData("watt-hour", "Wh")]
    [InlineData("calories", "cal")]
    [InlineData("kalorier", "cal")]
    [InlineData("btu", "BTU")]
    [InlineData("joules", "J")]
    [InlineData("kilojoules", "kJ")]
    [InlineData("megajoules", "MJ")]
    [InlineData("watt-hours", "Wh")]
    [InlineData("watt hours", "Wh")]
    [InlineData("watthour", "Wh")]
    [InlineData("watthours", "Wh")]
    [InlineData("kilowattimmar", "kWh")]
    [InlineData("kilowatt-hours", "kWh")]
    [InlineData("kilowatt hours", "kWh")]
    [InlineData("kilowatthour", "kWh")]
    [InlineData("kilowatthours", "kWh")]
    [InlineData("megawatt-hours", "MWh")]
    [InlineData("megawatthours", "MWh")]
    [InlineData("wattimmar", "Wh")]
    [InlineData("kilocalories", "kcal")]
    [InlineData("kilokalorier", "kcal")]
    [InlineData("GJ", "GJ")]
    [InlineData("gigajoule", "GJ")]
    [InlineData("gigajoules", "GJ")]
    [InlineData("TJ", "TJ")]
    [InlineData("terajoule", "TJ")]
    [InlineData("terajoules", "TJ")]
    [InlineData("GWh", "GWh")]
    [InlineData("gigawatt-hour", "GWh")]
    [InlineData("gigawatt-hours", "GWh")]
    [InlineData("gigawatthour", "GWh")]
    [InlineData("gigawatthours", "GWh")]
    [InlineData("TWh", "TWh")]
    [InlineData("terawatt-hour", "TWh")]
    [InlineData("terawatt-hours", "TWh")]
    [InlineData("terawatthour", "TWh")]
    [InlineData("terawatthours", "TWh")]
    public void EnergyUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(EnergyUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 kWh")]
    [InlineData("2.5 MJ")]
    [InlineData("0,5 kcal")]
    [InlineData("3.14 J")]
    [InlineData("1 000 J")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Energy.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 kJ", 5500)]
    [InlineData("2.5 kWh", 9000000)]
    [InlineData("1 000 J", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedJoules)
    {
        Assert.True(Energy.TryParse(input, out var result));
        Assert.Equal((decimal)expectedJoules, result!.Joules);
    }

    [Theory]
    [InlineData("5,5 kWh", "5.5 kWh")]
    [InlineData("  10  J  ", "10 J")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Energy.Format(input));
    }

    [Theory]
    [InlineData("2.5 kJ", "2500 J")]
    [InlineData("5,5 J", "5.5 J")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Energy.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Energy.FromKilojoules(1.5m);
        var b = Energy.FromJoules(500);
        Assert.Equal(2000m, (a + b).Joules);
    }

    [Fact]
    public void Conversions_GigajouleToJoule()
    {
        var energy = Energy.Parse("1 GJ");
        Assert.Equal(1_000_000_000m, energy.Joules);
    }

    [Fact]
    public void Conversions_TerajouleToJoule()
    {
        var energy = Energy.Parse("1 TJ");
        Assert.Equal(1_000_000_000_000m, energy.Joules);
    }

    [Fact]
    public void Conversions_GigawattHourToJoule()
    {
        var energy = Energy.Parse("1 GWh");
        Assert.Equal(3_600_000_000_000m, energy.Joules);
    }

    [Fact]
    public void Conversions_TerawattHourToJoule()
    {
        var energy = Energy.Parse("1 TWh");
        Assert.Equal(3_600_000_000_000_000m, energy.Joules);
    }

    [Fact]
    public void FromFactory_Gigajoules()
    {
        var energy = Energy.FromGigajoules(2);
        Assert.Equal(2m, energy.Gigajoules);
    }

    [Fact]
    public void FromFactory_Terajoules()
    {
        var energy = Energy.FromTerajoules(1);
        Assert.Equal(1m, energy.Terajoules);
    }

    [Fact]
    public void FromFactory_GigawattHours()
    {
        var energy = Energy.FromGigawattHours(5);
        Assert.Equal(5m, energy.GigawattHours);
    }

    [Fact]
    public void FromFactory_TerawattHours()
    {
        var energy = Energy.FromTerawattHours(1);
        Assert.Equal(1m, energy.TerawattHours);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Energy.TryParse("99999999999999 TWh", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Energy.Parse("10 Wh");
        var b = Energy.Parse("100 Wh");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Energy.Parse("10 Wh");
        Assert.Equal(1, a.CompareTo(null));
    }
}
