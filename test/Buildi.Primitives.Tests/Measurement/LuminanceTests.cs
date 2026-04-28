using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class LuminanceTests
{
    [Theory]
    [InlineData("500 cd/m²")]
    [InlineData("500 cd/m2")]
    [InlineData("500 nit")]
    [InlineData("500 nits")]
    [InlineData("1.5 kcd/m²")]
    [InlineData("1.5 kcd/m2")]
    [InlineData("2 knit")]
    [InlineData("100 candela per square metre")]
    [InlineData("500")]
    [InlineData("0")]
    [InlineData("1500")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Luminance.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("500 xyz")]
    [InlineData("500 lm")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Luminance.IsValid(input));
    }

    [Theory]
    [InlineData("500", 500)]
    [InlineData("1500", 1500)]
    [InlineData("0", 0)]
    public void TryParse_BareNumber_DefaultsToCandelaPerSquareMetre(string input, double expected)
    {
        Assert.True(Luminance.TryParse(input, out var result));
        Assert.Equal((decimal)expected, result!.CandelaPerSquareMetre);
        Assert.Same(LuminanceUnit.CandelaPerSquareMetre, result.OriginalUnit);
    }

    [Theory]
    [InlineData("500 cd/m²", 500)]
    [InlineData("500 nits", 500)]
    [InlineData("1.5 kcd/m²", 1500)]
    [InlineData("2 knit", 2000)]
    public void TryParse_ReturnsExpected_CandelaPerSquareMetre(string input, double expected)
    {
        Assert.True(Luminance.TryParse(input, out var result));
        Assert.Equal((decimal)expected, result!.CandelaPerSquareMetre);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("xyz")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Luminance.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("not a luminance")]
    [InlineData("500 lm")]
    public void Parse_Throws_ForInvalid(string input) =>
        Assert.Throws<ArgumentException>(() => Luminance.Parse(input));

    [Theory]
    [InlineData("500 cd/m²", "500 cd/m²")]
    [InlineData("500 nits", "500 nit")]
    [InlineData("1.5 kcd/m²", "1.5 kcd/m²")]
    public void Format_PreservesOriginalUnit(string? input, string? expected) =>
        Assert.Equal(expected, Luminance.Format(input));

    [Theory]
    [InlineData("500 cd/m²", "500 cd/m²")]
    [InlineData("500 nits", "500 cd/m²")]
    [InlineData("1.5 kcd/m²", "1500 cd/m²")]
    public void Normalize_ReturnsBaseUnit(string? input, string? expected) =>
        Assert.Equal(expected, Luminance.Normalize(input));

    [Theory]
    [InlineData("500 cd/m²", true)]
    [InlineData("1500 cd/m²", true)]
    [InlineData("1.5 kcd/m²", false)]
    [InlineData("500 nit", false)]
    public void IsNormalized_ReturnsExpected(string input, bool expected) =>
        Assert.Equal(expected, Luminance.IsNormalized(input));

    [Fact]
    public void NitsAndCandelaPerSquareMetre_AreEquivalent()
    {
        var l = Luminance.FromNits(500);
        Assert.Equal(500m, l.CandelaPerSquareMetre);
        Assert.Equal(500m, l.Nits);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var l = Luminance.FromKilocandelaPerSquareMetre(2);
        Assert.Equal(2000m, l.In(LuminanceUnit.CandelaPerSquareMetre));
        Assert.Equal(2m, l.In(LuminanceUnit.KilocandelaPerSquareMetre));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Luminance.FromNits(500);
        var b = Luminance.FromNits(300);
        Assert.Equal(800m, (a + b).CandelaPerSquareMetre);
    }

    [Fact]
    public void Equality_SameValue_DifferentUnits()
    {
        var a = Luminance.FromNits(1000);
        var b = Luminance.FromKilocandelaPerSquareMetre(1);
        Assert.True(a == b);
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var l = Luminance.Parse("1.5 kcd/m²");
        Assert.Same(LuminanceUnit.KilocandelaPerSquareMetre, l.OriginalUnit);
    }

    [Fact]
    public void ToNaturalString_PicksKiloUnitForLargeValues()
    {
        var l = Luminance.FromCandelaPerSquareMetre(2500);
        Assert.Equal("2.5 kcd/m²", l.ToNaturalString());
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var l = Luminance.Parse("500 cd/m²");
        Assert.Equal("*** cd/m²", l.ToMaskedString());
    }

    [Theory]
    [InlineData("cd/m²", "cd/m²")]
    [InlineData("cd/m2", "cd/m²")]
    [InlineData("nit", "nit")]
    [InlineData("nits", "nit")]
    [InlineData("kcd/m²", "kcd/m²")]
    [InlineData("kcd/m2", "kcd/m²")]
    [InlineData("knit", "knit")]
    public void LuminanceUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(LuminanceUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Fact]
    public void FindCandidatesInText_FindsLuminanceValues()
    {
        var text = "The display peaks at 1500 nits and the prior model at 500 cd/m².";
        var candidates = Luminance.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void TypeInfo_HasExpectedMetadata()
    {
        Assert.Equal("Luminance", Luminance.TypeInfo.EnglishName);
        Assert.Equal("Luminans", Luminance.TypeInfo.LocalizedName);
    }
}
