using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class LengthTests
{
    [Theory]
    [InlineData("10 m")]
    [InlineData("5.5 km")]
    [InlineData("100 cm")]
    [InlineData("3 ft")]
    [InlineData("12 in")]
    [InlineData("1 mi")]
    [InlineData("1 nmi")]
    [InlineData("1 NM")]
    [InlineData("500 mm")]
    [InlineData("2.5 yd")]
    [InlineData("1 mil")]
    [InlineData("100 nm")]
    [InlineData("5 µm")]
    [InlineData("3 dm")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Length.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("km 10")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Length.IsValid(input));
    }

    [Theory]
    [InlineData("10 m", 10)]
    [InlineData("1 km", 1000)]
    [InlineData("100 cm", 1)]
    [InlineData("1000 mm", 1)]
    [InlineData("1 mil", 10000)]
    [InlineData("1 NM", 1852)]
    [InlineData("1000000000 nm", 1)]
    [InlineData("1000000 µm", 1)]
    [InlineData("10 dm", 1)]
    public void TryParse_ReturnsExpected_Meters(string input, double expectedMeters)
    {
        Assert.True(Length.TryParse(input, out var result));
        Assert.Equal((decimal)expectedMeters, result!.Meters);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Length.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Length.Parse(input));
    }

    [Theory]
    [InlineData("10 km", "10 km")]
    [InlineData("100 cm", "100 cm")]
    [InlineData("5.5 m", "5.5 m")]
    [InlineData("100 nm", "100 nm")]
    [InlineData("5 µm", "5 µm")]
    [InlineData("3 dm", "3 dm")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Length.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("10000 m", Length.Format("10 km", unit: LengthUnit.Meter));
        Assert.Equal("10 km", Length.Format("10000 m", unit: LengthUnit.Kilometer));
        Assert.Equal("10 km", Length.Format("10 km"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("10 km", Length.Format("10.456 km", decimals: 0));
        Assert.Equal("10.5 km", Length.Format("10.456 km", decimals: 1));
        Assert.Equal("10.46 km", Length.Format("10.456 km", decimals: 2));
        Assert.Equal("10.456 km", Length.Format("10.456 km"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var length = Length.Parse("10.456 km");
        Assert.Equal("10 km", length.ToString(LengthUnit.Kilometer, decimals: 0));
        Assert.Equal("10.5 km", length.ToString(LengthUnit.Kilometer, decimals: 1));
        Assert.Equal("10456 m", length.ToString(LengthUnit.Meter, decimals: 0));
    }

    [Theory]
    [InlineData("10 km", "10000 m")]
    [InlineData("100 cm", "1 m")]
    [InlineData("5 m", "5 m")]
    [InlineData("1000 mm", "1 m")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Length.Normalize(input));
    }

    [Theory]
    [InlineData("10 km", "10 km")]
    [InlineData("5 m", "5 m")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var length = Length.Parse(input);
        Assert.Equal(expected, length.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var length = Length.FromKilometers(1);
        Assert.Equal("1000 m", length.ToString(LengthUnit.Meter));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var length = Length.FromMeters(1000);
        Assert.Equal(1000m, length.Meters);
        Assert.Equal(1m, length.Kilometers);
        Assert.Equal(100000m, length.Centimeters);
        Assert.Equal(1000000m, length.Millimeters);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var length = Length.FromKilometers(1);
        Assert.Equal(1000m, length.In(LengthUnit.Meter));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Length.FromMeters(10);
        var b = Length.FromMeters(20);
        Assert.Equal(30m, (a + b).Meters);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Length.FromMeters(20);
        var b = Length.FromMeters(5);
        Assert.Equal(15m, (a - b).Meters);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Length.FromMeters(10);
        Assert.Equal(30m, (a * 3).Meters);
        Assert.Equal(30m, (3 * a).Meters);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Length.FromMeters(30);
        Assert.Equal(10m, (a / 3).Meters);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Length.FromMeters(10);
        var b = Length.FromMeters(20);
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
        var a = Length.FromMeters(10);
        var b = Length.FromKilometers(0.01m);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var length = Length.Parse("5 km");
        Assert.Same(LengthUnit.Kilometer, length.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsLengthValues()
    {
        var text = "The room is 5 m wide and the road is 10 km long.";
        var candidates = Length.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var length = Length.Parse("10 km");
        Assert.Equal("*** km", length.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Length.IsNormalized("5 m"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Length.IsNormalized("5 km"));
    }

    [Theory]
    [InlineData("m", "m")]
    [InlineData("km", "km")]
    [InlineData("centimeter", "cm")]
    [InlineData("inches", "in")]
    [InlineData("feet", "ft")]
    [InlineData("tum", "in")]
    [InlineData("meters", "m")]
    [InlineData("metre", "m")]
    [InlineData("metres", "m")]
    [InlineData("kilometers", "km")]
    [InlineData("kilometre", "km")]
    [InlineData("kilometres", "km")]
    [InlineData("centimeters", "cm")]
    [InlineData("centimetre", "cm")]
    [InlineData("centimetres", "cm")]
    [InlineData("millimeters", "mm")]
    [InlineData("millimetre", "mm")]
    [InlineData("millimetres", "mm")]
    [InlineData("yards", "yd")]
    [InlineData("miles", "mi")]
    [InlineData("nm", "nm")]
    [InlineData("NM", "nmi")]
    [InlineData("nmi", "nmi")]
    [InlineData("nautical mile", "nmi")]
    [InlineData("sjömil", "nmi")]
    [InlineData("nanometer", "nm")]
    [InlineData("nanometers", "nm")]
    [InlineData("nanometre", "nm")]
    [InlineData("nanometres", "nm")]
    [InlineData("µm", "µm")]
    [InlineData("um", "µm")]
    [InlineData("micrometer", "µm")]
    [InlineData("micrometers", "µm")]
    [InlineData("micrometre", "µm")]
    [InlineData("micrometres", "µm")]
    [InlineData("dm", "dm")]
    [InlineData("decimeter", "dm")]
    [InlineData("decimeters", "dm")]
    [InlineData("decimetre", "dm")]
    [InlineData("decimetres", "dm")]
    public void LengthUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(LengthUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 km")]
    [InlineData("3,14 m")]
    [InlineData("0,5 mi")]
    [InlineData("2.5 km")]
    [InlineData("10,0 cm")]
    [InlineData("1 000 m")]
    [InlineData("1.000,5 km")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Length.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 km", 5500)]
    [InlineData("2.5 km", 2500)]
    [InlineData("3,14 m", 3.14)]
    [InlineData("0.5 mi", 804.672)]
    [InlineData("1 000 m", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedMeters)
    {
        Assert.True(Length.TryParse(input, out var result));
        Assert.Equal((decimal)expectedMeters, result!.Meters);
    }

    [Theory]
    [InlineData("5,5 km", "5.5 km")]
    [InlineData("2.50 km", "2.5 km")]
    [InlineData("  10  m  ", "10 m")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Length.Format(input));
    }

    [Theory]
    [InlineData("2.5 km", "2500 m")]
    [InlineData("5,5 km", "5500 m")]
    [InlineData("0.5 m", "0.5 m")]
    [InlineData("1 000 m", "1000 m")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Length.Normalize(input));
    }

    [Theory]
    [InlineData("5.5 km", "5.5 km")]
    [InlineData("3.14 m", "3.14 m")]
    public void ToString_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        var length = Length.Parse(input);
        Assert.Equal(expected, length.ToString());
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Length.FromKilometers(1.5m);
        var b = Length.FromMeters(500);
        Assert.Equal(2000m, (a + b).Meters);
    }

    [Fact]
    public void Conversions_NanometerToMeter()
    {
        var length = Length.Parse("1000000000 nm");
        Assert.Equal(1m, length.Meters);
    }

    [Fact]
    public void Conversions_MicrometerToMeter()
    {
        var length = Length.Parse("1000000 µm");
        Assert.Equal(1m, length.Meters);
    }

    [Fact]
    public void Conversions_DecimeterToMeter()
    {
        var length = Length.Parse("10 dm");
        Assert.Equal(1m, length.Meters);
    }

    [Fact]
    public void FromFactory_Nanometers()
    {
        var length = Length.FromNanometers(500);
        Assert.Equal(500m, length.Nanometers);
        Assert.Equal(0.0000005m, length.Meters);
    }

    [Fact]
    public void FromFactory_Micrometers()
    {
        var length = Length.FromMicrometers(500);
        Assert.Equal(500m, length.Micrometers);
        Assert.Equal(0.0005m, length.Meters);
    }

    [Fact]
    public void FromFactory_Decimeters()
    {
        var length = Length.FromDecimeters(5);
        Assert.Equal(5m, length.Decimeters);
        Assert.Equal(0.5m, length.Meters);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Length.TryParse("99999999999999999999999999 km", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Length.Parse("1 m");
        var b = Length.Parse("2 m");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Length.Parse("1 m");
        Assert.Equal(1, a.CompareTo(null));
    }
}
