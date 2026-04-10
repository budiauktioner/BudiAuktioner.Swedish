using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class NaturalUnitTests
{
    // ── DataSize ──

    [Theory]
    [InlineData(0, "B")]
    [InlineData(500, "B")]
    [InlineData(999, "B")]
    [InlineData(1000, "KB")]
    [InlineData(10_240, "KB")]
    [InlineData(1_000_000, "MB")]
    [InlineData(536_870_912, "MB")]
    [InlineData(1_000_000_000, "GB")]
    [InlineData(512_000_000_000, "GB")]
    [InlineData(1_000_000_000_000, "TB")]
    [InlineData(1_500_000_000_000_000, "PB")]
    public void DataSize_NaturalUnit_ReturnsExpected(long bytes, string expectedSymbol)
    {
        var ds = DataSize.Create((decimal)bytes, DataSizeUnit.Byte);
        Assert.Equal(expectedSymbol, ds.NaturalUnit.Symbol);
    }

    [Theory]
    [InlineData("512 GB", "512 GB")]
    [InlineData("10240 B", "10.24 KB")]
    [InlineData("1500000 B", "1.5 MB")]
    [InlineData("1 TB", "1 TB")]
    public void DataSize_ToNaturalString_ReturnsExpected(string input, string expected)
    {
        var ds = DataSize.Parse(input);
        Assert.Equal(expected, ds.ToNaturalString());
    }

    // ── Length ──

    [Theory]
    [InlineData(0.0005, "mm")]
    [InlineData(0.005, "mm")]
    [InlineData(0.01, "cm")]
    [InlineData(0.5, "cm")]
    [InlineData(1, "m")]
    [InlineData(999, "m")]
    [InlineData(1000, "km")]
    [InlineData(150_000, "km")]
    public void Length_NaturalUnit_ReturnsExpected(double meters, string expectedSymbol)
    {
        var len = Length.Create((decimal)meters, LengthUnit.Meter);
        Assert.Equal(expectedSymbol, len.NaturalUnit.Symbol);
    }

    [Theory]
    [InlineData("1500 m", "1.5 km")]
    [InlineData("0.5 m", "50 cm")]
    [InlineData("0.003 m", "3 mm")]
    [InlineData("42 km", "42 km")]
    public void Length_ToNaturalString_ReturnsExpected(string input, string expected)
    {
        var len = Length.Parse(input);
        Assert.Equal(expected, len.ToNaturalString());
    }

    // ── Weight ──

    [Theory]
    [InlineData(0.000001, "mg")]
    [InlineData(0.001, "g")]
    [InlineData(0.5, "g")]
    [InlineData(1, "kg")]
    [InlineData(500, "kg")]
    [InlineData(1000, "t")]
    public void Weight_NaturalUnit_ReturnsExpected(double kilograms, string expectedSymbol)
    {
        var w = Weight.Create((decimal)kilograms, WeightUnit.Kilogram);
        Assert.Equal(expectedSymbol, w.NaturalUnit.Symbol);
    }

    [Theory]
    [InlineData("1500 g", "1.5 kg")]
    [InlineData("2000 kg", "2 t")]
    [InlineData("500 mg", "500 mg")]
    public void Weight_ToNaturalString_ReturnsExpected(string input, string expected)
    {
        var w = Weight.Parse(input);
        Assert.Equal(expected, w.ToNaturalString());
    }

    // ── Area ──

    [Theory]
    [InlineData(0.0001, "cm²")]
    [InlineData(1, "m²")]
    [InlineData(10000, "ha")]
    [InlineData(1000000, "km²")]
    public void Area_NaturalUnit_ReturnsExpected(double squareMeters, string expectedSymbol)
    {
        var a = Area.Create((decimal)squareMeters, AreaUnit.SquareMeter);
        Assert.Equal(expectedSymbol, a.NaturalUnit.Symbol);
    }

    // ── Volume ──

    [Theory]
    [InlineData(0.001, "mL")]
    [InlineData(0.1, "dL")]
    [InlineData(1, "L")]
    [InlineData(1000, "m³")]
    public void Volume_NaturalUnit_ReturnsExpected(double liters, string expectedSymbol)
    {
        var v = Volume.Create((decimal)liters, VolumeUnit.Liter);
        Assert.Equal(expectedSymbol, v.NaturalUnit.Symbol);
    }

    // ── Power ──

    [Theory]
    [InlineData(500, "W")]
    [InlineData(1000, "kW")]
    [InlineData(1500, "kW")]
    [InlineData(1_000_000, "MW")]
    [InlineData(1_000_000_000, "GW")]
    public void Power_NaturalUnit_ReturnsExpected(double watts, string expectedSymbol)
    {
        var p = Power.Create((decimal)watts, PowerUnit.Watt);
        Assert.Equal(expectedSymbol, p.NaturalUnit.Symbol);
    }

    [Theory]
    [InlineData("1500 W", "1.5 kW")]
    [InlineData("500 W", "500 W")]
    [InlineData("2500000 W", "2.5 MW")]
    public void Power_ToNaturalString_ReturnsExpected(string input, string expected)
    {
        var p = Power.Parse(input);
        Assert.Equal(expected, p.ToNaturalString());
    }

    // ── Frequency ──

    [Theory]
    [InlineData(500, "Hz")]
    [InlineData(1000, "kHz")]
    [InlineData(3_500_000_000, "GHz")]
    public void Frequency_NaturalUnit_ReturnsExpected(double hertz, string expectedSymbol)
    {
        var f = Frequency.Create((decimal)hertz, FrequencyUnit.Hertz);
        Assert.Equal(expectedSymbol, f.NaturalUnit.Symbol);
    }

    // ── Voltage ──

    [Theory]
    [InlineData(0.001, "mV")]
    [InlineData(0.5, "mV")]
    [InlineData(1, "V")]
    [InlineData(230, "V")]
    [InlineData(1000, "kV")]
    public void Voltage_NaturalUnit_ReturnsExpected(double volts, string expectedSymbol)
    {
        var v = Voltage.Create((decimal)volts, VoltageUnit.Volt);
        Assert.Equal(expectedSymbol, v.NaturalUnit.Symbol);
    }

    // ── ElectricCharge ──

    [Theory]
    [InlineData(0.5, "mAh")]
    [InlineData(1, "Ah")]
    [InlineData(5, "Ah")]
    public void ElectricCharge_NaturalUnit_ReturnsExpected(double ampereHours, string expectedSymbol)
    {
        var ec = ElectricCharge.Create((decimal)ampereHours, ElectricChargeUnit.AmpereHour);
        Assert.Equal(expectedSymbol, ec.NaturalUnit.Symbol);
    }

    [Theory]
    [InlineData("5000 mAh", "5 Ah")]
    [InlineData("500 mAh", "500 mAh")]
    public void ElectricCharge_ToNaturalString_ReturnsExpected(string input, string expected)
    {
        var ec = ElectricCharge.Parse(input);
        Assert.Equal(expected, ec.ToNaturalString());
    }

    // ── Pressure ──

    [Theory]
    [InlineData(50, "Pa")]
    [InlineData(100, "hPa")]
    [InlineData(1000, "kPa")]
    [InlineData(100_000, "bar")]
    [InlineData(200_000, "bar")]
    [InlineData(1_000_000, "MPa")]
    public void Pressure_NaturalUnit_ReturnsExpected(double pascals, string expectedSymbol)
    {
        var p = Pressure.Create((decimal)pascals, PressureUnit.Pascal);
        Assert.Equal(expectedSymbol, p.NaturalUnit.Symbol);
    }

    [Theory]
    [InlineData("200000 Pa", "2 bar")]
    [InlineData("500 Pa", "5 hPa")]
    public void Pressure_ToNaturalString_ReturnsExpected(string input, string expected)
    {
        var p = Pressure.Parse(input);
        Assert.Equal(expected, p.ToNaturalString());
    }

    // ── Energy (Wh scale) ──

    [Theory]
    [InlineData("100 kWh", "100 kWh")]
    [InlineData("5000 Wh", "5 kWh")]
    [InlineData("1500000 Wh", "1.5 MWh")]
    public void Energy_ToNaturalString_ReturnsExpected(string input, string expected)
    {
        var e = Energy.Parse(input);
        Assert.Equal(expected, e.ToNaturalString());
    }

    // ── NaturalScale properties ──

    [Fact]
    public void NaturalScale_IsOrderedAscendingByFactor()
    {
        AssertAscending(DataSizeUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(LengthUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(WeightUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(AreaUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(VolumeUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(EnergyUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(PowerUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(FrequencyUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(VoltageUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(ElectricChargeUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
        AssertAscending(PressureUnit.NaturalScale.Select(u => u.ToBaseUnitFactor));
    }

    [Fact]
    public void NaturalScale_HasAtLeastTwoEntries()
    {
        Assert.True(DataSizeUnit.NaturalScale.Count >= 2);
        Assert.True(LengthUnit.NaturalScale.Count >= 2);
        Assert.True(WeightUnit.NaturalScale.Count >= 2);
        Assert.True(AreaUnit.NaturalScale.Count >= 2);
        Assert.True(VolumeUnit.NaturalScale.Count >= 2);
        Assert.True(EnergyUnit.NaturalScale.Count >= 2);
        Assert.True(PowerUnit.NaturalScale.Count >= 2);
        Assert.True(FrequencyUnit.NaturalScale.Count >= 2);
        Assert.True(VoltageUnit.NaturalScale.Count >= 2);
        Assert.True(ElectricChargeUnit.NaturalScale.Count >= 2);
        Assert.True(PressureUnit.NaturalScale.Count >= 2);
    }

    [Fact]
    public void GetNatural_Zero_ReturnsSmallestUnit()
    {
        Assert.Equal(DataSizeUnit.Byte, DataSizeUnit.GetNatural(0m));
        Assert.Equal(LengthUnit.Millimeter, LengthUnit.GetNatural(0m));
        Assert.Equal(WeightUnit.Milligram, WeightUnit.GetNatural(0m));
        Assert.Equal(PowerUnit.Watt, PowerUnit.GetNatural(0m));
    }

    private static void AssertAscending(IEnumerable<decimal> values)
    {
        decimal? prev = null;
        foreach (var v in values)
        {
            if (prev.HasValue)
                Assert.True(v > prev.Value, $"Expected {v} > {prev.Value}");
            prev = v;
        }
    }
}
