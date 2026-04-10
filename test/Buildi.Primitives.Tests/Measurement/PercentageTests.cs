using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class PercentageTests
{
    [Theory]
    [InlineData("85%")]
    [InlineData("85 %")]
    [InlineData("0.85")]
    [InlineData("50 procent")]
    [InlineData("85 percent")]
    [InlineData("100%")]
    [InlineData("0%")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Percentage.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("101%")]
    [InlineData("-1%")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Percentage.IsValid(input));
    }

    [Theory]
    [InlineData("85%", 0.85, 85)]
    [InlineData("85 %", 0.85, 85)]
    [InlineData("0.85", 0.85, 85)]
    [InlineData("50 procent", 0.5, 50)]
    [InlineData("100%", 1, 100)]
    [InlineData("0%", 0, 0)]
    public void TryParse_SetsValueAndPercent(string input, decimal expectedValue, decimal expectedPercent)
    {
        Assert.True(Percentage.TryParse(input, out var result));
        Assert.NotNull(result);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedPercent, result.Percent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Percentage.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("101%")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Percentage.Parse(input));
    }

    [Theory]
    [InlineData("85%", "85%")]
    [InlineData("0.85", "85%")]
    [InlineData("50 procent", "50%")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Percentage.Format(input));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("86%", Percentage.Format("85.567%", decimals: 0));
        Assert.Equal("85.6%", Percentage.Format("85.567%", decimals: 1));
        Assert.Equal("85.567%", Percentage.Format("85.567%"));
    }

    [Theory]
    [InlineData("85%", "0.85")]
    [InlineData("85 %", "0.85")]
    [InlineData("50 procent", "0.5")]
    [InlineData("100%", "1")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Percentage.Normalize(input));
    }

    [Theory]
    [InlineData("85%", "85%")]
    [InlineData("0.5", "50%")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var p = Percentage.Parse(input);
        Assert.Equal(expected, p.ToString());
    }

    [Theory]
    [InlineData("0.85", "0.85")]
    [InlineData("100%", "1")]
    public void ToNormalizedString_ReturnsDecimalForm(string input, string expected)
    {
        var p = Percentage.Parse(input);
        Assert.Equal(expected, p.ToNormalizedString());
    }

    [Fact]
    public void FromDecimal_AndFromPercent_MatchTryParse()
    {
        Assert.Equal(0.85m, Percentage.FromDecimal(0.85m).Value);
        Assert.Equal(85m, Percentage.FromPercent(85m).Percent);
        Assert.Equal(Percentage.FromPercent(40m), Percentage.Create(40m));
    }

    [Fact]
    public void Arithmetic_Addition_InRange()
    {
        var a = Percentage.FromPercent(30m);
        var b = Percentage.FromPercent(20m);
        Assert.Equal(0.5m, (a + b).Value);
    }

    [Fact]
    public void Arithmetic_Subtraction_InRange()
    {
        var a = Percentage.FromPercent(80m);
        var b = Percentage.FromPercent(30m);
        Assert.Equal(0.5m, (a - b).Value);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Percentage.FromPercent(50m);
        Assert.Equal(0.25m, (a * 0.5m).Value);
        Assert.Equal(0.25m, (0.5m * a).Value);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Percentage.FromPercent(50m);
        Assert.Equal(1m, (a / 0.5m).Value);
    }

    [Fact]
    public void Arithmetic_Addition_OverRange_Throws()
    {
        var a = Percentage.FromPercent(60m);
        var b = Percentage.FromPercent(60m);
        Assert.Throws<OverflowException>(() => a + b);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Percentage.FromPercent(25m);
        var b = Percentage.FromPercent(75m);
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
        var a = Percentage.Parse("50%");
        var b = Percentage.Parse("0.5");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void FindCandidatesInText_FindsPercentValues()
    {
        var text = "Rabatt 15% och sedan 7,5 % extra.";
        var candidates = Percentage.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var p = Percentage.Parse("85%");
        Assert.Equal("***%", p.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForDecimalForm()
    {
        Assert.True(Percentage.IsNormalized("0.85"));
    }

    [Fact]
    public void IsNormalized_FalseForPercentForm()
    {
        Assert.False(Percentage.IsNormalized("85%"));
    }

    [Theory]
    [InlineData("5,5%")]
    [InlineData("2.5%")]
    [InlineData("99,9%")]
    [InlineData("0,1 %")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Percentage.IsValid(input));
    }

    [Theory]
    [InlineData("5,5%", 0.055)]
    [InlineData("2.5%", 0.025)]
    [InlineData("99,9%", 0.999)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedValue)
    {
        Assert.True(Percentage.TryParse(input, out var result));
        Assert.Equal((decimal)expectedValue, result!.Value);
    }

    [Theory]
    [InlineData("5,5%", "5.5%")]
    [InlineData("  85  %  ", "85%")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Percentage.Format(input));
    }

    [Theory]
    [InlineData("5,5%", "0.055")]
    [InlineData("99.9%", "0.999")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Percentage.Normalize(input));
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Percentage.Parse("25%");
        var b = Percentage.Parse("75%");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Percentage.Parse("25%");
        Assert.Equal(1, a.CompareTo(null));
    }
}
