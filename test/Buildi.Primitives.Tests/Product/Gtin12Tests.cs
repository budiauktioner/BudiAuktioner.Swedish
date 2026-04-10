using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class Gtin12Tests
{
    [Theory]
    [InlineData("614141000036")]
    [InlineData("012345678905")]
    [InlineData("000000000000")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Gtin12.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("614141000037")]
    [InlineData("96385074")]
    [InlineData("5901234123457")]
    [InlineData("10614141000415")]
    [InlineData("ABCDEFGHIJKL")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Gtin12.IsValid(input));
    }

    [Theory]
    [InlineData("614141000036", "614141000036", 6)]
    [InlineData("012345678905", "012345678905", 5)]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedDigits, int expectedCheckDigit)
    {
        var ok = Gtin12.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.NotNull(gtin);
        Assert.Equal(expectedDigits, gtin!.Digits);
        Assert.Equal(expectedCheckDigit, gtin.CheckDigit);
    }

    [Theory]
    [InlineData(" 614141 000036 ", "614141000036")]
    [InlineData("614-141-000036", "614141000036")]
    public void TryParse_StripsWhitespaceAndDashes(string input, string expectedDigits)
    {
        var ok = Gtin12.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.Equal(expectedDigits, gtin!.Digits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("614141000037")]
    [InlineData("96385074")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = Gtin12.TryParse(input, out var gtin);

        Assert.False(ok);
        Assert.Null(gtin);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => Gtin12.Parse("614141000037"));
    }

    [Fact]
    public void ToGtin14Digits_ZeroPadsTo14()
    {
        var gtin = Gtin12.Parse("614141000036");

        Assert.Equal("00614141000036", gtin.ToGtin14Digits());
    }

    [Fact]
    public void ToString_ReturnsDigits()
    {
        var gtin = Gtin12.Parse("614141000036");

        Assert.Equal("614141000036", gtin.ToString());
        Assert.Equal("614141000036", gtin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Gtin12.Parse("614141000036");
        var b = Gtin12.Parse("614141000036");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Gtin12.Parse("614141000036");
        var b = Gtin12.Parse("012345678905");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Gtin12.Parse("614141000036");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Gtin12.Parse("012345678905");
        var b = Gtin12.Parse("614141000036");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Gtin12.Parse("614141000036");
        Assert.Equal(1, a.CompareTo(null));
    }
}
