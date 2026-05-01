using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class SpeedTests
{
    [Theory]
    [InlineData("10 m/s")]
    [InlineData("100 km/h")]
    [InlineData("60 mph")]
    [InlineData("5 ft/s")]
    [InlineData("10 kn")]
    [InlineData("10 kph")]
    [InlineData("10 kmh")]
    [InlineData("10 km/t")]
    [InlineData("10 miles per hour")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Speed.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("km/h 100")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Speed.IsValid(input));
    }

    [Theory]
    [InlineData("10 m/s", 10)]
    [InlineData("3.6 km/h", 1)]
    [InlineData("360 km/h", 100)]
    public void TryParse_ReturnsExpected_MetersPerSecond(string input, double expectedMps)
    {
        Assert.True(Speed.TryParse(input, out var result));
        Assert.Equal((decimal)expectedMps, result!.MetersPerSecond);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Speed.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Speed.Parse(input));
    }

    [Theory]
    [InlineData("100 km/h", "100 km/h")]
    [InlineData("60 mph", "60 mph")]
    [InlineData("10 m/s", "10 m/s")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Speed.Format(input));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("121 km/h", Speed.Format("120.567 km/h", decimals: 0));
        Assert.Equal("120.6 km/h", Speed.Format("120.567 km/h", decimals: 1));
        Assert.Equal("120.567 km/h", Speed.Format("120.567 km/h"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var s = Speed.Parse("120.567 km/h");
        Assert.Equal("121 km/h", s.ToString(SpeedUnit.KilometersPerHour, decimals: 0));
        Assert.Equal("120.6 km/h", s.ToString(SpeedUnit.KilometersPerHour, decimals: 1));
    }

    [Theory]
    [InlineData("10 m/s", "10 m/s")]
    [InlineData("3.6 km/h", "1 m/s")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Speed.Normalize(input));
    }

    [Fact]
    public void Normalize_KilometersPerHour_ToMetersPerSecond_UsesConsistentFormatting()
    {
        var normalized = Speed.Normalize("100 km/h");
        Assert.NotNull(normalized);
        Assert.EndsWith(" m/s", normalized);
        Assert.Equal(Speed.Parse("100 km/h").ToNormalizedString(), normalized);
    }

    [Theory]
    [InlineData("100 km/h", "100 km/h")]
    [InlineData("10 m/s", "10 m/s")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var speed = Speed.Parse(input);
        Assert.Equal(expected, speed.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var speed = Speed.FromKilometersPerHour(100);
        Assert.Equal(speed.ToNormalizedString(), speed.ToString(SpeedUnit.MetersPerSecond));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var speed = Speed.FromMetersPerSecond(10);
        Assert.Equal(10m, speed.MetersPerSecond);
        Assert.Equal(36m, speed.KilometersPerHour);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var speed = Speed.FromKilometersPerHour(36);
        Assert.Equal(10m, speed.In(SpeedUnit.MetersPerSecond));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Speed.FromMetersPerSecond(10);
        var b = Speed.FromMetersPerSecond(20);
        Assert.Equal(30m, (a + b).MetersPerSecond);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Speed.FromMetersPerSecond(20);
        var b = Speed.FromMetersPerSecond(5);
        Assert.Equal(15m, (a - b).MetersPerSecond);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Speed.FromMetersPerSecond(10);
        Assert.Equal(30m, (a * 3).MetersPerSecond);
        Assert.Equal(30m, (3 * a).MetersPerSecond);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Speed.FromMetersPerSecond(30);
        Assert.Equal(10m, (a / 3).MetersPerSecond);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Speed.FromMetersPerSecond(10);
        var b = Speed.FromMetersPerSecond(20);
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
        var a = Speed.FromMetersPerSecond(10);
        var b = Speed.FromKilometersPerHour(36);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var speed = Speed.Parse("100 km/h");
        Assert.Same(SpeedUnit.KilometersPerHour, speed.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsSpeedValues()
    {
        var text = "The limit is 50 km/h and the wind was 10 m/s.";
        var candidates = Speed.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var speed = Speed.Parse("100 km/h");
        Assert.Equal("*** km/h", speed.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Speed.IsNormalized("5 m/s"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Speed.IsNormalized("5 km/h"));
    }

    [Theory]
    [InlineData("m/s", "m/s")]
    [InlineData("km/h", "km/h")]
    [InlineData("kph", "km/h")]
    [InlineData("kmh", "km/h")]
    [InlineData("km/t", "km/h")]
    [InlineData("mph", "mph")]
    [InlineData("miles per hour", "mph")]
    [InlineData("ft/s", "ft/s")]
    [InlineData("kn", "kn")]
    [InlineData("knot", "kn")]
    [InlineData("kt", "kn")]
    [InlineData("knop", "kn")]
    [InlineData("meters per second", "m/s")]
    [InlineData("kilometers per hour", "km/h")]
    [InlineData("kilometre per hour", "km/h")]
    [InlineData("kilometres per hour", "km/h")]
    [InlineData("knots", "kn")]
    [InlineData("feet per second", "ft/s")]
    public void SpeedUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(SpeedUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Fact]
    public void SpeedUnit_Knots_ReturnsKnotUnit()
    {
        Assert.Same(SpeedUnit.Knot, SpeedUnit.Knots);
        Assert.Equal("kn", SpeedUnit.Knots.Symbol);
    }

    [Theory]
    [InlineData("5,5 m/s")]
    [InlineData("2.5 mph")]
    [InlineData("0,5 kn")]
    [InlineData("3.14 m/s")]
    [InlineData("1 000 km/h")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Speed.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 m/s", "5.5")]
    [InlineData("1 000 km/h", "277.777777777777777777777777778")]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, string expectedMs)
    {
        Assert.True(Speed.TryParse(input, out var result));
        Assert.Equal(decimal.Parse(expectedMs, System.Globalization.CultureInfo.InvariantCulture), result!.MetersPerSecond);
    }

    [Theory]
    [InlineData("5,5 m/s", "5.5 m/s")]
    [InlineData("  100  km/h  ", "100 km/h")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Speed.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("10 m/s", Speed.Format("36 km/h", unit: SpeedUnit.MetersPerSecond));
        Assert.Equal("36 km/h", Speed.Format("10 m/s", unit: SpeedUnit.KilometersPerHour));
        Assert.Equal("90 km/h", Speed.Format("90 km/h"));
    }

    [Theory]
    [InlineData("5,5 m/s", "5.5 m/s")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Speed.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Speed.FromMetersPerSecond(1.5m);
        var b = Speed.FromMetersPerSecond(0.5m);
        Assert.Equal(2m, (a + b).MetersPerSecond);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Speed.TryParse("99999999999999999999999999999 km/h", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Speed.Parse("50 km/h");
        var b = Speed.Parse("100 km/h");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Speed.Parse("50 km/h");
        Assert.Equal(1, a.CompareTo(null));
    }
}
