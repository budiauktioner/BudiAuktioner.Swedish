using Buildi.Primitives.Measurement;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class OdometerReadingTests
{
    [Theory]
    [InlineData("15000")]
    [InlineData("15000 km")]
    [InlineData("150 mil")]
    [InlineData("9320 mi")]
    [InlineData("0")]
    [InlineData("0 km")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(OdometerReading.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("-100 km")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(OdometerReading.IsValid(input));
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToKm()
    {
        Assert.True(OdometerReading.TryParse("15000", out var result));
        Assert.Equal(15000m, result!.Kilometers);
        Assert.Equal("15000 km", result.Value);
    }

    [Fact]
    public void TryParse_SwedishMil_ParsesCorrectly()
    {
        Assert.True(OdometerReading.TryParse("150 mil", out var result));
        Assert.Equal(1500m, result!.Kilometers);
        Assert.Equal(150m, result.SwedishMiles);
    }

    [Fact]
    public void TryParse_EnglishMiles_ParsesCorrectly()
    {
        Assert.True(OdometerReading.TryParse("10000 mi", out var result));
        Assert.Equal(10000m, result!.Miles);
    }

    [Fact]
    public void TryParse_Zero_IsValid()
    {
        Assert.True(OdometerReading.TryParse("0", out var result));
        Assert.Equal(0m, result!.Kilometers);
    }

    [Theory]
    [InlineData("15000 km", "15000 km")]
    [InlineData("150 mil", "150 mil")]
    [InlineData("15000", "15000 km")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, OdometerReading.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("150 mil", OdometerReading.Format("1500 km", unit: LengthUnit.SwedishMile));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("15001 km", OdometerReading.Format("15000.567 km", decimals: 0));
    }

    [Theory]
    [InlineData("15000 km", "15000 km")]
    [InlineData("150 mil", "1500 km")]
    public void Normalize_ReturnsKilometers(string? input, string? expected)
    {
        Assert.Equal(expected, OdometerReading.Normalize(input));
    }

    [Fact]
    public void Comparison_Works()
    {
        var a = OdometerReading.Parse("10000 km");
        var b = OdometerReading.Parse("20000 km");
        Assert.True(a < b);
        Assert.True(b > a);
    }

    [Fact]
    public void Equality_SameDistance()
    {
        var a = OdometerReading.Parse("10 mil");
        var b = OdometerReading.Parse("100 km");
        Assert.True(a == b);
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => OdometerReading.Parse(input));
    }

    [Fact]
    public void Create_FromDecimalAndUnit_Works()
    {
        var reading = OdometerReading.Create(15000m, LengthUnit.Kilometer);
        Assert.Equal(15000m, reading.Kilometers);
    }

    [Fact]
    public void Create_FromIntAndUnit_Works()
    {
        var reading = OdometerReading.Create(15000, LengthUnit.Kilometer);
        Assert.Equal(15000m, reading.Kilometers);
    }

    [Fact]
    public void Create_NegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OdometerReading.Create(-1m, LengthUnit.Kilometer));
    }

    [Fact]
    public void FromKilometers_Decimal_Works()
    {
        var reading = OdometerReading.FromKilometers(15000.5m);
        Assert.Equal(15000.5m, reading.Kilometers);
        Assert.Equal("15000.5 km", reading.ToString());
    }

    [Fact]
    public void FromKilometers_Int_Works()
    {
        var reading = OdometerReading.FromKilometers(15000);
        Assert.Equal(15000m, reading.Kilometers);
    }

    [Fact]
    public void FromSwedishMiles_Works()
    {
        var reading = OdometerReading.FromSwedishMiles(150m);
        Assert.Equal(1500m, reading.Kilometers);
        Assert.Equal(150m, reading.SwedishMiles);
    }

    [Fact]
    public void FromMiles_Works()
    {
        var reading = OdometerReading.FromMiles(10000m);
        Assert.Equal(10000m, reading.Miles);
    }

    [Fact]
    public void FromMeters_Works()
    {
        var reading = OdometerReading.FromMeters(15000m);
        Assert.Equal(15m, reading.Kilometers);
    }

    [Fact]
    public void Create_EqualsStringParsed_SameDistance()
    {
        var fromCreate = OdometerReading.Create(150m, LengthUnit.SwedishMile);
        var fromParse = OdometerReading.Parse("150 mil");
        Assert.Equal(fromCreate, fromParse);
    }

    [Fact]
    public void Operator_Add_CombinesDistance()
    {
        var a = OdometerReading.FromKilometers(15000);
        var b = OdometerReading.FromKilometers(5000);
        Assert.Equal(20000m, (a + b).Kilometers);
    }

    [Fact]
    public void Operator_Subtract_Works()
    {
        var a = OdometerReading.FromKilometers(20000);
        var b = OdometerReading.FromKilometers(15000);
        Assert.Equal(5000m, (a - b).Kilometers);
    }

    [Fact]
    public void Operator_Multiply_Works()
    {
        var a = OdometerReading.FromKilometers(10000);
        Assert.Equal(20000m, (a * 2m).Kilometers);
    }

    [Fact]
    public void Operator_Divide_Works()
    {
        var a = OdometerReading.FromKilometers(20000);
        Assert.Equal(10000m, (a / 2m).Kilometers);
    }

    [Fact]
    public void Operator_Multiply_Commutative()
    {
        var a = OdometerReading.FromKilometers(10000);
        Assert.Equal(20000m, (2m * a).Kilometers);
    }

    [Fact]
    public void Operator_Negate_Works()
    {
        var a = OdometerReading.FromKilometers(10000);
        Assert.Equal(-10000m, (-a).Kilometers);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = OdometerReading.Parse("10000 km");
        var b = OdometerReading.Parse("20000 km");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = OdometerReading.Parse("15000 km");
        Assert.Equal(1, a.CompareTo(null));
    }
}
