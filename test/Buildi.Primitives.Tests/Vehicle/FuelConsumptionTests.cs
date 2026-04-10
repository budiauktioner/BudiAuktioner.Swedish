using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class FuelConsumptionTests
{
    [Theory]
    [InlineData("8.3 l/100km")]
    [InlineData("8,3 l/100km")]
    [InlineData("12 km/l")]
    [InlineData("28 mpg")]
    [InlineData("28 mpg (imp)")]
    [InlineData("15 kWh/100km")]
    [InlineData("150 Wh/km")]
    [InlineData("0.15 kWh/km")]
    [InlineData("8.3")]
    [InlineData("  8.3 l/100km  ")]
    [InlineData("12 liter/100km")]
    [InlineData("30 miles per gallon")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(FuelConsumption.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("0 l/100km")]
    [InlineData("-5 l/100km")]
    [InlineData("0")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(FuelConsumption.IsValid(input));
    }

    [Fact]
    public void TryParse_LitersPer100Km_ReturnsExpectedProperties()
    {
        var ok = FuelConsumption.TryParse("8.3 l/100km", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(8.3m, fc.LitersPer100Km);
        Assert.Equal("8.3 l/100km", fc.Value);
        Assert.False(fc.IsElectric);
        Assert.Equal(0m, fc.KwhPer100Km);
    }

    [Fact]
    public void TryParse_LitersPer100Km_WithSpace_IsValid()
    {
        var ok = FuelConsumption.TryParse("8.3 l/100 km", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(8.3m, fc.LitersPer100Km);
    }

    [Fact]
    public void TryParse_CommaDecimal_ParsesCorrectly()
    {
        var ok = FuelConsumption.TryParse("8,3 l/100km", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(8.3m, fc.LitersPer100Km);
    }

    [Fact]
    public void TryParse_KmPerLiter_ConvertsCorrectly()
    {
        var ok = FuelConsumption.TryParse("12 km/l", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(Math.Round(100m / 12m, 6), fc.LitersPer100Km);
        Assert.Equal("12 km/l", fc.Value);
        Assert.False(fc.IsElectric);
    }

    [Fact]
    public void TryParse_MpgUs_ConvertsCorrectly()
    {
        var ok = FuelConsumption.TryParse("28 mpg", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(Math.Round(235.214583m / 28m, 6), fc.LitersPer100Km);
        Assert.Equal("28 mpg", fc.Value);
    }

    [Fact]
    public void TryParse_MpgImp_ConvertsCorrectly()
    {
        var ok = FuelConsumption.TryParse("28 mpg (imp)", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(Math.Round(282.481m / 28m, 6), fc.LitersPer100Km);
    }

    [Fact]
    public void TryParse_MilesPerGallon_ConvertsAsUsmpg()
    {
        var ok = FuelConsumption.TryParse("30 miles per gallon", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(Math.Round(235.214583m / 30m, 6), fc.LitersPer100Km);
    }

    [Fact]
    public void TryParse_KwhPer100Km_SetsElectricProperties()
    {
        var ok = FuelConsumption.TryParse("15 kWh/100km", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.True(fc.IsElectric);
        Assert.Equal(15m, fc.KwhPer100Km);
        Assert.Equal(0m, fc.LitersPer100Km);
        Assert.Equal("15 kWh/100km", fc.Value);
    }

    [Fact]
    public void TryParse_WhPerKm_ConvertsToKwhPer100Km()
    {
        var ok = FuelConsumption.TryParse("150 Wh/km", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.True(fc.IsElectric);
        Assert.Equal(15m, fc.KwhPer100Km);
        Assert.Equal("150 Wh/km", fc.Value);
    }

    [Fact]
    public void TryParse_KwhPerKm_ConvertsToKwhPer100Km()
    {
        var ok = FuelConsumption.TryParse("0.15 kWh/km", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.True(fc.IsElectric);
        Assert.Equal(15m, fc.KwhPer100Km);
        Assert.Equal("0.15 kWh/km", fc.Value);
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToLitersPer100Km()
    {
        var ok = FuelConsumption.TryParse("8.3", out var fc);

        Assert.True(ok);
        Assert.NotNull(fc);
        Assert.Equal(8.3m, fc.LitersPer100Km);
        Assert.Equal("8.3 l/100km", fc.Value);
        Assert.False(fc.IsElectric);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0 l/100km")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = FuelConsumption.TryParse(input, out var fc);

        Assert.False(ok);
        Assert.Null(fc);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => FuelConsumption.Parse("not-fuel"));
    }

    [Theory]
    [InlineData("8.3 l/100km", "8.3 l/100km")]
    [InlineData("12 km/l", "12 km/l")]
    [InlineData("28 mpg", "28 mpg")]
    [InlineData("15 kWh/100km", "15 kWh/100km")]
    [InlineData(null, null)]
    [InlineData("bad", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, FuelConsumption.Format(input));
    }

    [Theory]
    [InlineData("bad", "bad")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string input, string expected)
    {
        Assert.Equal(expected, FuelConsumption.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_LitersPer100Km_ReturnsSameUnit()
    {
        Assert.Equal("8.3 l/100km", FuelConsumption.Normalize("8.3 l/100km"));
    }

    [Fact]
    public void Normalize_KmPerLiter_ConvertsToLitersPer100Km()
    {
        var normalized = FuelConsumption.Normalize("12 km/l");

        Assert.NotNull(normalized);
        Assert.EndsWith(" l/100km", normalized);
    }

    [Fact]
    public void Normalize_MpgUs_ConvertsToLitersPer100Km()
    {
        var normalized = FuelConsumption.Normalize("28 mpg");

        Assert.NotNull(normalized);
        Assert.EndsWith(" l/100km", normalized);
    }

    [Fact]
    public void Normalize_KwhPer100Km_ReturnsKwhUnit()
    {
        Assert.Equal("15 kWh/100km", FuelConsumption.Normalize("15 kWh/100km"));
    }

    [Fact]
    public void Normalize_WhPerKm_ConvertsToKwhPer100Km()
    {
        Assert.Equal("15 kWh/100km", FuelConsumption.Normalize("150 Wh/km"));
    }

    [Fact]
    public void Normalize_BareNumber_NormalizesToLitersPer100Km()
    {
        Assert.Equal("8.3 l/100km", FuelConsumption.Normalize("8.3"));
    }

    [Theory]
    [InlineData("8.3 l/100km", true)]
    [InlineData("12 km/l", false)]
    [InlineData("28 mpg", false)]
    [InlineData("15 kWh/100km", true)]
    [InlineData("150 Wh/km", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, FuelConsumption.IsNormalized(input));
    }

    [Fact]
    public void KilometersPerLiter_ComputedCorrectly()
    {
        var fc = FuelConsumption.FromLitersPer100Km(10m);
        Assert.Equal(10m, fc.KilometersPerLiter);
    }

    [Fact]
    public void MilesPerGallonUs_ComputedCorrectly()
    {
        var fc = FuelConsumption.FromLitersPer100Km(10m);
        Assert.Equal(Math.Round(235.214583m / 10m, 6), fc.MilesPerGallonUs);
    }

    [Fact]
    public void MilesPerGallonImp_ComputedCorrectly()
    {
        var fc = FuelConsumption.FromLitersPer100Km(10m);
        Assert.Equal(Math.Round(282.481m / 10m, 6), fc.MilesPerGallonImp);
    }

    [Fact]
    public void ElectricVehicle_HasZeroFuelProperties()
    {
        var fc = FuelConsumption.FromKwhPer100Km(15m);

        Assert.Equal(0m, fc.LitersPer100Km);
        Assert.Equal(0m, fc.KilometersPerLiter);
        Assert.Equal(0m, fc.MilesPerGallonUs);
        Assert.Equal(0m, fc.MilesPerGallonImp);
    }

    [Fact]
    public void FromLitersPer100Km_CreatesCorrectly()
    {
        var fc = FuelConsumption.FromLitersPer100Km(8.3m);

        Assert.Equal(8.3m, fc.LitersPer100Km);
        Assert.Equal("8.3 l/100km", fc.Value);
        Assert.False(fc.IsElectric);
    }

    [Fact]
    public void FromKilometersPerLiter_CreatesCorrectly()
    {
        var fc = FuelConsumption.FromKilometersPerLiter(12m);

        Assert.Equal(Math.Round(100m / 12m, 6), fc.LitersPer100Km);
        Assert.Equal("12 km/l", fc.Value);
    }

    [Fact]
    public void FromMpgUs_CreatesCorrectly()
    {
        var fc = FuelConsumption.FromMpgUs(28m);

        Assert.Equal(Math.Round(235.214583m / 28m, 6), fc.LitersPer100Km);
        Assert.Equal("28 mpg", fc.Value);
    }

    [Fact]
    public void FromKwhPer100Km_CreatesCorrectly()
    {
        var fc = FuelConsumption.FromKwhPer100Km(15m);

        Assert.Equal(15m, fc.KwhPer100Km);
        Assert.True(fc.IsElectric);
        Assert.Equal("15 kWh/100km", fc.Value);
    }

    [Fact]
    public void FactoryMethods_ThrowForNonPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FuelConsumption.FromLitersPer100Km(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => FuelConsumption.FromLitersPer100Km(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => FuelConsumption.FromKilometersPerLiter(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => FuelConsumption.FromMpgUs(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => FuelConsumption.FromKwhPer100Km(0));
    }

    [Fact]
    public void Equality_SameValues()
    {
        var a = FuelConsumption.FromLitersPer100Km(8.3m);
        var b = FuelConsumption.Parse("8.3 l/100km");

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_BareNumberAndUnit_AreEqual()
    {
        var a = FuelConsumption.Parse("8.3");
        var b = FuelConsumption.Parse("8.3 l/100km");

        Assert.True(a == b);
    }

    [Fact]
    public void Equality_DifferentValues()
    {
        var a = FuelConsumption.FromLitersPer100Km(8.3m);
        var b = FuelConsumption.FromLitersPer100Km(10m);

        Assert.True(a != b);
    }

    [Fact]
    public void Equality_FuelAndElectric_NotEqual()
    {
        var fuel = FuelConsumption.FromLitersPer100Km(8.3m);
        var ev = FuelConsumption.FromKwhPer100Km(15m);

        Assert.True(fuel != ev);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = FuelConsumption.FromLitersPer100Km(5m);
        var b = FuelConsumption.FromLitersPer100Km(10m);

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = FuelConsumption.FromLitersPer100Km(8.3m);
        Assert.Equal(1, a.CompareTo(null));
    }
}
