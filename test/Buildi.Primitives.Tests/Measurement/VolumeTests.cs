using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class VolumeTests
{
    [Theory]
    [InlineData("2 L")]
    [InlineData("500 mL")]
    [InlineData("1 dL")]
    [InlineData("100 cL")]
    [InlineData("0.001 m³")]
    [InlineData("1 m3")]
    [InlineData("1 cbm")]
    [InlineData("1 kubikmeter")]
    [InlineData("1 gal")]
    [InlineData("2 pt")]
    [InlineData("8 fl oz")]
    [InlineData("1 cup")]
    [InlineData("100 µL")]
    [InlineData("5 hL")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Volume.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("L 2")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Volume.IsValid(input));
    }

    [Theory]
    [InlineData("1 L", 1)]
    [InlineData("1000 mL", 1)]
    [InlineData("10 dL", 1)]
    [InlineData("1 m³", 1000)]
    [InlineData("1 m3", 1000)]
    public void TryParse_ReturnsExpected_Liters(string input, double expectedLiters)
    {
        Assert.True(Volume.TryParse(input, out var result));
        Assert.Equal((decimal)expectedLiters, result!.Liters);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Volume.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Volume.Parse(input));
    }

    [Theory]
    [InlineData("2 L", "2 L")]
    [InlineData("500 mL", "500 mL")]
    [InlineData("1.5 dL", "1.5 dL")]
    [InlineData("100 µL", "100 µL")]
    [InlineData("5 hL", "5 hL")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Volume.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 mL", Volume.Format("1 L", unit: VolumeUnit.Milliliter));
        Assert.Equal("1 L", Volume.Format("1000 mL", unit: VolumeUnit.Liter));
        Assert.Equal("1 L", Volume.Format("1 L"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("2 L", Volume.Format("1.567 L", decimals: 0));
        Assert.Equal("1.6 L", Volume.Format("1.567 L", decimals: 1));
        Assert.Equal("1.57 L", Volume.Format("1.567 L", decimals: 2));
        Assert.Equal("1.567 L", Volume.Format("1.567 L"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var vol = Volume.Parse("1.567 L");
        Assert.Equal("2 L", vol.ToString(VolumeUnit.Liter, decimals: 0));
        Assert.Equal("1567 mL", vol.ToString(VolumeUnit.Milliliter, decimals: 0));
    }

    [Theory]
    [InlineData("1000 mL", "1 L")]
    [InlineData("10 dL", "1 L")]
    [InlineData("2 L", "2 L")]
    [InlineData("1 m³", "1000 L")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Volume.Normalize(input));
    }

    [Theory]
    [InlineData("2 L", "2 L")]
    [InlineData("500 mL", "500 mL")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var volume = Volume.Parse(input);
        Assert.Equal(expected, volume.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var volume = Volume.FromLiters(1);
        Assert.Equal("1000 mL", volume.ToString(VolumeUnit.Milliliter));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var volume = Volume.FromLiters(1);
        Assert.Equal(1m, volume.Liters);
        Assert.Equal(1000m, volume.Milliliters);
        Assert.Equal(100m, volume.Centiliters);
        Assert.Equal(10m, volume.Deciliters);
        Assert.Equal(0.001m, volume.CubicMeters);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var volume = Volume.FromLiters(1);
        Assert.Equal(1000m, volume.In(VolumeUnit.Milliliter));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Volume.FromLiters(1);
        var b = Volume.FromLiters(2);
        Assert.Equal(3m, (a + b).Liters);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Volume.FromLiters(3);
        var b = Volume.FromLiters(1);
        Assert.Equal(2m, (a - b).Liters);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Volume.FromLiters(2);
        Assert.Equal(6m, (a * 3).Liters);
        Assert.Equal(6m, (3 * a).Liters);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Volume.FromLiters(6);
        Assert.Equal(2m, (a / 3).Liters);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Volume.FromLiters(1);
        var b = Volume.FromLiters(2);
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
        var a = Volume.FromLiters(1);
        var b = Volume.FromMilliliters(1000);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var volume = Volume.Parse("500 mL");
        Assert.Same(VolumeUnit.Milliliter, volume.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsVolumeValues()
    {
        var text = "Use 2 L water and 500 mL oil.";
        var candidates = Volume.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var volume = Volume.Parse("2 L");
        Assert.Equal("*** L", volume.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Volume.IsNormalized("5 L"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Volume.IsNormalized("500 mL"));
    }

    [Theory]
    [InlineData("L", "L")]
    [InlineData("mL", "mL")]
    [InlineData("deciliter", "dL")]
    [InlineData("liter", "L")]
    [InlineData("kubikmeter", "m³")]
    [InlineData("gallon", "gal")]
    [InlineData("fluid ounce", "fl oz")]
    [InlineData("kopp", "cup")]
    [InlineData("liters", "L")]
    [InlineData("litre", "L")]
    [InlineData("litres", "L")]
    [InlineData("milliliters", "mL")]
    [InlineData("millilitre", "mL")]
    [InlineData("millilitres", "mL")]
    [InlineData("centiliters", "cL")]
    [InlineData("centilitre", "cL")]
    [InlineData("centilitres", "cL")]
    [InlineData("deciliters", "dL")]
    [InlineData("decilitre", "dL")]
    [InlineData("decilitres", "dL")]
    [InlineData("cubic metres", "m³")]
    [InlineData("cubic metre", "m³")]
    [InlineData("gallons", "gal")]
    [InlineData("fluid ounces", "fl oz")]
    [InlineData("floz", "fl oz")]
    [InlineData("pints", "pt")]
    [InlineData("cups", "cup")]
    [InlineData("koppar", "cup")]
    [InlineData("µL", "µL")]
    [InlineData("µl", "µL")]
    [InlineData("ul", "µL")]
    [InlineData("microliter", "µL")]
    [InlineData("microliters", "µL")]
    [InlineData("microlitre", "µL")]
    [InlineData("microlitres", "µL")]
    [InlineData("mikroliter", "µL")]
    [InlineData("hL", "hL")]
    [InlineData("hl", "hL")]
    [InlineData("hectoliter", "hL")]
    [InlineData("hectoliters", "hL")]
    [InlineData("hectolitre", "hL")]
    [InlineData("hectolitres", "hL")]
    [InlineData("hektoliter", "hL")]
    public void VolumeUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(VolumeUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 L")]
    [InlineData("2.5 dL")]
    [InlineData("0,5 gal")]
    [InlineData("3.14 mL")]
    [InlineData("1 000 mL")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Volume.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 L", 5.5)]
    [InlineData("2.5 dL", 0.25)]
    [InlineData("1 000 mL", 1)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedLiters)
    {
        Assert.True(Volume.TryParse(input, out var result));
        Assert.Equal((decimal)expectedLiters, result!.Liters);
    }

    [Theory]
    [InlineData("5,5 L", "5.5 L")]
    [InlineData("  10  L  ", "10 L")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Volume.Format(input));
    }

    [Theory]
    [InlineData("2.5 dL", "0.25 L")]
    [InlineData("5,5 L", "5.5 L")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Volume.Normalize(input));
    }

    [Theory]
    [InlineData("5.5 L", "5.5 L")]
    [InlineData("3.14 mL", "3.14 mL")]
    public void ToString_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        var volume = Volume.Parse(input);
        Assert.Equal(expected, volume.ToString());
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Volume.FromLiters(1.5m);
        var b = Volume.FromMilliliters(500);
        Assert.Equal(2m, (a + b).Liters);
    }

    [Fact]
    public void Conversions_MicroliterToLiter()
    {
        var volume = Volume.Parse("1000000 µL");
        Assert.Equal(1m, volume.Liters);
    }

    [Fact]
    public void Conversions_HectoliterToLiter()
    {
        var volume = Volume.Parse("1 hL");
        Assert.Equal(100m, volume.Liters);
    }

    [Fact]
    public void FromFactory_Microliters()
    {
        var volume = Volume.FromMicroliters(500);
        Assert.Equal(500m, volume.Microliters);
    }

    [Fact]
    public void FromFactory_Hectoliters()
    {
        var volume = Volume.FromHectoliters(2);
        Assert.Equal(2m, volume.Hectoliters);
        Assert.Equal(200m, volume.Liters);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Volume.TryParse("99999999999999999999999999 m³", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Volume.Parse("1 L");
        var b = Volume.Parse("2 L");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Volume.Parse("1 L");
        Assert.Equal(1, a.CompareTo(null));
    }
}
