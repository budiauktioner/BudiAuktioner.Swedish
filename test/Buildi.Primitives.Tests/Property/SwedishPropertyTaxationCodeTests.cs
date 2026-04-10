using Buildi.Primitives.Property;

namespace Buildi.Primitives.Tests.Property;

public class SwedishPropertyTaxationCodeTests
{
    [Theory]
    [InlineData("220")]
    [InlineData("320")]
    [InlineData("110")]
    [InlineData("420")]
    [InlineData("630")]
    [InlineData("890")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(SwedishPropertyTaxationCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("22")]
    [InlineData("2200")]
    [InlineData("099")]
    [InlineData("000")]
    [InlineData("900")]
    [InlineData("abc")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(SwedishPropertyTaxationCode.IsValid(input));
    }

    [Theory]
    [InlineData("220", "220", 220, SwedishPropertyTaxationCategory.Smahusenhet, "Small house unit, developed", "Småhusenhet, bebyggd")]
    [InlineData("320", "320", 320, SwedishPropertyTaxationCategory.Hyreshusenhet, "Rental property unit, developed, residential", "Hyreshusenhet, bebyggd, bostäder")]
    [InlineData("110", "110", 110, SwedishPropertyTaxationCategory.Lantbruksenhet, "Agricultural unit, undeveloped", "Lantbruksenhet, obebyggd")]
    [InlineData("630", "630", 630, SwedishPropertyTaxationCategory.Elproduktionsenhet, "Power production unit, wind power", "Elproduktionsenhet, vindkraftverk")]
    public void TryParse_ReturnsExpectedProperties_ForKnownCode(
        string input, string expectedCode, int expectedNumeric,
        SwedishPropertyTaxationCategory expectedCategory,
        string expectedEnglish, string expectedSwedish)
    {
        var ok = SwedishPropertyTaxationCode.TryParse(input, out var code);

        Assert.True(ok);
        Assert.NotNull(code);
        Assert.Equal(expectedCode, code!.Code);
        Assert.Equal(expectedNumeric, code.NumericCode);
        Assert.Equal(expectedCategory, code.Category);
        Assert.Equal(expectedEnglish, code.EnglishDescription);
        Assert.Equal(expectedSwedish, code.LocalizedDescription);
        Assert.True(code.IsKnown);
    }

    [Fact]
    public void TryParse_ReturnsCorrectCategory_ForUnknownCode()
    {
        var ok = SwedishPropertyTaxationCode.TryParse("199", out var code);

        Assert.True(ok);
        Assert.NotNull(code);
        Assert.Equal("199", code!.Code);
        Assert.Equal(199, code.NumericCode);
        Assert.Equal(SwedishPropertyTaxationCategory.Lantbruksenhet, code.Category);
        Assert.False(code.IsKnown);
        Assert.Null(code.EnglishDescription);
        Assert.Null(code.LocalizedDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("22")]
    [InlineData("099")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = SwedishPropertyTaxationCode.TryParse(input, out var code);

        Assert.False(ok);
        Assert.Null(code);
    }

    [Theory]
    [InlineData("22")]
    [InlineData("099")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => SwedishPropertyTaxationCode.Parse(input));
    }

    [Theory]
    [InlineData("  220  ", "220")]
    [InlineData(" 3 2 0 ", "320")]
    public void TryParse_NormalizesInput(string input, string expectedCode)
    {
        var ok = SwedishPropertyTaxationCode.TryParse(input, out var code);

        Assert.True(ok);
        Assert.Equal(expectedCode, code!.Code);
    }

    [Fact]
    public void Format_ReturnsDescription_ForKnownCode()
    {
        Assert.Equal("Småhusenhet, bebyggd", SwedishPropertyTaxationCode.Format("220"));
    }

    [Fact]
    public void Format_ReturnsCode_ForUnknownValidCode()
    {
        Assert.Equal("199", SwedishPropertyTaxationCode.Format("199"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("22", null)]
    public void Format_ReturnsNull_ForInvalidInput(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishPropertyTaxationCode.Format(input));
    }

    [Theory]
    [InlineData("220", "220")]
    [InlineData(" 220 ", "220")]
    [InlineData(null, null)]
    [InlineData("22", null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, SwedishPropertyTaxationCode.Normalize(input));
    }

    [Theory]
    [InlineData("220", true)]
    [InlineData(" 220 ", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, SwedishPropertyTaxationCode.IsNormalized(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_ForInvalid()
    {
        Assert.Equal("22", SwedishPropertyTaxationCode.Format("  22  ", fallbackToTrimmedInputWhenInvalid: true));
        Assert.Null(SwedishPropertyTaxationCode.Format(null, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Fact]
    public void ToString_ReturnsDescription_ForKnown()
    {
        var code = SwedishPropertyTaxationCode.Parse("220");
        Assert.Equal("Småhusenhet, bebyggd", code.ToString());
    }

    [Fact]
    public void ToString_ReturnsCode_ForUnknown()
    {
        var code = SwedishPropertyTaxationCode.Parse("199");
        Assert.Equal("199", code.ToString());
    }

    [Fact]
    public void ToNormalizedString_ReturnsCode()
    {
        var code = SwedishPropertyTaxationCode.Parse("220");
        Assert.Equal("220", code.ToNormalizedString());
    }

    [Theory]
    [InlineData(100, SwedishPropertyTaxationCategory.Lantbruksenhet)]
    [InlineData(200, SwedishPropertyTaxationCategory.Smahusenhet)]
    [InlineData(300, SwedishPropertyTaxationCategory.Hyreshusenhet)]
    [InlineData(400, SwedishPropertyTaxationCategory.Industrienhet)]
    [InlineData(500, SwedishPropertyTaxationCategory.Taktmark)]
    [InlineData(600, SwedishPropertyTaxationCategory.Elproduktionsenhet)]
    [InlineData(700, SwedishPropertyTaxationCategory.Specialenhet)]
    [InlineData(800, SwedishPropertyTaxationCategory.OvrigMark)]
    public void Category_MapsCorrectly(int numericCode, SwedishPropertyTaxationCategory expectedCategory)
    {
        var code = SwedishPropertyTaxationCode.Parse(numericCode.ToString());
        Assert.Equal(expectedCategory, code.Category);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = SwedishPropertyTaxationCode.Parse("220");
        var b = SwedishPropertyTaxationCode.Parse("220");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = SwedishPropertyTaxationCode.Parse("220");
        var b = SwedishPropertyTaxationCode.Parse("320");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = SwedishPropertyTaxationCode.Parse("220");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = SwedishPropertyTaxationCode.Parse("220");
        var b = SwedishPropertyTaxationCode.Parse("320");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = SwedishPropertyTaxationCode.Parse("220");
        Assert.Equal(1, a.CompareTo(null));
    }
}
