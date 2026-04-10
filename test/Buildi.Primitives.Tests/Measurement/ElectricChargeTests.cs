using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class ElectricChargeTests
{
    [Theory]
    [InlineData("5000 mAh")]
    [InlineData("2.5 Ah")]
    [InlineData("3600 C")]
    [InlineData("1 Ah")]
    [InlineData("100 mAh")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ElectricCharge.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("100")]
    [InlineData("100 xyz")]
    [InlineData("Ah 1")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ElectricCharge.IsValid(input));
    }

    [Theory]
    [InlineData("1000 mAh", 1)]
    [InlineData("3600 C", 1)]
    [InlineData("5 Ah", 5)]
    [InlineData("1 Ah", 1)]
    public void TryParse_ReturnsExpected_AmpereHours(string input, double expectedAh)
    {
        Assert.True(ElectricCharge.TryParse(input, out var result));
        Assert.Equal((decimal)expectedAh, result!.AmpereHours);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(ElectricCharge.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => ElectricCharge.Parse(input));
    }

    [Theory]
    [InlineData("5000 mAh", "5000 mAh")]
    [InlineData("2.5 Ah", "2.5 Ah")]
    [InlineData("3600 C", "3600 C")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ElectricCharge.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("5 Ah", ElectricCharge.Format("5000 mAh", unit: ElectricChargeUnit.AmpereHour));
        Assert.Equal("5000 mAh", ElectricCharge.Format("5 Ah", unit: ElectricChargeUnit.MilliampereHour));
        Assert.Equal("5 Ah", ElectricCharge.Format("5 Ah"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("5001 mAh", ElectricCharge.Format("5000.567 mAh", decimals: 0));
        Assert.Equal("5000.6 mAh", ElectricCharge.Format("5000.567 mAh", decimals: 1));
        Assert.Equal("5000.567 mAh", ElectricCharge.Format("5000.567 mAh"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var c = ElectricCharge.Parse("5000.567 mAh");
        Assert.Equal("5001 mAh", c.ToString(ElectricChargeUnit.MilliampereHour, decimals: 0));
        Assert.Equal("5 Ah", c.ToString(ElectricChargeUnit.AmpereHour, decimals: 0));
    }

    [Theory]
    [InlineData("1000 mAh", "1 Ah")]
    [InlineData("3600 C", "1 Ah")]
    [InlineData("2 Ah", "2 Ah")]
    [InlineData("5000 mAh", "5 Ah")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ElectricCharge.Normalize(input));
    }

    [Theory]
    [InlineData("5000 mAh", "5000 mAh")]
    [InlineData("2 Ah", "2 Ah")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var charge = ElectricCharge.Parse(input);
        Assert.Equal(expected, charge.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var charge = ElectricCharge.FromAmpereHours(1);
        Assert.Equal("3600 C", charge.ToString(ElectricChargeUnit.Coulomb));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var charge = ElectricCharge.FromAmpereHours(5);
        Assert.Equal(5m, charge.AmpereHours);
        Assert.Equal(5000m, charge.MilliampereHours);
        Assert.Equal(18000m, charge.Coulombs);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var charge = ElectricCharge.FromAmpereHours(1);
        Assert.Equal(1000m, charge.In(ElectricChargeUnit.MilliampereHour));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = ElectricCharge.FromAmpereHours(1);
        var b = ElectricCharge.FromAmpereHours(2);
        Assert.Equal(3m, (a + b).AmpereHours);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = ElectricCharge.FromAmpereHours(5);
        var b = ElectricCharge.FromAmpereHours(2);
        Assert.Equal(3m, (a - b).AmpereHours);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = ElectricCharge.FromAmpereHours(2);
        Assert.Equal(6m, (a * 3).AmpereHours);
        Assert.Equal(6m, (3 * a).AmpereHours);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = ElectricCharge.FromAmpereHours(6);
        Assert.Equal(2m, (a / 3).AmpereHours);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = ElectricCharge.FromAmpereHours(1);
        var b = ElectricCharge.FromAmpereHours(2);
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
        var a = ElectricCharge.FromAmpereHours(1);
        var b = ElectricCharge.FromCoulombs(3600);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var charge = ElectricCharge.Parse("5000 mAh");
        Assert.Same(ElectricChargeUnit.MilliampereHour, charge.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsChargeValues()
    {
        var text = "Battery 5000 mAh and backup 2 Ah.";
        var candidates = ElectricCharge.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var charge = ElectricCharge.Parse("5000 mAh");
        Assert.Equal("*** mAh", charge.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(ElectricCharge.IsNormalized("5 Ah"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(ElectricCharge.IsNormalized("5000 mAh"));
    }

    [Theory]
    [InlineData("mAh", "mAh")]
    [InlineData("Ah", "Ah")]
    [InlineData("C", "C")]
    [InlineData("milliamperetimme", "mAh")]
    [InlineData("amperetimme", "Ah")]
    [InlineData("coulomb", "C")]
    [InlineData("milliampere-hour", "mAh")]
    [InlineData("ampere-hour", "Ah")]
    [InlineData("ampere-hours", "Ah")]
    [InlineData("milliampere-hours", "mAh")]
    [InlineData("amperehour", "Ah")]
    [InlineData("amperehours", "Ah")]
    [InlineData("amp-hour", "Ah")]
    [InlineData("amp-hours", "Ah")]
    [InlineData("amp hour", "Ah")]
    [InlineData("amp hours", "Ah")]
    [InlineData("milliamperehour", "mAh")]
    [InlineData("milliamperehours", "mAh")]
    [InlineData("amperetimmar", "Ah")]
    [InlineData("milliamperetimmar", "mAh")]
    [InlineData("coulombs", "C")]
    public void ElectricChargeUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(ElectricChargeUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 Ah")]
    [InlineData("2.5 Ah")]
    [InlineData("0,5 mAh")]
    [InlineData("3.14 Ah")]
    [InlineData("1 000 mAh")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(ElectricCharge.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 Ah", 5.5)]
    [InlineData("2.5 Ah", 2.5)]
    [InlineData("1 000 mAh", 1)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedAh)
    {
        Assert.True(ElectricCharge.TryParse(input, out var result));
        Assert.Equal((decimal)expectedAh, result!.AmpereHours);
    }

    [Theory]
    [InlineData("5,5 Ah", "5.5 Ah")]
    [InlineData("  10  Ah  ", "10 Ah")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, ElectricCharge.Format(input));
    }

    [Theory]
    [InlineData("2500 mAh", "2.5 Ah")]
    [InlineData("5,5 Ah", "5.5 Ah")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, ElectricCharge.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = ElectricCharge.FromAmpereHours(1.5m);
        var b = ElectricCharge.FromMilliampereHours(500);
        Assert.Equal(2m, (a + b).AmpereHours);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ElectricCharge.Parse("1000 mAh");
        var b = ElectricCharge.Parse("5000 mAh");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ElectricCharge.Parse("1000 mAh");
        Assert.Equal(1, a.CompareTo(null));
    }
}
