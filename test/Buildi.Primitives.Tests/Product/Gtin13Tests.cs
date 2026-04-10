using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class Gtin13Tests
{
    [Theory]
    [InlineData("5901234123457")]
    [InlineData("4006381333931")]
    [InlineData("7350053850019")]
    [InlineData("0000000000000")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(Gtin13.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("5901234123458")]
    [InlineData("96385074")]
    [InlineData("10614141000415")]
    [InlineData("1234567890123")]
    [InlineData("ABCDEFGHIJKLM")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(Gtin13.IsValid(input));
    }

    [Theory]
    [InlineData("5901234123457", "5901234123457", 7, "590")]
    [InlineData("4006381333931", "4006381333931", 1, "400")]
    [InlineData("7350053850019", "7350053850019", 9, "735")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedDigits, int expectedCheckDigit, string expectedPrefix)
    {
        var ok = Gtin13.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.NotNull(gtin);
        Assert.Equal(expectedDigits, gtin!.Digits);
        Assert.Equal(expectedCheckDigit, gtin.CheckDigit);
        Assert.Equal(expectedPrefix, gtin.Gs1Prefix);
    }

    [Theory]
    [InlineData("7310865001764", "731", "Sweden", "SE")]
    [InlineData("5901234123457", "590", "Poland", "PL")]
    [InlineData("4006381333931", "400", "Germany", "DE")]
    [InlineData("0000000000000", "000", "USA & Canada", null)]
    [InlineData("9789100000004", "978", "Books (ISBN)", null)]
    public void TryParse_ResolvesGs1PrefixMetadata(string input, string expectedPrefix, string expectedName, string? expectedCountryCode)
    {
        var ok = Gtin13.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.Equal(expectedPrefix, gtin!.Gs1Prefix);
        Assert.Equal(expectedName, gtin.Gs1PrefixName);
        Assert.Equal(expectedCountryCode, gtin.Gs1PrefixCountryCode);
    }

    [Fact]
    public void Gs1PrefixName_IsSweden_ForSwedishProducts()
    {
        var gtin = Gtin13.Parse("7350053850019");

        Assert.Equal("735", gtin.Gs1Prefix);
        Assert.Equal("Sweden", gtin.Gs1PrefixName);
        Assert.Equal("SE", gtin.Gs1PrefixCountryCode);
    }

    [Theory]
    [InlineData(" 5901234 123457 ", "5901234123457")]
    [InlineData("590-1234-123457", "5901234123457")]
    public void TryParse_StripsWhitespaceAndDashes(string input, string expectedDigits)
    {
        var ok = Gtin13.TryParse(input, out var gtin);

        Assert.True(ok);
        Assert.Equal(expectedDigits, gtin!.Digits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("5901234123458")]
    [InlineData("96385074")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = Gtin13.TryParse(input, out var gtin);

        Assert.False(ok);
        Assert.Null(gtin);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => Gtin13.Parse("5901234123458"));
    }

    [Fact]
    public void ToGtin14Digits_ZeroPadsTo14()
    {
        var gtin = Gtin13.Parse("5901234123457");

        Assert.Equal("05901234123457", gtin.ToGtin14Digits());
    }

    [Fact]
    public void ToString_ReturnsDigits()
    {
        var gtin = Gtin13.Parse("5901234123457");

        Assert.Equal("5901234123457", gtin.ToString());
        Assert.Equal("5901234123457", gtin.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Gtin13.Parse("5901234123457");
        var b = Gtin13.Parse("5901234123457");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = Gtin13.Parse("5901234123457");
        var b = Gtin13.Parse("4006381333931");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = Gtin13.Parse("5901234123457");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = Gtin13.Parse("4006381333931");
        var b = Gtin13.Parse("5901234123457");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = Gtin13.Parse("5901234123457");
        Assert.Equal(1, a.CompareTo(null));
    }
}
