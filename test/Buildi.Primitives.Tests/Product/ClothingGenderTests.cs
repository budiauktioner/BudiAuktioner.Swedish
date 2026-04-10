using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class ClothingGenderTests
{
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(5, ClothingGender.All.Count);
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Unisex")]
    [InlineData("Boys")]
    [InlineData("Girls")]
    [InlineData("male")]
    [InlineData("female")]
    [InlineData("MALE")]
    [InlineData("Herr")]
    [InlineData("Dam")]
    [InlineData("herr")]
    [InlineData("dam")]
    [InlineData("man")]
    [InlineData("men")]
    [InlineData("men's")]
    [InlineData("mens")]
    [InlineData("woman")]
    [InlineData("women")]
    [InlineData("women's")]
    [InlineData("womens")]
    [InlineData("herrar")]
    [InlineData("damer")]
    [InlineData("kvinna")]
    [InlineData("kvinnor")]
    [InlineData("herrkläder")]
    [InlineData("damkläder")]
    [InlineData("gentleman")]
    [InlineData("gentlemen")]
    [InlineData("lady")]
    [InlineData("ladies")]
    [InlineData("uni")]
    [InlineData("both")]
    [InlineData("boy")]
    [InlineData("pojke")]
    [InlineData("pojkar")]
    [InlineData("pojk")]
    [InlineData("kille")]
    [InlineData("killar")]
    [InlineData("girl")]
    [InlineData("flicka")]
    [InlineData("flickor")]
    [InlineData("tjej")]
    [InlineData("tjejer")]
    [InlineData("  Male  ")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ClothingGender.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("neutral")]
    [InlineData("other")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ClothingGender.IsValid(input));
    }

    [Theory]
    [InlineData("Male", "Male", "Male", "Herr")]
    [InlineData("male", "Male", "Male", "Herr")]
    [InlineData("MALE", "Male", "Male", "Herr")]
    [InlineData("Herr", "Male", "Male", "Herr")]
    [InlineData("herr", "Male", "Male", "Herr")]
    [InlineData("man", "Male", "Male", "Herr")]
    [InlineData("men", "Male", "Male", "Herr")]
    [InlineData("men's", "Male", "Male", "Herr")]
    [InlineData("mens", "Male", "Male", "Herr")]
    [InlineData("herrar", "Male", "Male", "Herr")]
    [InlineData("herrkläder", "Male", "Male", "Herr")]
    [InlineData("gentleman", "Male", "Male", "Herr")]
    [InlineData("gentlemen", "Male", "Male", "Herr")]
    [InlineData("Female", "Female", "Female", "Dam")]
    [InlineData("female", "Female", "Female", "Dam")]
    [InlineData("Dam", "Female", "Female", "Dam")]
    [InlineData("dam", "Female", "Female", "Dam")]
    [InlineData("woman", "Female", "Female", "Dam")]
    [InlineData("women", "Female", "Female", "Dam")]
    [InlineData("women's", "Female", "Female", "Dam")]
    [InlineData("womens", "Female", "Female", "Dam")]
    [InlineData("damer", "Female", "Female", "Dam")]
    [InlineData("kvinna", "Female", "Female", "Dam")]
    [InlineData("kvinnor", "Female", "Female", "Dam")]
    [InlineData("damkläder", "Female", "Female", "Dam")]
    [InlineData("lady", "Female", "Female", "Dam")]
    [InlineData("ladies", "Female", "Female", "Dam")]
    [InlineData("Unisex", "Unisex", "Unisex", "Unisex")]
    [InlineData("unisex", "Unisex", "Unisex", "Unisex")]
    [InlineData("UNISEX", "Unisex", "Unisex", "Unisex")]
    [InlineData("uni", "Unisex", "Unisex", "Unisex")]
    [InlineData("both", "Unisex", "Unisex", "Unisex")]
    [InlineData("Boys", "Boys", "Boys", "Pojke")]
    [InlineData("boy", "Boys", "Boys", "Pojke")]
    [InlineData("pojke", "Boys", "Boys", "Pojke")]
    [InlineData("pojkar", "Boys", "Boys", "Pojke")]
    [InlineData("pojk", "Boys", "Boys", "Pojke")]
    [InlineData("kille", "Boys", "Boys", "Pojke")]
    [InlineData("killar", "Boys", "Boys", "Pojke")]
    [InlineData("Girls", "Girls", "Girls", "Flicka")]
    [InlineData("girl", "Girls", "Girls", "Flicka")]
    [InlineData("flicka", "Girls", "Girls", "Flicka")]
    [InlineData("flickor", "Girls", "Girls", "Flicka")]
    [InlineData("tjej", "Girls", "Girls", "Flicka")]
    [InlineData("tjejer", "Girls", "Girls", "Flicka")]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedValue, string expectedEnglish, string expectedSwedish)
    {
        var ok = ClothingGender.TryParse(input, out var result);
        Assert.True(ok);
        Assert.NotNull(result);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(expectedEnglish, result.EnglishName);
        Assert.Equal(expectedSwedish, result.LocalizedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = ClothingGender.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("neutral")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => ClothingGender.Parse(input));
    }

    [Theory]
    [InlineData("Male", "Male")]
    [InlineData("male", "Male")]
    [InlineData("Herr", "Male")]
    [InlineData("dam", "Female")]
    [InlineData("unisex", "Unisex")]
    [InlineData("pojke", "Boys")]
    [InlineData("flicka", "Girls")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, ClothingGender.Normalize(input));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", ClothingGender.Normalize(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Normalize_WithFallback_ReturnsNull_ForEmpty()
    {
        Assert.Null(ClothingGender.Normalize("", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(ClothingGender.Normalize(" ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", ClothingGender.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void Format_ReturnsNull_ForInvalidInput()
    {
        Assert.Null(ClothingGender.Format(null));
        Assert.Null(ClothingGender.Format(""));
        Assert.Null(ClothingGender.Format("nope"));
    }

    [Theory]
    [InlineData("Male", true)]
    [InlineData("Female", true)]
    [InlineData("Unisex", true)]
    [InlineData("Boys", true)]
    [InlineData("Girls", true)]
    [InlineData("male", false)]
    [InlineData("Herr", false)]
    [InlineData("dam", false)]
    [InlineData("nope", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, ClothingGender.IsNormalized(input));
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        var male = ClothingGender.Parse("Male");
        var display = male.ToString();
        Assert.True(display == "Male" || display == "Herr");
    }

    [Fact]
    public void ToNormalizedString_ReturnsValue()
    {
        var female = ClothingGender.Parse("dam");
        Assert.Equal("Female", female.ToNormalizedString());
    }

    [Fact]
    public void Equality_SameGender()
    {
        var a = ClothingGender.Parse("Male");
        var b = ClothingGender.Parse("Herr");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentGenders()
    {
        var a = ClothingGender.Parse("Male");
        var b = ClothingGender.Parse("Female");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equality_Null()
    {
        var a = ClothingGender.Parse("Male");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersBySortOrder()
    {
        Assert.True(ClothingGender.Male < ClothingGender.Female);
        Assert.True(ClothingGender.Female < ClothingGender.Unisex);
        Assert.True(ClothingGender.Unisex < ClothingGender.Boys);
        Assert.True(ClothingGender.Boys < ClothingGender.Girls);
        Assert.True(ClothingGender.Girls > ClothingGender.Male);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        Assert.Equal(1, ClothingGender.Male.CompareTo(null));
    }

    [Fact]
    public void Parse_ReturnsSameInstance()
    {
        var a = ClothingGender.Parse("Male");
        Assert.Same(ClothingGender.Male, a);

        var b = ClothingGender.Parse("dam");
        Assert.Same(ClothingGender.Female, b);

        var c = ClothingGender.Parse("pojke");
        Assert.Same(ClothingGender.Boys, c);
    }

    [Theory]
    [InlineData("  Male  ")]
    [InlineData("  herr  ")]
    [InlineData("  Dam  ")]
    public void TryParse_TrimsWhitespace(string input)
    {
        Assert.True(ClothingGender.TryParse(input, out var result));
        Assert.NotNull(result);
    }
}
