using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class AreaTests
{
    [Theory]
    [InlineData("100 m²")]
    [InlineData("100 m2")]
    [InlineData("2 ha")]
    [InlineData("1 acre")]
    [InlineData("500 cm²")]
    [InlineData("500 cm2")]
    [InlineData("1 km²")]
    [InlineData("10000 mm²")]
    [InlineData("50 sq ft")]
    [InlineData("100 sq in")]
    [InlineData("10 sq yd")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Area.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("100")]
    [InlineData("100 xyz")]
    [InlineData("ha 2")]
    [InlineData("10 m")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Area.IsValid(input));
    }

    [Theory]
    [InlineData("100 m²", 100)]
    [InlineData("100 m2", 100)]
    [InlineData("1 ha", 10000)]
    [InlineData("1 km²", 1000000)]
    [InlineData("10000 cm²", 1)]
    public void TryParse_ReturnsExpected_SquareMeters(string input, double expectedSquareMeters)
    {
        Assert.True(Area.TryParse(input, out var result));
        Assert.Equal((decimal)expectedSquareMeters, result!.SquareMeters);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Area.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("100 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Area.Parse(input));
    }

    [Theory]
    [InlineData("2 ha", "2 ha")]
    [InlineData("100 m²", "100 m²")]
    [InlineData("5.5 m2", "5.5 m²")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Area.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("10000 m²", Area.Format("1 ha", unit: AreaUnit.SquareMeter));
        Assert.Equal("1 ha", Area.Format("10000 m²", unit: AreaUnit.Hectare));
        Assert.Equal("1 ha", Area.Format("1 ha"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("1 ha", Area.Format("1.456 ha", decimals: 0));
        Assert.Equal("1.5 ha", Area.Format("1.456 ha", decimals: 1));
        Assert.Equal("1.46 ha", Area.Format("1.456 ha", decimals: 2));
        Assert.Equal("1.456 ha", Area.Format("1.456 ha"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var area = Area.Parse("1.456 ha");
        Assert.Equal("1 ha", area.ToString(AreaUnit.Hectare, decimals: 0));
        Assert.Equal("1.5 ha", area.ToString(AreaUnit.Hectare, decimals: 1));
    }

    [Theory]
    [InlineData("2 ha", "20000 m²")]
    [InlineData("10000 cm²", "1 m²")]
    [InlineData("5 m²", "5 m²")]
    [InlineData("1000000 mm²", "1 m²")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Area.Normalize(input));
    }

    [Theory]
    [InlineData("2 ha", "2 ha")]
    [InlineData("5 m²", "5 m²")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var area = Area.Parse(input);
        Assert.Equal(expected, area.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var area = Area.FromHectares(1);
        Assert.Equal("10000 m²", area.ToString(AreaUnit.SquareMeter));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var area = Area.FromSquareMeters(10000);
        Assert.Equal(10000m, area.SquareMeters);
        Assert.Equal(1m, area.Hectares);
        Assert.Equal(0.01m, area.SquareKilometers);
        Assert.Equal(100000000m, area.SquareCentimeters);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var area = Area.FromHectares(1);
        Assert.Equal(10000m, area.In(AreaUnit.SquareMeter));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Area.FromSquareMeters(100);
        var b = Area.FromSquareMeters(200);
        Assert.Equal(300m, (a + b).SquareMeters);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Area.FromSquareMeters(200);
        var b = Area.FromSquareMeters(50);
        Assert.Equal(150m, (a - b).SquareMeters);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Area.FromSquareMeters(100);
        Assert.Equal(300m, (a * 3).SquareMeters);
        Assert.Equal(300m, (3 * a).SquareMeters);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Area.FromSquareMeters(300);
        Assert.Equal(100m, (a / 3).SquareMeters);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Area.FromSquareMeters(100);
        var b = Area.FromSquareMeters(200);
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
        var a = Area.FromSquareMeters(10000);
        var b = Area.FromHectares(1);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var area = Area.Parse("2 ha");
        Assert.Same(AreaUnit.Hectare, area.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsAreaValues()
    {
        var text = "The plot is 500 m² and the farm is 2 ha.";
        var candidates = Area.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var area = Area.Parse("2 ha");
        Assert.Equal("*** ha", area.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Area.IsNormalized("5 m²"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Area.IsNormalized("2 ha"));
    }

    [Theory]
    [InlineData("m²", "m²")]
    [InlineData("m2", "m²")]
    [InlineData("ha", "ha")]
    [InlineData("hektar", "ha")]
    [InlineData("km²", "km²")]
    [InlineData("kvadratmeter", "m²")]
    [InlineData("sq ft", "sq ft")]
    [InlineData("sqft", "sq ft")]
    [InlineData("square meters", "m²")]
    [InlineData("sqm", "m²")]
    [InlineData("square metre", "m²")]
    [InlineData("square metres", "m²")]
    [InlineData("square kilometers", "km²")]
    [InlineData("square kilometre", "km²")]
    [InlineData("square kilometres", "km²")]
    [InlineData("square millimeters", "mm²")]
    [InlineData("square millimetre", "mm²")]
    [InlineData("square centimeters", "cm²")]
    [InlineData("square centimetre", "cm²")]
    [InlineData("hectares", "ha")]
    [InlineData("acres", "acre")]
    [InlineData("square feet", "sq ft")]
    [InlineData("square inches", "sq in")]
    public void AreaUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(AreaUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Fact]
    public void LengthUnit_BareMeter_DoesNotParse_AsAreaUnit()
    {
        Assert.True(LengthUnit.TryParse("m", out var lengthUnit));
        Assert.Equal("m", lengthUnit!.Symbol);
        Assert.False(AreaUnit.TryParse("m", out _));
        Assert.True(AreaUnit.TryParse("m2", out var areaUnit));
        Assert.Equal("m²", areaUnit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 m²")]
    [InlineData("2.5 ha")]
    [InlineData("0,5 km²")]
    [InlineData("3.14 cm²")]
    [InlineData("1 000 m²")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Area.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 m²", 5.5)]
    [InlineData("2.5 ha", 25000)]
    [InlineData("0,5 km²", 500000)]
    [InlineData("1 000 m²", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedSqMeters)
    {
        Assert.True(Area.TryParse(input, out var result));
        Assert.Equal((decimal)expectedSqMeters, result!.SquareMeters);
    }

    [Theory]
    [InlineData("5,5 m²", "5.5 m²")]
    [InlineData("  10  m²  ", "10 m²")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Area.Format(input));
    }

    [Theory]
    [InlineData("2.5 ha", "25000 m²")]
    [InlineData("5,5 m²", "5.5 m²")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Area.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Area.FromSquareMeters(1.5m);
        var b = Area.FromSquareMeters(0.5m);
        Assert.Equal(2m, (a + b).SquareMeters);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Area.TryParse("99999999999999999999999 km²", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Area.Parse("10 m²");
        var b = Area.Parse("20 m²");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Area.Parse("10 m²");
        Assert.Equal(1, a.CompareTo(null));
    }
}
