using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ChildClothingSizeTests
{
    [Theory]
    [InlineData("128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData(" 128 ", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("EU 128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("eu 128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("EU128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("US 8", ClothingSizeSystem.US, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("us 8", ClothingSizeSystem.US, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("UK 8-9", ClothingSizeSystem.UK, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("uk 8-9", ClothingSizeSystem.UK, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("child 128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("CHILD 128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("barn 128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("Barn  128", ClothingSizeSystem.EU, 128, "128", "8", "8-9", "8-9 years")]
    [InlineData("68", ClothingSizeSystem.EU, 68, "68", "3-6m", "3-6m", "3-6 months")]
    [InlineData("US 3-6m", ClothingSizeSystem.US, 68, "68", "3-6m", "3-6m", "3-6 months")]
    [InlineData("US newborn", ClothingSizeSystem.US, 56, "56", "newborn", "newborn", "Newborn")]
    [InlineData("UK 12-18m", ClothingSizeSystem.UK, 80, "80", "12m", "12-18m", "9-12 months")]
    public void TryParse_ReturnsExpectedProperties(
        string input,
        ClothingSizeSystem expectedSystem,
        int expectedHeightCm,
        string expectedEuSize,
        string expectedUs,
        string expectedUk,
        string expectedAge)
    {
        var ok = ChildClothingSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectedSystem, size.System);
        Assert.Equal(expectedHeightCm, size.HeightCm);
        Assert.Equal(expectedEuSize, size.EuSize);
        Assert.Equal(expectedUs, size.UsSize);
        Assert.Equal(expectedUk, size.UkSize);
        Assert.Equal(expectedAge, size.AgeRange);
        Assert.Equal($"EU {expectedHeightCm}", size.ToString());
        Assert.Equal($"EU {expectedHeightCm}", size.ToNormalizedString());
    }

    [Theory]
    [InlineData("120/130", 122)]
    [InlineData("110/116", 110)]
    [InlineData("86/92", 86)]
    [InlineData("98/104", 98)]
    [InlineData("134/140", 134)]
    [InlineData("170/176", 170)]
    [InlineData("120-130", 122)]
    [InlineData("110-116", 110)]
    [InlineData("120 / 130", 122)]
    [InlineData(" 120/130 ", 122)]
    [InlineData("EU 120/130", 122)]
    [InlineData("EU 110/116", 110)]
    [InlineData("barn 120/130", 122)]
    [InlineData("child 120-130", 122)]
    [InlineData("100/110", 104)]
    [InlineData("130/140", 134)]
    public void TryParse_FromHeightRange_MapsToClosestHeight(string input, int expectedHeightCm)
    {
        var ok = ChildClothingSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(ClothingSizeSystem.EU, size.System);
        Assert.Equal(expectedHeightCm, size.HeightCm);
    }

    [Theory]
    [InlineData("8 years", 128)]
    [InlineData("8 year", 128)]
    [InlineData("8 YEARS", 128)]
    [InlineData("8 år", 128)]
    [InlineData("8År", 128)]
    [InlineData("8,5 years", 128)]
    public void TryParse_FromAgeString_MapsToClosestHeight(string input, int expectedHeightCm)
    {
        var ok = ChildClothingSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(ClothingSizeSystem.Unknown, size.System);
        Assert.Equal(expectedHeightCm, size.HeightCm);
    }

    [Theory]
    [InlineData("128", "EU 128")]
    [InlineData("US 8", "EU 128")]
    [InlineData("UK 8-9", "EU 128")]
    [InlineData("8 years", "EU 128")]
    [InlineData("120/130", "EU 122")]
    [InlineData("110-116", "EU 110")]
    public void Format_ReturnsEuDisplay(string input, string? expected)
    {
        Assert.Equal(expected, ChildClothingSize.Format(input));
    }

    [Theory]
    [InlineData("128", "EU 128")]
    [InlineData("US 8", "EU 128")]
    public void Normalize_ReturnsCanonicalEu(string input, string? expected)
    {
        Assert.Equal(expected, ChildClothingSize.Normalize(input));
    }

    [Theory]
    [InlineData("EU 128")]
    public void IsNormalized_ReturnsTrue_ForCanonicalEu(string input)
    {
        Assert.True(ChildClothingSize.IsNormalized(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("128")]
    [InlineData("eu 128")]
    [InlineData("EU128")]
    public void IsNormalized_ReturnsFalse_WhenNotExactCanonical(string? input)
    {
        Assert.False(ChildClothingSize.IsNormalized(input));
    }

    [Theory]
    [InlineData("128")]
    [InlineData("EU 104")]
    [InlineData("US 4")]
    [InlineData("barn 92")]
    [InlineData("120/130")]
    [InlineData("110-116")]
    [InlineData("EU 86/92")]
    [InlineData("barn 120/130")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ChildClothingSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("120")]
    [InlineData("EU 120")]
    [InlineData("US 99")]
    [InlineData("UK 8-10")]
    [InlineData("not-a-size")]
    [InlineData("M")]
    [InlineData("40")]
    [InlineData("8")]
    [InlineData("10")]
    [InlineData("130/120")]
    [InlineData("120/120")]
    [InlineData("10/20")]
    [InlineData("200/210")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ChildClothingSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EU 200")]
    [InlineData("US XL")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = ChildClothingSize.TryParse(input, out var size);

        Assert.False(ok);
        Assert.Null(size);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => ChildClothingSize.Parse("EU 120"));
    }

    [Fact]
    public void Format_FallbackToInputWhenInvalid_ReturnsTrimmedInput()
    {
        Assert.Equal("nope", ChildClothingSize.Format("  nope  ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Us16_MapsToHeight164()
    {
        var ok = ChildClothingSize.TryParse("US 16", out var size);
        Assert.True(ok);
        Assert.Equal(164, size!.HeightCm);
        Assert.Equal("16", size.UsSize);
    }

    [Fact]
    public void Us16_18_MapsToHeight170()
    {
        var ok = ChildClothingSize.TryParse("US 16-18", out var size);
        Assert.True(ok);
        Assert.Equal(170, size!.HeightCm);
    }

    [Fact]
    public void Equality_SameSize()
    {
        var a = ChildClothingSize.Parse("128");
        var b = ChildClothingSize.Parse("EU 128");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Comparison_SmallerToLarger()
    {
        var s = ChildClothingSize.Parse("104");
        var l = ChildClothingSize.Parse("140");
        Assert.True(s < l);
        Assert.True(l > s);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ChildClothingSize.Parse("110");
        var b = ChildClothingSize.Parse("140");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ChildClothingSize.Parse("128");
        Assert.Equal(1, a.CompareTo(null));
    }
}
