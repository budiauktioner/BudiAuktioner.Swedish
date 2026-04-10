using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class Gtin14Tests
{
    [Theory]
    [InlineData("10614141000415")]
    [InlineData("05901234123457")]
    [InlineData("00000000000000")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Gtin14.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("10614141000416")]
    [InlineData("5901234123457")]
    [InlineData("96385074")]
    [InlineData("ABCDEFGHIJKLMN")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Gtin14.IsValid(input));
    }

    [Theory]
    [InlineData("10614141000415", "10614141000415", 5, 1, "0614141000415")]
    [InlineData("05901234123457", "05901234123457", 7, 0, "5901234123457")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedDigits, int expectedCheckDigit, int expectedIndicator, string expectedInner)
    {
        var ok = Gtin14.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.NotNull(gtin);
        Assert.Equal(expectedDigits, gtin!.Digits);
        Assert.Equal(expectedCheckDigit, gtin.CheckDigit);
        Assert.Equal(expectedIndicator, gtin.IndicatorDigit);
        Assert.Equal(expectedInner, gtin.InnerGtin13Digits);
    }

    [Theory]
    [InlineData(" 1061 4141 0004 15 ", "10614141000415")]
    [InlineData("1061-4141-0004-15", "10614141000415")]
    public void TryParse_StripsWhitespaceAndDashes(string input, string expectedDigits)
    {
        var ok = Gtin14.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.Equal(expectedDigits, gtin!.Digits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("10614141000416")]
    [InlineData("96385074")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = Gtin14.TryParse(input, out var gtin);

        Assert.False(ok);
        Assert.Null(gtin);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => Gtin14.Parse("10614141000416"));
    }

    [Fact]
    public void ToString_ReturnsDigits()
    {
        var gtin = Gtin14.Parse("10614141000415");

        Assert.Equal("10614141000415", gtin.ToString());
        Assert.Equal("10614141000415", gtin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Gtin14.Parse("10614141000415");
        var b = Gtin14.Parse("10614141000415");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Gtin14.Parse("10614141000415");
        var b = Gtin14.Parse("05901234123457");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Gtin14.Parse("10614141000415");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Gtin14.Parse("05901234123457");
        var b = Gtin14.Parse("10614141000415");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Gtin14.Parse("10614141000415");
        Assert.Equal(1, a.CompareTo(null));
    }
}
