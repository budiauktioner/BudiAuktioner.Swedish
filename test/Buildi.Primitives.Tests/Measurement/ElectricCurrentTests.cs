using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class ElectricCurrentTests
{
    [Theory]
    [InlineData("10 A")]
    [InlineData("500 mA")]
    [InlineData("16 A")]
    [InlineData("32 A")]
    [InlineData("100 µA")]
    [InlineData("2.5 kA")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ElectricCurrent.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("10")]
    [InlineData("10 xyz")]
    [InlineData("A 10")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ElectricCurrent.IsValid(input));
    }

    [Theory]
    [InlineData("10 A", 10)]
    [InlineData("1 kA", 1000)]
    [InlineData("1000 mA", 1)]
    [InlineData("500 mA", 0.5)]
    public void TryParse_ReturnsExpected_Amperes(string input, double expectedAmperes)
    {
        Assert.True(ElectricCurrent.TryParse(input, out var result));
        Assert.Equal((decimal)expectedAmperes, result!.Amperes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(ElectricCurrent.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => ElectricCurrent.Parse(input));
    }

    [Theory]
    [InlineData("16 A", "16 A")]
    [InlineData("500 mA", "500 mA")]
    [InlineData("2.5 kA", "2.5 kA")]
    [InlineData("100 µA", "100 µA")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ElectricCurrent.Format(input));
    }

    [Theory]
    [InlineData("2.5 kA", "2500 A")]
    [InlineData("500 mA", "0.5 A")]
    [InlineData("16 A", "16 A")]
    [InlineData("1000 mA", "1 A")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ElectricCurrent.Normalize(input));
    }

    [Theory]
    [InlineData("16 A", "16 A")]
    [InlineData("500 mA", "500 mA")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var current = ElectricCurrent.Parse(input);
        Assert.Equal(expected, current.ToString());
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var current = ElectricCurrent.FromAmperes(1);
        Assert.Equal(1m, current.Amperes);
        Assert.Equal(1000m, current.Milliamperes);
        Assert.Equal(0.001m, current.Kiloamperes);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var current = ElectricCurrent.FromKiloamperes(1);
        Assert.Equal(1000m, current.In(ElectricCurrentUnit.Ampere));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = ElectricCurrent.FromAmperes(10);
        var b = ElectricCurrent.FromAmperes(20);
        Assert.Equal(30m, (a + b).Amperes);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = ElectricCurrent.FromAmperes(20);
        var b = ElectricCurrent.FromAmperes(5);
        Assert.Equal(15m, (a - b).Amperes);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = ElectricCurrent.FromAmperes(10);
        Assert.Equal(30m, (a * 3).Amperes);
        Assert.Equal(30m, (3 * a).Amperes);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = ElectricCurrent.FromAmperes(30);
        Assert.Equal(10m, (a / 3).Amperes);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = ElectricCurrent.FromAmperes(10);
        var b = ElectricCurrent.FromAmperes(20);
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
        var a = ElectricCurrent.FromAmperes(10);
        var b = ElectricCurrent.FromMilliamperes(10000);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var current = ElectricCurrent.Parse("5 kA");
        Assert.Same(ElectricCurrentUnit.Kiloampere, current.OriginalUnit);
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(ElectricCurrent.IsNormalized("16 A"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(ElectricCurrent.IsNormalized("5 kA"));
    }

    [Theory]
    [InlineData("A", "A")]
    [InlineData("kA", "kA")]
    [InlineData("mA", "mA")]
    [InlineData("ampere", "A")]
    [InlineData("amperes", "A")]
    [InlineData("amp", "A")]
    [InlineData("amps", "A")]
    [InlineData("µA", "µA")]
    [InlineData("uA", "µA")]
    [InlineData("microampere", "µA")]
    [InlineData("milliampere", "mA")]
    [InlineData("kiloampere", "kA")]
    public void ElectricCurrentUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(ElectricCurrentUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 A")]
    [InlineData("2.5 kA")]
    [InlineData("1 000 mA")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(ElectricCurrent.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 A", 5.5)]
    [InlineData("2.5 kA", 2500)]
    [InlineData("1 000 mA", 1)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedAmperes)
    {
        Assert.True(ElectricCurrent.TryParse(input, out var result));
        Assert.Equal((decimal)expectedAmperes, result!.Amperes);
    }

    [Fact]
    public void FindCandidatesInText_FindsCurrentValues()
    {
        var text = "The motor draws 16 A and the control board uses 500 mA.";
        var candidates = ElectricCurrent.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var current = ElectricCurrent.Parse("16 A");
        Assert.Equal("*** A", current.ToMaskedString());
    }

    [Fact]
    public void ToNaturalString_PicksBestUnit()
    {
        var current = ElectricCurrent.FromAmperes(0.005m);
        Assert.Equal("5 mA", current.ToNaturalString());
    }
}
