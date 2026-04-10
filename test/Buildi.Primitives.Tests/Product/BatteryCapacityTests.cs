using Buildi.Primitives.Measurement;
using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class BatteryCapacityTests
{
    [Theory]
    [InlineData("5000")]
    [InlineData("5000 mAh")]
    [InlineData("5 Ah")]
    [InlineData("50 Wh")]
    [InlineData("3.7 kWh")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(BatteryCapacity.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("-5 mAh")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(BatteryCapacity.IsValid(input));
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToMAh()
    {
        Assert.True(BatteryCapacity.TryParse("5000", out var result));
        Assert.NotNull(result!.Charge);
        Assert.Null(result.EnergyValue);
        Assert.Equal("5000 mAh", result.Value);
    }

    [Fact]
    public void TryParse_WithCharge_SetsChargeProperty()
    {
        Assert.True(BatteryCapacity.TryParse("5 Ah", out var result));
        Assert.NotNull(result!.Charge);
        Assert.Null(result.EnergyValue);
    }

    [Fact]
    public void TryParse_WithEnergy_SetsEnergyProperty()
    {
        Assert.True(BatteryCapacity.TryParse("50 Wh", out var result));
        Assert.Null(result!.Charge);
        Assert.NotNull(result.EnergyValue);
    }

    [Theory]
    [InlineData("5000 mAh", "5000 mAh")]
    [InlineData("50 Wh", "50 Wh")]
    [InlineData("5000", "5000 mAh")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, BatteryCapacity.Format(input));
    }

    [Fact]
    public void Equality_SameCharge()
    {
        var a = BatteryCapacity.Parse("5000 mAh");
        var b = BatteryCapacity.Parse("5 Ah");
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_DifferentUnits_NotEqual()
    {
        var charge = BatteryCapacity.Parse("5000 mAh");
        var energy = BatteryCapacity.Parse("50 Wh");
        Assert.False(charge == energy);
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => BatteryCapacity.Parse(input));
    }

    [Fact]
    public void FromMilliampereHours_Decimal_Works()
    {
        var bc = BatteryCapacity.FromMilliampereHours(5000m);
        Assert.NotNull(bc.Charge);
        Assert.Null(bc.EnergyValue);
    }

    [Fact]
    public void FromMilliampereHours_Int_Works()
    {
        var bc = BatteryCapacity.FromMilliampereHours(5000);
        Assert.NotNull(bc.Charge);
    }

    [Fact]
    public void FromMilliampereHours_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BatteryCapacity.FromMilliampereHours(0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => BatteryCapacity.FromMilliampereHours(-1m));
    }

    [Fact]
    public void FromAmpereHours_Works()
    {
        var bc = BatteryCapacity.FromAmpereHours(5m);
        Assert.NotNull(bc.Charge);
    }

    [Fact]
    public void FromWattHours_Works()
    {
        var bc = BatteryCapacity.FromWattHours(50m);
        Assert.NotNull(bc.EnergyValue);
        Assert.Null(bc.Charge);
    }

    [Fact]
    public void FromKilowattHours_Works()
    {
        var bc = BatteryCapacity.FromKilowattHours(75m);
        Assert.NotNull(bc.EnergyValue);
    }

    [Fact]
    public void FromMilliampereHours_EqualsStringParsed()
    {
        var fromFactory = BatteryCapacity.FromMilliampereHours(5000);
        var fromString = BatteryCapacity.Parse("5000 mAh");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void FromWattHours_EqualsStringParsed()
    {
        var fromFactory = BatteryCapacity.FromWattHours(50);
        var fromString = BatteryCapacity.Parse("50 Wh");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void Operator_Add_SameChargeUnit_Works()
    {
        var a = BatteryCapacity.FromMilliampereHours(3000);
        var b = BatteryCapacity.FromMilliampereHours(2000);
        var result = a + b;
        Assert.NotNull(result.Charge);
    }

    [Fact]
    public void Operator_Add_SameEnergyUnit_Works()
    {
        var a = BatteryCapacity.FromWattHours(50);
        var b = BatteryCapacity.FromWattHours(25);
        var result = a + b;
        Assert.NotNull(result.EnergyValue);
    }

    [Fact]
    public void Operator_Add_MixedUnits_Throws()
    {
        var charge = BatteryCapacity.FromMilliampereHours(5000);
        var energy = BatteryCapacity.FromWattHours(50);
        Assert.Throws<InvalidOperationException>(() => charge + energy);
    }

    [Fact]
    public void Operator_Subtract_MixedUnits_Throws()
    {
        var charge = BatteryCapacity.FromMilliampereHours(5000);
        var energy = BatteryCapacity.FromWattHours(50);
        Assert.Throws<InvalidOperationException>(() => charge - energy);
    }

    [Fact]
    public void Operator_Multiply_Works()
    {
        var a = BatteryCapacity.FromMilliampereHours(3000);
        var result = a * 2m;
        Assert.NotNull(result.Charge);
    }

    [Fact]
    public void Operator_Divide_Works()
    {
        var a = BatteryCapacity.FromWattHours(100);
        var result = a / 2m;
        Assert.NotNull(result.EnergyValue);
    }

    [Fact]
    public void Operator_Subtract_SameChargeUnit_Works()
    {
        var a = BatteryCapacity.FromMilliampereHours(5000);
        var b = BatteryCapacity.FromMilliampereHours(2000);
        var result = a - b;
        Assert.NotNull(result.Charge);
    }

    [Fact]
    public void Operator_Subtract_SameEnergyUnit_Works()
    {
        var a = BatteryCapacity.FromWattHours(100);
        var b = BatteryCapacity.FromWattHours(25);
        var result = a - b;
        Assert.NotNull(result.EnergyValue);
    }

    [Fact]
    public void Operator_Multiply_Commutative()
    {
        var a = BatteryCapacity.FromMilliampereHours(3000);
        var result = 2m * a;
        Assert.NotNull(result.Charge);
    }

    [Fact]
    public void Operator_Negate_Charge_Works()
    {
        var a = BatteryCapacity.FromMilliampereHours(3000);
        var result = -a;
        Assert.NotNull(result.Charge);
    }

    [Fact]
    public void Operator_Negate_Energy_Works()
    {
        var a = BatteryCapacity.FromWattHours(50);
        var result = -a;
        Assert.NotNull(result.EnergyValue);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = BatteryCapacity.Parse("3000 mAh");
        var b = BatteryCapacity.Parse("5000 mAh");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = BatteryCapacity.Parse("5000 mAh");
        Assert.Equal(1, a.CompareTo(null));
    }
}
