using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ShoeSizeTests
{
    [Theory]
    [InlineData("EU 42", true, 42)]
    [InlineData("EU 28", false, 28)]
    [InlineData("US 11C", false, 29)]
    [InlineData("US 9", true, 42)]
    public void TryParse_AutoDetectsAdultOrChild(string input, bool expectAdult, decimal expectedEu)
    {
        var ok = ShoeSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectAdult, size.IsAdult);
        Assert.Equal(!expectAdult, size.IsChild);
        if (expectAdult)
            Assert.Equal(expectedEu, size.AsAdult()!.EuSize);
        else
            Assert.Equal(expectedEu, size.AsChild()!.EuSize);
    }

    [Theory]
    [InlineData("EU 37", true)]
    [InlineData("37", true)]
    public void TryParse_EuOverlap_DefaultsToAdult(string input, bool expectAdult)
    {
        var ok = ShoeSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectAdult, size.IsAdult);
        Assert.Equal(37m, size.AsAdult()!.EuSize);
    }

    [Fact]
    public void TryParse_ChildPrefix_EuOverlap_SelectsChild()
    {
        var ok = ShoeSize.TryParse("child EU 37", out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.True(size.IsChild);
        Assert.Equal(37m, size.AsChild()!.EuSize);
        Assert.Null(size.AsAdult());
    }

    [Theory]
    [InlineData("adult EU 42", 42, true)]
    [InlineData("ADULT eu 42.5", 42.5, true)]
    [InlineData("vuxen 42", 42, true)]
    [InlineData("Vuxen US 9", 42, true)]
    [InlineData("child EU 28", 28, false)]
    [InlineData("barn 28", 28, false)]
    [InlineData("BARN EU 37", 37, false)]
    public void TryParse_ExplicitPrefix_ParsesExpected(string input, decimal expectedEu, bool expectAdult)
    {
        var ok = ShoeSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectAdult, size.IsAdult);
        if (expectAdult)
            Assert.Equal(expectedEu, size.AsAdult()!.EuSize);
        else
            Assert.Equal(expectedEu, size.AsChild()!.EuSize);
    }

    [Fact]
    public void AsAdult_ReturnsNull_WhenChild()
    {
        Assert.True(ShoeSize.TryParse("EU 28", out var size));
        Assert.NotNull(size);
        Assert.Null(size.AsAdult());
        Assert.NotNull(size.AsChild());
    }

    [Fact]
    public void AsChild_ReturnsNull_WhenAdult()
    {
        Assert.True(ShoeSize.TryParse("EU 42", out var size));
        Assert.NotNull(size);
        Assert.NotNull(size.AsAdult());
        Assert.Null(size.AsChild());
    }

    [Theory]
    [InlineData("  EU 42  ", "EU 42")]
    [InlineData("us 9", "EU 42")]
    [InlineData("EU 28", "EU 28")]
    public void Format_ReturnsCanonicalEuDisplay(string input, string expected)
    {
        Assert.Equal(expected, ShoeSize.Format(input));
    }

    [Theory]
    [InlineData("EU 42", "EU 42")]
    [InlineData("child EU 37", "EU 37")]
    public void Normalize_ReturnsSameAsFormat_ForValid(string input, string expected)
    {
        Assert.Equal(expected, ShoeSize.Normalize(input));
    }

    [Theory]
    [InlineData("EU 42")]
    [InlineData("EU 28")]
    public void IsNormalized_ReturnsTrue_ForCanonical(string input)
    {
        Assert.True(ShoeSize.IsNormalized(input));
    }

    [Theory]
    [InlineData("eu 42")]
    [InlineData("US 9")]
    public void IsNormalized_ReturnsFalse_WhenNotCanonical(string input)
    {
        Assert.False(ShoeSize.IsNormalized(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not a size")]
    [InlineData("EU 99")]
    [InlineData("adult EU 28")]
    [InlineData("child EU 42")]
    public void IsValid_ReturnsFalse_ForInvalid(string? input)
    {
        Assert.False(ShoeSize.IsValid(input));
    }

    [Theory]
    [InlineData("EU 42")]
    [InlineData("28")]
    public void IsValid_ReturnsTrue_ForValid(string input)
    {
        Assert.True(ShoeSize.IsValid(input));
    }

    [Theory]
    [InlineData("nope")]
    [InlineData("EU 99")]
    public void Parse_Throws_ForInvalid(string input)
    {
        Assert.Throws<ArgumentException>(() => ShoeSize.Parse(input));
    }

    [Fact]
    public void TryParse_ReturnsFalse_ForInvalid()
    {
        Assert.False(ShoeSize.TryParse("EU 99", out var size));
        Assert.Null(size);
    }

    [Theory]
    [InlineData("EU 42", "EU 42")]
    [InlineData("EU 28", "EU 28")]
    public void ToString_ReturnsValue(string input, string expected)
    {
        var s = ShoeSize.Parse(input);
        Assert.Equal(expected, s.ToString());
        Assert.Equal(expected, s.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameAdultSize()
    {
        var a = ShoeSize.Parse("EU 42");
        var b = ShoeSize.Parse("42");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equality_AdultVsChild_NotEqual()
    {
        var adult = ShoeSize.Parse("adult EU 42");
        var child = ShoeSize.Parse("child EU 28");
        Assert.True(adult != child);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ShoeSize.Parse("EU 28");
        var b = ShoeSize.Parse("EU 42");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ShoeSize.Parse("EU 42");
        Assert.Equal(1, a.CompareTo(null));
    }
}
