using Buildi.Primitives.Measurement;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class EngineDisplacementTests
{
    [Theory]
    [InlineData("1998")]
    [InlineData("1998 mL")]
    [InlineData("2.0 L")]
    [InlineData("1998 cc")]
    [InlineData("500 mL")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EngineDisplacement.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("-500 cc")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EngineDisplacement.IsValid(input));
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToCC()
    {
        Assert.True(EngineDisplacement.TryParse("1998", out var result));
        Assert.Equal(1998m, result!.CubicCentimeters);
        Assert.Equal("1998 mL", result.Value);
    }

    [Fact]
    public void TryParse_CC_ParsesCorrectly()
    {
        Assert.True(EngineDisplacement.TryParse("1998 cc", out var result));
        Assert.Equal(1998m, result!.CubicCentimeters);
    }

    [Fact]
    public void TryParse_Liters_ParsesCorrectly()
    {
        Assert.True(EngineDisplacement.TryParse("2.0 L", out var result));
        Assert.Equal(2.0m, result!.Liters);
    }

    [Theory]
    [InlineData("1998 cc", "1998 mL")]
    [InlineData("2.0 L", "2 L")]
    [InlineData("1998", "1998 mL")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EngineDisplacement.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("2 L", EngineDisplacement.Format("2000 cc", unit: VolumeUnit.Liter));
    }

    [Fact]
    public void Comparison_Works()
    {
        var a = EngineDisplacement.Parse("1600 cc");
        var b = EngineDisplacement.Parse("2.0 L");
        Assert.True(a < b);
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => EngineDisplacement.Parse(input));
    }

    [Fact]
    public void Create_FromDecimalAndUnit_Works()
    {
        var ed = EngineDisplacement.Create(1998m, VolumeUnit.Milliliter);
        Assert.True(ed.CubicCentimeters > 0);
    }

    [Fact]
    public void Create_FromIntAndUnit_Works()
    {
        var ed = EngineDisplacement.Create(1998, VolumeUnit.Milliliter);
        Assert.True(ed.CubicCentimeters > 0);
    }

    [Fact]
    public void Create_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EngineDisplacement.Create(0m, VolumeUnit.Milliliter));
        Assert.Throws<ArgumentOutOfRangeException>(() => EngineDisplacement.Create(-1m, VolumeUnit.Milliliter));
    }

    [Fact]
    public void FromCubicCentimeters_Decimal_Works()
    {
        var ed = EngineDisplacement.FromCubicCentimeters(1998m);
        Assert.Equal(1998m, ed.CubicCentimeters);
    }

    [Fact]
    public void FromCubicCentimeters_Int_Works()
    {
        var ed = EngineDisplacement.FromCubicCentimeters(1998);
        Assert.Equal(1998m, ed.CubicCentimeters);
    }

    [Fact]
    public void FromLiters_Works()
    {
        var ed = EngineDisplacement.FromLiters(2m);
        Assert.Equal(2m, ed.Liters);
    }

    [Fact]
    public void Create_EqualsStringParsed()
    {
        var fromFactory = EngineDisplacement.FromCubicCentimeters(1998);
        var fromString = EngineDisplacement.Parse("1998");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void Operator_Add_CombinesDisplacement()
    {
        var a = EngineDisplacement.FromCubicCentimeters(1000);
        var b = EngineDisplacement.FromCubicCentimeters(1000);
        Assert.Equal(2000m, (a + b).CubicCentimeters);
    }

    [Fact]
    public void Operator_Subtract_Works()
    {
        var a = EngineDisplacement.FromCubicCentimeters(2000);
        var b = EngineDisplacement.FromCubicCentimeters(500);
        Assert.Equal(1500m, (a - b).CubicCentimeters);
    }

    [Fact]
    public void Operator_Multiply_Works()
    {
        var a = EngineDisplacement.FromCubicCentimeters(1000);
        Assert.Equal(2000m, (a * 2m).CubicCentimeters);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = EngineDisplacement.Parse("2000 cc");
        var b = EngineDisplacement.Parse("2000 cc");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = EngineDisplacement.Parse("2000 cc");
        var b = EngineDisplacement.Parse("1600 cc");
        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = EngineDisplacement.Parse("2000 cc");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void Operator_Divide_Works()
    {
        var a = EngineDisplacement.FromCubicCentimeters(2000);
        Assert.Equal(1000m, (a / 2m).CubicCentimeters);
    }

    [Fact]
    public void Operator_Multiply_Commutative()
    {
        var a = EngineDisplacement.FromCubicCentimeters(1000);
        Assert.Equal(2000m, (2m * a).CubicCentimeters);
    }

    [Fact]
    public void Operator_Negate_Works()
    {
        var a = EngineDisplacement.FromCubicCentimeters(2000);
        Assert.Equal(-2000m, (-a).CubicCentimeters);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = EngineDisplacement.Parse("1600 cc");
        var b = EngineDisplacement.Parse("2000 cc");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = EngineDisplacement.Parse("1998 cc");
        Assert.Equal(1, a.CompareTo(null));
    }
}
