using System.Globalization;
using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class AdultClothingSizeTests
{
    [Theory]
    [InlineData("M", ClothingSizeSystem.Letter, 38, 8, 10, "M")]
    [InlineData("m", ClothingSizeSystem.Letter, 38, 8, 10, "M")]
    [InlineData("XL", ClothingSizeSystem.Letter, 42, 12, 14, "XL")]
    [InlineData("xxs", ClothingSizeSystem.Letter, 32, 2, 4, "XXS")]
    [InlineData("XXXXL", ClothingSizeSystem.Letter, 48, 18, 20, "XXXXL")]
    [InlineData("EU 40", ClothingSizeSystem.EU, 40, 10, 12, "L")]
    [InlineData("eu 40", ClothingSizeSystem.EU, 40, 10, 12, "L")]
    [InlineData("EU40", ClothingSizeSystem.EU, 40, 10, 12, "L")]
    [InlineData("US 10", ClothingSizeSystem.US, 40, 10, 12, "L")]
    [InlineData("us 10", ClothingSizeSystem.US, 40, 10, 12, "L")]
    [InlineData("UK 12", ClothingSizeSystem.UK, 40, 10, 12, "L")]
    [InlineData("40", ClothingSizeSystem.EU, 40, 10, 12, "L")]
    [InlineData(" 40 ", ClothingSizeSystem.EU, 40, 10, 12, "L")]
    [InlineData("32", ClothingSizeSystem.EU, 32, 2, 4, "XXS")]
    public void TryParse_ReturnsExpectedProperties(
        string input,
        ClothingSizeSystem expectedSystem,
        int expectedEu,
        int expectedUs,
        int expectedUk,
        string expectedValue)
    {
        var ok = AdultClothingSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectedSystem, size!.System);
        Assert.Equal(expectedEu, size.EuSize);
        Assert.Equal(expectedUs, size.UsSize);
        Assert.Equal(expectedUk, size.UkSize);
        Assert.Equal(expectedValue, size.Value);
        Assert.Equal(expectedValue, size.ToString());
        Assert.Equal($"EU {expectedEu}", size.ToNormalizedString());
    }

    [Theory]
    [InlineData("50", 50, 20, 22)]
    [InlineData("56", 56, 26, 28)]
    public void TryParse_ReturnsNullLetter_ForEuWithoutLetterMapping(
        string input,
        int expectedEu,
        int expectedUs,
        int expectedUk)
    {
        var ok = AdultClothingSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Null(size!.LetterSize);
        Assert.Equal(expectedEu, size.EuSize);
        Assert.Equal(expectedUs, size.UsSize);
        Assert.Equal(expectedUk, size.UkSize);
        Assert.Equal($"EU {expectedEu}", size.Value);
    }

    [Theory]
    [InlineData("M", "M")]
    [InlineData("EU 40", "L")]
    [InlineData("US 10", "L")]
    [InlineData("UK 12", "L")]
    [InlineData("40", "L")]
    [InlineData("50", "EU 50")]
    public void Format_ReturnsLetterOrEu(string input, string? expected)
    {
        Assert.Equal(expected, AdultClothingSize.Format(input));
    }

    [Theory]
    [InlineData("M", "EU 38")]
    [InlineData("EU 40", "EU 40")]
    [InlineData("50", "EU 50")]
    public void Normalize_ReturnsCanonicalEu(string input, string? expected)
    {
        Assert.Equal(expected, AdultClothingSize.Normalize(input));
    }

    [Theory]
    [InlineData("EU 40")]
    [InlineData("EU 50")]
    public void IsNormalized_ReturnsTrue_ForCanonicalEu(string input)
    {
        Assert.True(AdultClothingSize.IsNormalized(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("M")]
    [InlineData("eu 40")]
    [InlineData("EU40")]
    public void IsNormalized_ReturnsFalse_WhenNotExactCanonical(string? input)
    {
        Assert.False(AdultClothingSize.IsNormalized(input));
    }

    [Theory]
    [InlineData("M")]
    [InlineData("EU 40")]
    [InlineData("US 10")]
    [InlineData("32")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(AdultClothingSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("XX")]
    [InlineData("EU 41")]
    [InlineData("US 11")]
    [InlineData("UK 13")]
    [InlineData("41")]
    [InlineData("30")]
    [InlineData("31")]
    [InlineData("21")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("10")]
    [InlineData("20")]
    [InlineData("57")]
    [InlineData("not-a-size")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(AdultClothingSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("XX")]
    [InlineData("EU 41")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = AdultClothingSize.TryParse(input, out var size);

        Assert.False(ok);
        Assert.Null(size);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => AdultClothingSize.Parse("EU 41"));
    }

    [Fact]
    public void Format_FallbackToInputWhenInvalid_ReturnsTrimmedInput()
    {
        Assert.Equal("nope", AdultClothingSize.Format("  nope  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData(38, 8, 10)]
    [InlineData(40, 10, 12)]
    [InlineData(42, 12, 14)]
    [InlineData(50, 20, 22)]
    public void UsAndUkSizes_FollowApproximateFormulas(int eu, int expectedUs, int expectedUk)
    {
        var size = AdultClothingSize.Parse(eu.ToString(CultureInfo.InvariantCulture));

        Assert.Equal(eu, size.EuSize);
        Assert.Equal(expectedUs, size.UsSize);
        Assert.Equal(expectedUk, size.UkSize);
    }

    [Fact]
    public void Equality_SameSize()
    {
        var a = AdultClothingSize.Parse("M");
        var b = AdultClothingSize.Parse("EU 38");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentSizes()
    {
        var a = AdultClothingSize.Parse("M");
        var b = AdultClothingSize.Parse("L");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Comparison_SmallerToLarger()
    {
        var s = AdultClothingSize.Parse("S");
        var l = AdultClothingSize.Parse("L");
        Assert.True(s < l);
        Assert.True(l > s);
        Assert.True(s.CompareTo(l) < 0);
    }
}
