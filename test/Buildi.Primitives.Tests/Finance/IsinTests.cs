using Buildi.Primitives.Finance;

namespace Buildi.Primitives.Tests.Finance;

public class IsinTests
{
    [Theory]
    [InlineData("SE0000108656")]
    [InlineData("SE0000667891")]
    [InlineData("US0378331005")]
    [InlineData("GB0002374006")]
    [InlineData("DE0007164600")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Isin.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("SE000010865")]
    [InlineData("SE00001086560")]
    [InlineData("SE0000108657")]
    [InlineData("12SE00001086")]
    [InlineData("ABCDEFGHIJKL")]
    [InlineData("1234567890AB")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Isin.IsValid(input));
    }

    [Theory]
    [InlineData("SE0000108656", "SE", "000010865", '6')]
    [InlineData("US0378331005", "US", "037833100", '5')]
    [InlineData("GB0002374006", "GB", "000237400", '6')]
    public void TryParse_ReturnsExpectedProperties_ForValidInput(
        string input, string expectedCountry, string expectedNsin, char expectedCheckDigit)
    {
        var ok = Isin.TryParse(input, out var isin);

        Assert.True(ok);
        Assert.NotNull(isin);
        Assert.Equal(input, isin!.Value);
        Assert.Equal(expectedCountry, isin.CountryCode);
        Assert.Equal(expectedNsin, isin.Nsin);
        Assert.Equal(expectedCheckDigit, isin.CheckDigit);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("INVALID")]
    [InlineData("SE0000108657")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = Isin.TryParse(input, out var isin);

        Assert.False(ok);
        Assert.Null(isin);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("SE0000108657")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => Isin.Parse(input));
    }

    [Theory]
    [InlineData("se0000108656", "SE0000108656")]
    [InlineData("SE 0000 1086 56", "SE0000108656")]
    [InlineData("  SE0000108656  ", "SE0000108656")]
    [InlineData("se-0000-108656", "SE0000108656")]
    public void TryParse_NormalizesInput(string input, string expected)
    {
        var ok = Isin.TryParse(input, out var isin);

        Assert.True(ok);
        Assert.Equal(expected, isin!.Value);
    }

    [Theory]
    [InlineData("SE0000108656", "SE0000108656")]
    [InlineData("se0000108656", "SE0000108656")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("INVALID", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Isin.Format(input));
    }

    [Theory]
    [InlineData("SE0000108656", "SE0000108656")]
    [InlineData("se0000108656", "SE0000108656")]
    [InlineData(null, null)]
    [InlineData("INVALID", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Isin.Normalize(input));
    }

    [Theory]
    [InlineData("SE0000108656", true)]
    [InlineData("se0000108656", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, Isin.IsNormalized(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_ForInvalid()
    {
        Assert.Equal("INVALID", Isin.Format("  INVALID  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(Isin.Format(null, fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(Isin.Format("  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_ForInvalid()
    {
        Assert.Equal("INVALID", Isin.Normalize("  INVALID  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(Isin.Normalize(null, fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(Isin.Normalize("  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var isin = Isin.Parse("SE0000108656");
        Assert.Equal("SE0000108656", isin.ToString());
        Assert.Equal("SE0000108656", isin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Isin.Parse("SE0000108656");
        var b = Isin.Parse("SE0000108656");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Isin.Parse("SE0000108656");
        var b = Isin.Parse("US0378331005");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Isin.Parse("SE0000108656");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Isin.Parse("GB0002374006");
        var b = Isin.Parse("SE0000108656");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Isin.Parse("SE0000108656");
        Assert.Equal(1, a.CompareTo(null));
    }
}
