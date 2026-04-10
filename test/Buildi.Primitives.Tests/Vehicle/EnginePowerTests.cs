using Buildi.Primitives.Measurement;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class EnginePowerTests
{
    [Theory]
    [InlineData("150")]
    [InlineData("150 HP")]
    [InlineData("110 kW")]
    [InlineData("150 hk")]
    [InlineData("100 W")]
    [InlineData("ca 512")]
    [InlineData("ca. 150")]
    [InlineData("ca 150 hk")]
    [InlineData("circa 200")]
    [InlineData("~300")]
    [InlineData("~ 300")]
    [InlineData("ungefär 150")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(EnginePower.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("-150 HP")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EnginePower.IsValid(input));
    }

    [Fact]
    public void TryParse_BareNumber_DefaultsToHP()
    {
        Assert.True(EnginePower.TryParse("150", out var result));
        Assert.Equal(150m, result!.Horsepower);
        Assert.Equal("150 HP", result.Value);
    }

    [Fact]
    public void TryParse_Hk_ParsesAsHP()
    {
        Assert.True(EnginePower.TryParse("150 hk", out var result));
        Assert.Equal(150m, result!.Horsepower);
        Assert.Equal("150 HP", result.Value);
    }

    [Fact]
    public void TryParse_KW_ParsesCorrectly()
    {
        Assert.True(EnginePower.TryParse("110 kW", out var result));
        Assert.Equal(110m, result!.Kilowatts);
    }

    [Theory]
    [InlineData("150 HP", "150 HP")]
    [InlineData("150 hk", "150 HP")]
    [InlineData("110 kW", "110 kW")]
    [InlineData("150", "150 HP")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, EnginePower.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 W", EnginePower.Format("1 kW", unit: PowerUnit.Watt));
    }

    [Fact]
    public void Comparison_Works()
    {
        var a = EnginePower.Parse("100 HP");
        var b = EnginePower.Parse("200 HP");
        Assert.True(a < b);
    }

    [Theory]
    [InlineData("abc")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => EnginePower.Parse(input));
    }

    [Fact]
    public void Create_FromDecimalAndUnit_Works()
    {
        var ep = EnginePower.Create(150m, PowerUnit.Horsepower);
        Assert.True(ep.Horsepower > 0);
    }

    [Fact]
    public void Create_FromIntAndUnit_Works()
    {
        var ep = EnginePower.Create(150, PowerUnit.Horsepower);
        Assert.True(ep.Horsepower > 0);
    }

    [Fact]
    public void Create_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnginePower.Create(0m, PowerUnit.Horsepower));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnginePower.Create(-1m, PowerUnit.Horsepower));
    }

    [Fact]
    public void FromHorsepower_Decimal_Works()
    {
        var ep = EnginePower.FromHorsepower(150m);
        Assert.Equal(150m, ep.Horsepower);
    }

    [Fact]
    public void FromHorsepower_Int_Works()
    {
        var ep = EnginePower.FromHorsepower(150);
        Assert.Equal(150m, ep.Horsepower);
    }

    [Fact]
    public void FromKilowatts_Works()
    {
        var ep = EnginePower.FromKilowatts(110m);
        Assert.Equal(110m, ep.Kilowatts);
    }

    [Fact]
    public void FromWatts_Works()
    {
        var ep = EnginePower.FromWatts(110000m);
        Assert.Equal(110000m, ep.Watts);
    }

    [Fact]
    public void Create_EqualsStringParsed()
    {
        var fromFactory = EnginePower.FromHorsepower(150);
        var fromString = EnginePower.Parse("150 HP");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void Operator_Add_CombinesPower()
    {
        var a = EnginePower.FromHorsepower(150);
        var b = EnginePower.FromHorsepower(150);
        Assert.Equal(300m, (a + b).Horsepower);
    }

    [Fact]
    public void Operator_Subtract_Works()
    {
        var a = EnginePower.FromHorsepower(300);
        var b = EnginePower.FromHorsepower(150);
        Assert.Equal(150m, (a - b).Horsepower);
    }

    [Fact]
    public void Operator_Multiply_ScalesPower()
    {
        var a = EnginePower.FromHorsepower(150);
        Assert.Equal(300m, (a * 2m).Horsepower);
        Assert.Equal(300m, (2m * a).Horsepower);
    }

    [Fact]
    public void Operator_Divide_Works()
    {
        var a = EnginePower.FromHorsepower(300);
        Assert.Equal(150m, (a / 2m).Horsepower);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = EnginePower.Parse("150 hp");
        var b = EnginePower.Parse("150 hp");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = EnginePower.Parse("150 hp");
        var b = EnginePower.Parse("200 hp");
        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = EnginePower.Parse("150 hp");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void Operator_Negate_Works()
    {
        var a = EnginePower.FromHorsepower(150);
        var result = -a;
        Assert.Equal(-150m, result.Horsepower);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = EnginePower.Parse("100 hp");
        var b = EnginePower.Parse("200 hp");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = EnginePower.Parse("150 HP");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData("ca 512", 512)]
    [InlineData("ca. 150", 150)]
    [InlineData("circa 200", 200)]
    [InlineData("~300", 300)]
    [InlineData("ca 150 hk", 150)]
    [InlineData("ca 110 kW", 110)]
    public void TryParse_StripsApproximatePrefix(string input, decimal expectedValue)
    {
        Assert.True(EnginePower.TryParse(input, out var result));
        Assert.NotNull(result);
        if (input.Contains("kW"))
            Assert.Equal(expectedValue, result.Kilowatts);
        else
            Assert.Equal(expectedValue, result.Horsepower);
    }
}
