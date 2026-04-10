using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class GoogleProductCategoryTests
{
    [Theory]
    [InlineData("Animals & Pet Supplies")]
    [InlineData("Animals & Pet Supplies > Pet Supplies")]
    [InlineData("Animals & Pet Supplies > Pet Supplies > Bird Supplies")]
    [InlineData("Electronics > Computers > Laptops")]
    [InlineData("Apparel & Accessories > Clothing > Shirts & Tops")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(GoogleProductCategory.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(">")]
    [InlineData(" > ")]
    [InlineData("> Foo")]
    [InlineData("Foo >")]
    [InlineData("Foo > > Bar")]
    [InlineData("Foo > > ")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(GoogleProductCategory.IsValid(input));
    }

    [Theory]
    [InlineData("Electronics", "Electronics", 1, "Electronics", "Electronics")]
    [InlineData("Electronics > Computers", "Electronics > Computers", 2, "Electronics", "Computers")]
    [InlineData("Electronics > Computers > Laptops", "Electronics > Computers > Laptops", 3, "Electronics", "Laptops")]
    [InlineData("Animals & Pet Supplies > Pet Supplies > Bird Supplies > Bird Food",
        "Animals & Pet Supplies > Pet Supplies > Bird Supplies > Bird Food", 4, "Animals & Pet Supplies", "Bird Food")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedPath, int expectedDepth, string expectedRoot, string expectedLeaf)
    {
        var ok = GoogleProductCategory.TryParse(input, out var category);

        Assert.True(ok);
        Assert.NotNull(category);
        Assert.Equal(expectedPath, category!.Path);
        Assert.Equal(expectedDepth, category.Depth);
        Assert.Equal(expectedRoot, category.RootCategory);
        Assert.Equal(expectedLeaf, category.LeafCategory);
    }

    [Theory]
    [InlineData("Electronics>Computers>Laptops", "Electronics > Computers > Laptops")]
    [InlineData("Electronics >Computers> Laptops", "Electronics > Computers > Laptops")]
    [InlineData("  Electronics > Computers  ", "Electronics > Computers")]
    [InlineData("Electronics  >  Computers", "Electronics > Computers")]
    public void TryParse_NormalizesWhitespaceAroundSeparators(string input, string expectedPath)
    {
        var ok = GoogleProductCategory.TryParse(input, out var category);

        Assert.True(ok);
        Assert.Equal(expectedPath, category!.Path);
    }

    [Fact]
    public void TryParse_ReturnsCorrectSegments()
    {
        GoogleProductCategory.TryParse("Animals & Pet Supplies > Pet Supplies > Bird Supplies", out var category);

        Assert.Equal(3, category!.Segments.Count);
        Assert.Equal("Animals & Pet Supplies", category.Segments[0]);
        Assert.Equal("Pet Supplies", category.Segments[1]);
        Assert.Equal("Bird Supplies", category.Segments[2]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(">")]
    [InlineData("Foo > > Bar")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = GoogleProductCategory.TryParse(input, out var category);

        Assert.False(ok);
        Assert.Null(category);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => GoogleProductCategory.Parse(">"));
    }

    [Theory]
    [InlineData("Electronics > Computers", "Electronics > Computers")]
    [InlineData("Electronics>Computers", "Electronics > Computers")]
    [InlineData("  Electronics > Computers  ", "Electronics > Computers")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(">", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, GoogleProductCategory.Format(input));
    }

    [Theory]
    [InlineData(">", ">")]
    [InlineData("  >  ", ">")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string input, string expected)
    {
        Assert.Equal(expected, GoogleProductCategory.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("Electronics > Computers", "Electronics > Computers")]
    [InlineData("Electronics>Computers", "Electronics > Computers")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(">", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, GoogleProductCategory.Normalize(input));
    }

    [Theory]
    [InlineData("Electronics > Computers", true)]
    [InlineData("Electronics>Computers", false)]
    [InlineData("  Electronics > Computers  ", false)]
    [InlineData(null, false)]
    [InlineData(">", false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, GoogleProductCategory.IsNormalized(input));
    }

    [Fact]
    public void ToString_And_ToNormalizedString_ReturnCanonicalPath()
    {
        var category = GoogleProductCategory.Parse("Electronics>Computers>Laptops");

        Assert.Equal("Electronics > Computers > Laptops", category.ToString());
        Assert.Equal("Electronics > Computers > Laptops", category.ToNormalizedString());
    }

    [Fact]
    public void SingleSegment_RootAndLeaf_AreSame()
    {
        var category = GoogleProductCategory.Parse("Electronics");

        Assert.Equal(1, category.Depth);
        Assert.Equal("Electronics", category.RootCategory);
        Assert.Equal("Electronics", category.LeafCategory);
        Assert.Single(category.Segments);
    }

    [Fact]
    public void DeeplyNested_Category_ParsesCorrectly()
    {
        var input = "Animals & Pet Supplies > Pet Supplies > Dog Supplies > Dog Food > Non-prescription Dog Food";
        var category = GoogleProductCategory.Parse(input);

        Assert.Equal(5, category.Depth);
        Assert.Equal("Animals & Pet Supplies", category.RootCategory);
        Assert.Equal("Non-prescription Dog Food", category.LeafCategory);
        Assert.Equal(input, category.Path);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = GoogleProductCategory.Parse("Electronics > Computers > Laptops");
        var b = GoogleProductCategory.Parse("Electronics > Computers > Laptops");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = GoogleProductCategory.Parse("Electronics > Computers > Laptops");
        var b = GoogleProductCategory.Parse("Animals & Pet Supplies");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = GoogleProductCategory.Parse("Electronics > Computers > Laptops");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = GoogleProductCategory.Parse("Animals & Pet Supplies");
        var b = GoogleProductCategory.Parse("Electronics");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = GoogleProductCategory.Parse("Electronics > Computers > Laptops");
        Assert.Equal(1, a.CompareTo(null));
    }
}
