using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class WeightTests
{
    [Theory]
    [InlineData("10 kg")]
    [InlineData("5.5 g")]
    [InlineData("100 mg")]
    [InlineData("2 t")]
    [InlineData("1 metric ton")]
    [InlineData("3 lb")]
    [InlineData("8 oz")]
    [InlineData("12 st")]
    [InlineData("500 gram")]
    [InlineData("2 pund")]
    [InlineData("100 µg")]
    [InlineData("5 hg")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Weight.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("kg 10")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Weight.IsValid(input));
    }

    [Theory]
    [InlineData("10 kg", 10)]
    [InlineData("1000 g", 1)]
    [InlineData("1 t", 1000)]
    [InlineData("1000000 mg", 1)]
    public void TryParse_ReturnsExpected_Kilograms(string input, double expectedKg)
    {
        Assert.True(Weight.TryParse(input, out var result));
        Assert.Equal((decimal)expectedKg, result!.Kilograms);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Weight.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Weight.Parse(input));
    }

    [Theory]
    [InlineData("10 kg", "10 kg")]
    [InlineData("1000 g", "1000 g")]
    [InlineData("5.5 kg", "5.5 kg")]
    [InlineData("100 µg", "100 µg")]
    [InlineData("5 hg", "5 hg")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Weight.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 g", Weight.Format("1 kg", unit: WeightUnit.Gram));
        Assert.Equal("1 kg", Weight.Format("1000 g", unit: WeightUnit.Kilogram));
        Assert.Equal("1 kg", Weight.Format("1 kg"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("3 kg", Weight.Format("2.567 kg", decimals: 0));
        Assert.Equal("2.6 kg", Weight.Format("2.567 kg", decimals: 1));
        Assert.Equal("2.57 kg", Weight.Format("2.567 kg", decimals: 2));
        Assert.Equal("2.567 kg", Weight.Format("2.567 kg"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var w = Weight.Parse("2.567 kg");
        Assert.Equal("3 kg", w.ToString(WeightUnit.Kilogram, decimals: 0));
        Assert.Equal("2567 g", w.ToString(WeightUnit.Gram, decimals: 0));
    }

    [Theory]
    [InlineData("10 t", "10000 kg")]
    [InlineData("1000 g", "1 kg")]
    [InlineData("5 kg", "5 kg")]
    [InlineData("1000000 mg", "1 kg")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Weight.Normalize(input));
    }

    [Theory]
    [InlineData("10 kg", "10 kg")]
    [InlineData("5 kg", "5 kg")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var weight = Weight.Parse(input);
        Assert.Equal(expected, weight.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var weight = Weight.FromKilograms(1);
        Assert.Equal("1000 g", weight.ToString(WeightUnit.Gram));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var weight = Weight.FromKilograms(1);
        Assert.Equal(1m, weight.Kilograms);
        Assert.Equal(1000m, weight.Grams);
        Assert.Equal(1000000m, weight.Milligrams);
        Assert.Equal(0.001m, weight.MetricTons);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var weight = Weight.FromKilograms(1);
        Assert.Equal(1000m, weight.In(WeightUnit.Gram));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Weight.FromKilograms(10);
        var b = Weight.FromKilograms(20);
        Assert.Equal(30m, (a + b).Kilograms);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Weight.FromKilograms(20);
        var b = Weight.FromKilograms(5);
        Assert.Equal(15m, (a - b).Kilograms);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Weight.FromKilograms(10);
        Assert.Equal(30m, (a * 3).Kilograms);
        Assert.Equal(30m, (3 * a).Kilograms);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Weight.FromKilograms(30);
        Assert.Equal(10m, (a / 3).Kilograms);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Weight.FromKilograms(10);
        var b = Weight.FromKilograms(20);
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
        var a = Weight.FromKilograms(1);
        var b = Weight.FromGrams(1000);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var weight = Weight.Parse("5 g");
        Assert.Same(WeightUnit.Gram, weight.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsWeightValues()
    {
        var text = "Paketet väger 2 kg och lasten är 1 t.";
        var candidates = Weight.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var weight = Weight.Parse("10 kg");
        Assert.Equal("*** kg", weight.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Weight.IsNormalized("5 kg"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Weight.IsNormalized("5 g"));
    }

    [Theory]
    [InlineData("kg", "kg")]
    [InlineData("g", "g")]
    [InlineData("kilogram", "kg")]
    [InlineData("pounds", "lb")]
    [InlineData("pund", "lb")]
    [InlineData("uns", "oz")]
    [InlineData("tonne", "t")]
    [InlineData("metric ton", "t")]
    [InlineData("kilograms", "kg")]
    [InlineData("kilo", "kg")]
    [InlineData("kilos", "kg")]
    [InlineData("grams", "g")]
    [InlineData("milligrams", "mg")]
    [InlineData("tonnes", "t")]
    [InlineData("metric tons", "t")]
    [InlineData("ounces", "oz")]
    [InlineData("stones", "st")]
    [InlineData("µg", "µg")]
    [InlineData("ug", "µg")]
    [InlineData("microgram", "µg")]
    [InlineData("micrograms", "µg")]
    [InlineData("mikrogram", "µg")]
    [InlineData("hg", "hg")]
    [InlineData("hectogram", "hg")]
    [InlineData("hectograms", "hg")]
    [InlineData("hektogram", "hg")]
    [InlineData("hekto", "hg")]
    public void WeightUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(WeightUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 kg")]
    [InlineData("2.5 t")]
    [InlineData("0,5 lb")]
    [InlineData("3.14 g")]
    [InlineData("1 000 kg")]
    [InlineData("1.000,5 g")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Weight.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 kg", 5.5)]
    [InlineData("2.5 t", 2500)]
    [InlineData("3,14 g", 0.00314)]
    [InlineData("1 000 kg", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedKg)
    {
        Assert.True(Weight.TryParse(input, out var result));
        Assert.Equal((decimal)expectedKg, result!.Kilograms);
    }

    [Theory]
    [InlineData("5,5 kg", "5.5 kg")]
    [InlineData("  10  kg  ", "10 kg")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Weight.Format(input));
    }

    [Theory]
    [InlineData("2.5 t", "2500 kg")]
    [InlineData("5,5 kg", "5.5 kg")]
    [InlineData("1 000 g", "1 kg")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Weight.Normalize(input));
    }

    [Theory]
    [InlineData("5.5 kg", "5.5 kg")]
    [InlineData("3.14 g", "3.14 g")]
    public void ToString_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        var weight = Weight.Parse(input);
        Assert.Equal(expected, weight.ToString());
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Weight.FromKilograms(1.5m);
        var b = Weight.FromGrams(500);
        Assert.Equal(2m, (a + b).Kilograms);
    }

    [Fact]
    public void Conversions_MicrogramToKilogram()
    {
        var weight = Weight.Parse("1000000000 µg");
        Assert.Equal(1m, weight.Kilograms);
    }

    [Fact]
    public void Conversions_HectogramToKilogram()
    {
        var weight = Weight.Parse("10 hg");
        Assert.Equal(1m, weight.Kilograms);
    }

    [Fact]
    public void FromFactory_Micrograms()
    {
        var weight = Weight.FromMicrograms(500);
        Assert.Equal(500m, weight.Micrograms);
    }

    [Fact]
    public void FromFactory_Hectograms()
    {
        var weight = Weight.FromHectograms(5);
        Assert.Equal(5m, weight.Hectograms);
        Assert.Equal(0.5m, weight.Kilograms);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Weight.TryParse("99999999999999999999999999 t", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Weight.Parse("1 kg");
        var b = Weight.Parse("2 kg");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Weight.Parse("1 kg");
        Assert.Equal(1, a.CompareTo(null));
    }
}
