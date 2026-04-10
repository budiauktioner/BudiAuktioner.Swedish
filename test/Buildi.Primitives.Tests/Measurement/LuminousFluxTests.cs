using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class LuminousFluxTests
{
    [Theory]
    [InlineData("800 lm")]
    [InlineData("1000 lm")]
    [InlineData("2.5 klm")]
    [InlineData("100 lm")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(LuminousFlux.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(LuminousFlux.IsValid(input));
    }

    [Theory]
    [InlineData("800 lm", 800)]
    [InlineData("2.5 klm", 2500)]
    [InlineData("1000 lm", 1000)]
    public void TryParse_ReturnsExpected_Lumens(string input, double expectedLumens)
    {
        Assert.True(LuminousFlux.TryParse(input, out var result));
        Assert.Equal((decimal)expectedLumens, result!.Lumens);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(LuminousFlux.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => LuminousFlux.Parse(input));
    }

    [Theory]
    [InlineData("800 lm", "800 lm")]
    [InlineData("2.5 klm", "2.5 klm")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, LuminousFlux.Format(input));
    }

    [Theory]
    [InlineData("800 lm", "800 lm")]
    [InlineData("2.5 klm", "2500 lm")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, LuminousFlux.Normalize(input));
    }

    [Theory]
    [InlineData("800 lm", "800 lm")]
    [InlineData("2.5 klm", "2.5 klm")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var flux = LuminousFlux.Parse(input);
        Assert.Equal(expected, flux.ToString());
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var flux = LuminousFlux.FromLumens(1000);
        Assert.Equal(1000m, flux.Lumens);
        Assert.Equal(1m, flux.Kilolumens);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var flux = LuminousFlux.FromKilolumens(2);
        Assert.Equal(2000m, flux.In(LuminousFluxUnit.Lumen));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = LuminousFlux.FromLumens(500);
        var b = LuminousFlux.FromLumens(300);
        Assert.Equal(800m, (a + b).Lumens);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = LuminousFlux.FromLumens(500);
        var b = LuminousFlux.FromLumens(1000);
        Assert.True(a < b);
        Assert.True(b > a);
    }

    [Fact]
    public void Equality_SameValue()
    {
        var a = LuminousFlux.FromLumens(1000);
        var b = LuminousFlux.FromKilolumens(1);
        Assert.True(a == b);
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var flux = LuminousFlux.Parse("2.5 klm");
        Assert.Same(LuminousFluxUnit.Kilolumen, flux.OriginalUnit);
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(LuminousFlux.IsNormalized("800 lm"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(LuminousFlux.IsNormalized("2.5 klm"));
    }

    [Theory]
    [InlineData("lm", "lm")]
    [InlineData("klm", "klm")]
    [InlineData("lumen", "lm")]
    [InlineData("lumens", "lm")]
    [InlineData("kilolumen", "klm")]
    [InlineData("kilolumens", "klm")]
    public void LuminousFluxUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(LuminousFluxUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 lm")]
    [InlineData("2.5 klm")]
    [InlineData("1 000 lm")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(LuminousFlux.IsValid(input));
    }

    [Fact]
    public void FindCandidatesInText_FindsLumenValues()
    {
        var text = "This LED produces 800 lm and the other one 2.5 klm.";
        var candidates = LuminousFlux.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var flux = LuminousFlux.Parse("800 lm");
        Assert.Equal("*** lm", flux.ToMaskedString());
    }

    [Fact]
    public void ToNaturalString_PicksBestUnit()
    {
        var flux = LuminousFlux.FromLumens(2500);
        Assert.Equal("2.5 klm", flux.ToNaturalString());
    }
}
