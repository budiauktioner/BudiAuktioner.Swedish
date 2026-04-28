using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class EnergyConsumptionTests
{
    [Theory]
    [InlineData("15 kWh/100km")]
    [InlineData("15kWh/100km")]
    [InlineData("15 kWh/100 km")]
    [InlineData("0.15 kWh/km")]
    [InlineData("150 Wh/km")]
    [InlineData("4 mi/kWh")]
    [InlineData("4 miles/kWh")]
    [InlineData("0.25 kWh/mi")]
    [InlineData("0.25 kWh/mile")]
    [InlineData("12.5 kWh/100km")]
    [InlineData("15")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EnergyConsumption.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("0 kWh/100km")]
    [InlineData("-5 kWh/100km")]
    [InlineData("15 l/100km")]
    [InlineData("15 xyz")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EnergyConsumption.IsValid(input));
    }

    [Theory]
    [InlineData("15 kWh/100km", 15)]
    [InlineData("0.15 kWh/km", 15)]
    [InlineData("150 Wh/km", 15)]
    [InlineData("15", 15)]
    public void TryParse_ReturnsExpected_KwhPer100Km(string input, double expected)
    {
        Assert.True(EnergyConsumption.TryParse(input, out var result));
        Assert.Equal((decimal)expected, result!.KwhPer100Km);
    }

    [Fact]
    public void TryParse_MilesPerKwh_ConvertsCorrectly()
    {
        Assert.True(EnergyConsumption.TryParse("4 mi/kWh", out var result));
        var expected = Math.Round(100m / (4m * 1.609344m), 6);
        Assert.Equal(expected, result!.KwhPer100Km);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("15 l/100km")]
    public void Parse_Throws_ForInvalid(string input) =>
        Assert.Throws<ArgumentException>(() => EnergyConsumption.Parse(input));

    [Theory]
    [InlineData("15 kWh/100km", "15 kWh/100km")]
    [InlineData("0.15 kWh/km", "0.15 kWh/km")]
    [InlineData("150 Wh/km", "150 Wh/km")]
    [InlineData("4 mi/kWh", "4 mi/kWh")]
    public void Format_PreservesOriginalUnit(string input, string expected) =>
        Assert.Equal(expected, EnergyConsumption.Format(input));

    [Theory]
    [InlineData("15 kWh/100km", "15 kWh/100km")]
    [InlineData("0.15 kWh/km", "15 kWh/100km")]
    [InlineData("150 Wh/km", "15 kWh/100km")]
    [InlineData("15", "15 kWh/100km")]
    public void Normalize_ReturnsKwhPer100Km(string input, string expected) =>
        Assert.Equal(expected, EnergyConsumption.Normalize(input));

    [Theory]
    [InlineData("15 kWh/100km", true)]
    [InlineData("0.15 kWh/km", false)]
    [InlineData("150 Wh/km", false)]
    [InlineData("15", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected) =>
        Assert.Equal(expected, EnergyConsumption.IsNormalized(input));

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var ec = EnergyConsumption.FromKwhPer100Km(15m);
        Assert.Equal(15m, ec.KwhPer100Km);
        Assert.Equal(0.15m, ec.KwhPerKm);
        Assert.Equal(150m, ec.WhPerKm);
        Assert.True(ec.MilesPerKwh > 4m && ec.MilesPerKwh < 5m);
    }

    [Fact]
    public void FromKwhPer100Km_Throws_ForNonPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnergyConsumption.FromKwhPer100Km(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnergyConsumption.FromKwhPer100Km(-1));
    }

    [Fact]
    public void FromWhPerKm_ConvertsCorrectly()
    {
        var ec = EnergyConsumption.FromWhPerKm(150);
        Assert.Equal(15m, ec.KwhPer100Km);
    }

    [Fact]
    public void FromMilesPerKwh_ConvertsCorrectly()
    {
        var ec = EnergyConsumption.FromMilesPerKwh(4m);
        var expected = Math.Round(100m / (4m * 1.609344m), 6);
        Assert.Equal(expected, ec.KwhPer100Km);
    }

    [Fact]
    public void Equality_SameKwhPer100Km()
    {
        var a = EnergyConsumption.FromKwhPer100Km(15);
        var b = EnergyConsumption.FromWhPerKm(150);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Comparison_OrdersByKwhPer100Km()
    {
        var efficient = EnergyConsumption.FromKwhPer100Km(12);
        var thirsty = EnergyConsumption.FromKwhPer100Km(25);
        Assert.True(efficient < thirsty);
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        Assert.Equal("kWh/100km", EnergyConsumption.Parse("15 kWh/100km").OriginalUnit);
        Assert.Equal("Wh/km", EnergyConsumption.Parse("150 Wh/km").OriginalUnit);
        Assert.Equal("mi/kWh", EnergyConsumption.Parse("4 mi/kWh").OriginalUnit);
    }

    [Fact]
    public void ToMaskedString_PreservesUnit()
    {
        var ec = EnergyConsumption.Parse("15 kWh/100km");
        Assert.Equal("*** kWh/100km", ec.ToMaskedString());
    }

    [Fact]
    public void FindCandidatesInText_FindsValues()
    {
        var text = "EPA rates the car at 4 mi/kWh, equivalent to roughly 15 kWh/100km or 150 Wh/km.";
        var candidates = EnergyConsumption.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 3);
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Energy Consumption", EnergyConsumption.TypeInfo.EnglishName);
        Assert.Equal("Elförbrukning", EnergyConsumption.TypeInfo.LocalizedName);
    }
}
