using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class Gtin8Tests
{
    [Theory]
    [InlineData("96385074")]
    [InlineData("00000000")]
    [InlineData("12345670")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Gtin8.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("96385075")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("5901234123457")]
    [InlineData("ABCDEFGH")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Gtin8.IsValid(input));
    }

    [Theory]
    [InlineData("96385074", "96385074", 4)]
    [InlineData("12345670", "12345670", 0)]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedDigits, int expectedCheckDigit)
    {
        var ok = Gtin8.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.NotNull(gtin);
        Assert.Equal(expectedDigits, gtin!.Digits);
        Assert.Equal(expectedCheckDigit, gtin.CheckDigit);
    }

    [Theory]
    [InlineData(" 9638 5074 ", "96385074")]
    [InlineData("9638-5074", "96385074")]
    public void TryParse_StripsWhitespaceAndDashes(string input, string expectedDigits)
    {
        var ok = Gtin8.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.Equal(expectedDigits, gtin!.Digits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("96385075")]
    [InlineData("5901234123457")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = Gtin8.TryParse(input, out var gtin);

        Assert.False(ok);
        Assert.Null(gtin);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => Gtin8.Parse("96385075"));
    }

    [Fact]
    public void ToGtin14Digits_ZeroPadsTo14()
    {
        var gtin = Gtin8.Parse("96385074");

        Assert.Equal("00000096385074", gtin.ToGtin14Digits());
    }

    [Fact]
    public void ToString_ReturnsDigits()
    {
        var gtin = Gtin8.Parse("96385074");

        Assert.Equal("96385074", gtin.ToString());
        Assert.Equal("96385074", gtin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Gtin8.Parse("96385074");
        var b = Gtin8.Parse("96385074");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Gtin8.Parse("96385074");
        var b = Gtin8.Parse("12345670");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Gtin8.Parse("96385074");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Gtin8.Parse("12345670");
        var b = Gtin8.Parse("96385074");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Gtin8.Parse("96385074");
        Assert.Equal(1, a.CompareTo(null));
    }
}
