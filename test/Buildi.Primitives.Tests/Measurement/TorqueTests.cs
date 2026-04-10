using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Tests.Measurement;

public class TorqueTests
{
    [Theory]
    [InlineData("250 Nm")]
    [InlineData("100 ft-lb")]
    [InlineData("10 kgf-m")]
    [InlineData("50 in-lb")]
    [InlineData("200 N·m")]
    [InlineData("75 foot-pound")]
    [InlineData("12 kgfm")]
    [InlineData("25 newtonmeter")]
    [InlineData("500 mNm")]
    [InlineData("2 kNm")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Torque.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("250")]
    [InlineData("250 xyz")]
    [InlineData("Nm 250")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Torque.IsValid(input));
    }

    [Theory]
    [InlineData("1 Nm", 1)]
    [InlineData("1 kgf-m", 9.80665)]
    public void TryParse_ReturnsExpected_NewtonMeters(string input, decimal expectedNm)
    {
        Assert.True(Torque.TryParse(input, out var result));
        Assert.Equal(expectedNm, result!.NewtonMeters);
    }

    [Fact]
    public void TryParse_ConvertsFootPoundsAndInchPounds_ToNewtonMeters()
    {
        Assert.True(Torque.TryParse("2 ft-lb", out var ftLb));
        Assert.Equal(2 * TorqueUnit.FootPound.ToBaseUnitFactor, ftLb!.NewtonMeters);

        Assert.True(Torque.TryParse("12 in-lb", out var inLb));
        Assert.Equal(12 * TorqueUnit.InchPound.ToBaseUnitFactor, inLb!.NewtonMeters);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        Assert.False(Torque.TryParse(input, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("250 xyz")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Torque.Parse(input));
    }

    [Theory]
    [InlineData("100 ft-lb", "100 ft-lb")]
    [InlineData("250 Nm", "250 Nm")]
    [InlineData("10.5 kgf-m", "10.5 kgf-m")]
    [InlineData("500 mNm", "500 mNm")]
    [InlineData("2 kNm", "2 kNm")]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Torque.Format(input));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("51 Nm", Torque.Format("50.567 Nm", decimals: 0));
        Assert.Equal("50.6 Nm", Torque.Format("50.567 Nm", decimals: 1));
        Assert.Equal("50.567 Nm", Torque.Format("50.567 Nm"));
    }

    [Fact]
    public void ToString_WithDecimals_RoundsValue()
    {
        var t = Torque.Parse("50.567 Nm");
        Assert.Equal("51 Nm", t.ToString(TorqueUnit.NewtonMeter, decimals: 0));
        Assert.Equal("50.6 Nm", t.ToString(TorqueUnit.NewtonMeter, decimals: 1));
    }

    [Theory]
    [InlineData("250 Nm", "250 Nm")]
    [InlineData("1 kgf-m", "9.80665 Nm")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Torque.Normalize(input));
    }

    [Fact]
    public void Normalize_ConvertsFootPounds_ToNewtonMetersString()
    {
        var expected = Torque.Parse("100 ft-lb").ToNormalizedString();
        Assert.Equal(expected, Torque.Normalize("100 ft-lb"));
    }

    [Theory]
    [InlineData("250 Nm", "250 Nm")]
    [InlineData("100 ft-lb", "100 ft-lb")]
    public void ToString_ReturnsFormattedValue(string input, string expected)
    {
        var torque = Torque.Parse(input);
        Assert.Equal(expected, torque.ToString());
    }

    [Fact]
    public void ToString_WithUnit_ReturnsValueInSpecifiedUnit()
    {
        var torque = Torque.FromNewtonMeters(1.3558179483314004m);
        Assert.Equal("1 ft-lb", torque.ToString(TorqueUnit.FootPound));
    }

    [Fact]
    public void ConversionProperties_AreCorrect()
    {
        var torque = Torque.FromNewtonMeters(9.80665m);
        Assert.Equal(9.80665m, torque.NewtonMeters);
        Assert.Equal(1m, torque.KilogramForceMeters);
    }

    [Fact]
    public void In_ReturnsValueInSpecifiedUnit()
    {
        var torque = Torque.FromFootPounds(1);
        Assert.Equal(1m, torque.In(TorqueUnit.FootPound));
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = Torque.FromNewtonMeters(100);
        var b = Torque.FromNewtonMeters(50);
        Assert.Equal(150m, (a + b).NewtonMeters);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = Torque.FromNewtonMeters(100);
        var b = Torque.FromNewtonMeters(25);
        Assert.Equal(75m, (a - b).NewtonMeters);
    }

    [Fact]
    public void Arithmetic_Multiplication()
    {
        var a = Torque.FromNewtonMeters(100);
        Assert.Equal(300m, (a * 3).NewtonMeters);
        Assert.Equal(300m, (3 * a).NewtonMeters);
    }

    [Fact]
    public void Arithmetic_Division()
    {
        var a = Torque.FromNewtonMeters(100);
        Assert.Equal(25m, (a / 4).NewtonMeters);
    }

    [Fact]
    public void Comparison_Operators()
    {
        var a = Torque.FromNewtonMeters(100);
        var b = Torque.FromNewtonMeters(200);
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
        var a = Torque.FromNewtonMeters(100);
        var b = Torque.FromFootPounds(100m / TorqueUnit.FootPound.ToBaseUnitFactor);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void OriginalUnit_PreservedFromParsing()
    {
        var torque = Torque.Parse("100 ft-lb");
        Assert.Same(TorqueUnit.FootPound, torque.OriginalUnit);
    }

    [Fact]
    public void FindCandidatesInText_FindsTorqueValues()
    {
        var text = "Bolt to 50 Nm; max 100 ft-lb.";
        var candidates = Torque.FindCandidatesInText(text);
        Assert.True(candidates.Count >= 2);
    }

    [Fact]
    public void ToMaskedString_MasksValue()
    {
        var torque = Torque.Parse("250 Nm");
        Assert.Equal("*** Nm", torque.ToMaskedString());
    }

    [Fact]
    public void IsNormalized_TrueForBaseUnit()
    {
        Assert.True(Torque.IsNormalized("250 Nm"));
    }

    [Fact]
    public void IsNormalized_FalseForNonBaseUnit()
    {
        Assert.False(Torque.IsNormalized("100 ft-lb"));
    }

    [Theory]
    [InlineData("Nm", "Nm")]
    [InlineData("ft-lb", "ft-lb")]
    [InlineData("foot-pound", "ft-lb")]
    [InlineData("newtonmeter", "Nm")]
    [InlineData("fotpund", "ft-lb")]
    [InlineData("kgfm", "kgf-m")]
    [InlineData("newton meters", "Nm")]
    [InlineData("newton metre", "Nm")]
    [InlineData("newton metres", "Nm")]
    [InlineData("newton-meters", "Nm")]
    [InlineData("newton-metre", "Nm")]
    [InlineData("newton-metres", "Nm")]
    [InlineData("foot-pounds", "ft-lb")]
    [InlineData("ft lb", "ft-lb")]
    [InlineData("ft-lbs", "ft-lb")]
    [InlineData("ft lbs", "ft-lb")]
    [InlineData("ftlb", "ft-lb")]
    [InlineData("ftlbs", "ft-lb")]
    [InlineData("inch-pounds", "in-lb")]
    [InlineData("in lb", "in-lb")]
    [InlineData("in-lbs", "in-lb")]
    [InlineData("in lbs", "in-lb")]
    [InlineData("mNm", "mNm")]
    [InlineData("mN·m", "mNm")]
    [InlineData("millinewton meter", "mNm")]
    [InlineData("millinewton meters", "mNm")]
    [InlineData("millinewton-meter", "mNm")]
    [InlineData("millinewton-meters", "mNm")]
    [InlineData("kNm", "kNm")]
    [InlineData("kN·m", "kNm")]
    [InlineData("kilonewton meter", "kNm")]
    [InlineData("kilonewton meters", "kNm")]
    [InlineData("kilonewton-meter", "kNm")]
    [InlineData("kilonewton-meters", "kNm")]
    public void TorqueUnit_TryParse_ResolvesSymbols(string input, string expectedSymbol)
    {
        Assert.True(TorqueUnit.TryParse(input, out var unit));
        Assert.Equal(expectedSymbol, unit!.Symbol);
    }

    [Theory]
    [InlineData("5,5 Nm")]
    [InlineData("2.5 Nm")]
    [InlineData("0,5 ft-lb")]
    [InlineData("3.14 Nm")]
    [InlineData("1 000 Nm")]
    public void IsValid_ReturnsTrue_ForDecimalInputs(string input)
    {
        Assert.True(Torque.IsValid(input));
    }

    [Theory]
    [InlineData("5,5 Nm", 5.5)]
    [InlineData("2.5 Nm", 2.5)]
    [InlineData("1 000 Nm", 1000)]
    public void TryParse_ReturnsExpected_ForDecimalInputs(string input, double expectedNm)
    {
        Assert.True(Torque.TryParse(input, out var result));
        Assert.Equal((decimal)expectedNm, result!.NewtonMeters);
    }

    [Theory]
    [InlineData("5,5 Nm", "5.5 Nm")]
    [InlineData("  10  Nm  ", "10 Nm")]
    public void Format_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Torque.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        Assert.Equal("1000 Nm", Torque.Format("1 kNm", unit: TorqueUnit.NewtonMeter));
        Assert.Equal("1 kNm", Torque.Format("1000 Nm", unit: TorqueUnit.KilonewtonMeter));
        Assert.Equal("1 kNm", Torque.Format("1 kNm"));
    }

    [Theory]
    [InlineData("5,5 Nm", "5.5 Nm")]
    [InlineData("1 000 Nm", "1000 Nm")]
    public void Normalize_ReturnsExpected_ForDecimalInputs(string input, string expected)
    {
        Assert.Equal(expected, Torque.Normalize(input));
    }

    [Fact]
    public void Arithmetic_WithDecimals()
    {
        var a = Torque.FromNewtonMeters(1.5m);
        var b = Torque.FromNewtonMeters(0.5m);
        Assert.Equal(2m, (a + b).NewtonMeters);
    }

    [Fact]
    public void Conversions_MillinewtonMeterToNewtonMeter()
    {
        var torque = Torque.Parse("1000 mNm");
        Assert.Equal(1m, torque.NewtonMeters);
    }

    [Fact]
    public void Conversions_KilonewtonMeterToNewtonMeter()
    {
        var torque = Torque.Parse("1 kNm");
        Assert.Equal(1000m, torque.NewtonMeters);
    }

    [Fact]
    public void FromFactory_MillinewtonMeters()
    {
        var torque = Torque.FromMillinewtonMeters(500);
        Assert.Equal(500m, torque.MillinewtonMeters);
        Assert.Equal(0.5m, torque.NewtonMeters);
    }

    [Fact]
    public void FromFactory_KilonewtonMeters()
    {
        var torque = Torque.FromKilonewtonMeters(2);
        Assert.Equal(2m, torque.KilonewtonMeters);
        Assert.Equal(2000m, torque.NewtonMeters);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenConversionOverflows()
    {
        Assert.False(Torque.TryParse("99999999999999999999999999 kNm", out var result));
        Assert.Null(result);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Torque.Parse("100 Nm");
        var b = Torque.Parse("200 Nm");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Torque.Parse("100 Nm");
        Assert.Equal(1, a.CompareTo(null));
    }
}
