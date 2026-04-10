using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class GtinTests
{
    [Theory]
    [InlineData("96385074")]
    [InlineData("00000000")]
    [InlineData("012345678905")]
    [InlineData("5901234123457")]
    [InlineData("4006381333931")]
    [InlineData("7350053850019")]
    [InlineData("10614141000415")]
    [InlineData("05901234123457")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Gtin.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("1234567890")]
    [InlineData("12345678901")]
    [InlineData("123456789012345")]
    [InlineData("96385075")]
    [InlineData("5901234123458")]
    [InlineData("ABCDEFGH")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Gtin.IsValid(input));
    }

    [Theory]
    [InlineData("96385074", 8)]
    [InlineData("012345678905", 12)]
    [InlineData("5901234123457", 13)]
    [InlineData("10614141000415", 14)]
    public void TryParse_ReturnsExpectedLength(string input, int expectedLength)
    {
        var ok = Gtin.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.NotNull(gtin);
        Assert.Equal(expectedLength, gtin!.Length);
        Assert.Equal(input, gtin.Digits);
    }

    [Theory]
    [InlineData("96385074", 4)]
    [InlineData("5901234123457", 7)]
    [InlineData("10614141000415", 5)]
    public void TryParse_ReturnsExpectedCheckDigit(string input, int expectedCheckDigit)
    {
        Gtin.TryParse(input, out var gtin);

        Assert.Equal(expectedCheckDigit, gtin!.CheckDigit);
    }

    [Theory]
    [InlineData(" 590 1234 1234 57 ", "5901234123457")]
    [InlineData("5901-2341-23457", "5901234123457")]
    public void TryParse_StripsWhitespaceAndDashes(string input, string expectedDigits)
    {
        var ok = Gtin.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.Equal(expectedDigits, gtin!.Digits);
    }

    [Theory]
    [InlineData("96385074", "00000096385074")]
    [InlineData("012345678905", "00012345678905")]
    [InlineData("5901234123457", "05901234123457")]
    [InlineData("10614141000415", "10614141000415")]
    public void ToGtin14Digits_ZeroPadsCorrectly(string input, string expected14)
    {
        var gtin = Gtin.Parse(input);

        Assert.Equal(expected14, gtin.ToGtin14Digits());
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => Gtin.Parse("invalid"));
    }

    [Theory]
    [InlineData("5901234123457", "5901234123457")]
    [InlineData("invalid", null)]
    [InlineData("  invalid  ", null)]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Gtin.Format(input, fallbackToTrimmedInputWhenInvalid: expected != null && !Gtin.IsValid(input)));
    }

    [Theory]
    [InlineData("invalid", "invalid")]
    [InlineData("  invalid  ", "invalid")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string? input, string? expected)
    {
        Assert.Equal(expected, Gtin.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("5901234123457", "5901234123457")]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, Gtin.Normalize(input));
    }

    [Fact]
    public void ToString_ReturnsDigits()
    {
        var gtin = Gtin.Parse("5901234123457");

        Assert.Equal("5901234123457", gtin.ToString());
        Assert.Equal("5901234123457", gtin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Gtin.Parse("5901234123457");
        var b = Gtin.Parse("5901234123457");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Gtin.Parse("5901234123457");
        var b = Gtin.Parse("96385074");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Gtin.Parse("5901234123457");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Gtin.Parse("012345678905");
        var b = Gtin.Parse("5901234123457");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Gtin.Parse("5901234123457");
        Assert.Equal(1, a.CompareTo(null));
    }
}
