using Buildi.Primitives.Product;

namespace Buildi.Primitives.Tests.Product;

public class HsCodeTests
{
    [Theory]
    [InlineData("01")]
    [InlineData("84")]
    [InlineData("97")]
    [InlineData("8471")]
    [InlineData("84.71")]
    [InlineData("847130")]
    [InlineData("8471.30")]
    [InlineData("84713000")]
    [InlineData("8471.30.00")]
    [InlineData("8471300000")]
    [InlineData("8471.30.00.00")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(HsCode.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("00")]
    [InlineData("0000")]
    [InlineData("98")]
    [InlineData("9900")]
    [InlineData("ABCD")]
    [InlineData("AB.CD")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(HsCode.IsValid(input));
    }

    [Theory]
    [InlineData("84", "84", "84", HsCodeLevel.Chapter)]
    [InlineData("01", "01", "01", HsCodeLevel.Chapter)]
    [InlineData("97", "97", "97", HsCodeLevel.Chapter)]
    [InlineData("8471", "8471", "84", HsCodeLevel.Heading)]
    [InlineData("847130", "847130", "84", HsCodeLevel.Subheading)]
    [InlineData("84713000", "84713000", "84", HsCodeLevel.CnSubheading)]
    [InlineData("8471300000", "8471300000", "84", HsCodeLevel.TaricCode)]
    public void TryParse_ReturnsExpectedProperties(string input, string expectedDigits, string expectedChapter, HsCodeLevel expectedLevel)
    {
        var ok = HsCode.TryParse(input, out var code);

        Assert.True(ok);
        Assert.NotNull(code);
        Assert.Equal(expectedDigits, code!.Digits);
        Assert.Equal(expectedChapter, code.Chapter);
        Assert.Equal(expectedLevel, code.Level);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("00")]
    [InlineData("123")]
    [InlineData("ABCD")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = HsCode.TryParse(input, out var code);

        Assert.False(ok);
        Assert.Null(code);
    }

    [Theory]
    [InlineData("84.71", "8471")]
    [InlineData("8471.30", "847130")]
    [InlineData("8471.30.00", "84713000")]
    [InlineData("8471.30.00.00", "8471300000")]
    [InlineData("  84.71  ", "8471")]
    [InlineData("84-71-30", "847130")]
    [InlineData("84 71 30", "847130")]
    public void TryParse_StripsDotsSpacesDashesAndWhitespace(string input, string expectedDigits)
    {
        var ok = HsCode.TryParse(input, out var code);

        Assert.True(ok);
        Assert.Equal(expectedDigits, code!.Digits);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => HsCode.Parse("invalid"));
    }

    [Theory]
    [InlineData("84", "84")]
    [InlineData("8471", "84.71")]
    [InlineData("84.71", "84.71")]
    [InlineData("847130", "8471.30")]
    [InlineData("8471.30", "8471.30")]
    [InlineData("84713000", "8471.30.00")]
    [InlineData("8471.30.00", "8471.30.00")]
    [InlineData("8471300000", "8471.30.00.00")]
    [InlineData("8471.30.00.00", "8471.30.00.00")]
    [InlineData("  847130  ", "8471.30")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, HsCode.Format(input));
    }

    [Theory]
    [InlineData("invalid", "invalid")]
    [InlineData("  invalid  ", "invalid")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string input, string expected)
    {
        Assert.Equal(expected, HsCode.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("invalid", null)]
    [InlineData("84", "84")]
    [InlineData("8471", "84.71")]
    [InlineData("84.71", "84.71")]
    [InlineData("847130", "8471.30")]
    [InlineData("8471.30", "8471.30")]
    [InlineData("84713000", "8471.30.00")]
    [InlineData("8471.30.00", "8471.30.00")]
    [InlineData("8471300000", "8471.30.00.00")]
    [InlineData("8471.30.00.00", "8471.30.00.00")]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, HsCode.Normalize(input));
    }

    [Theory]
    [InlineData("8471.30", true)]
    [InlineData("847130", false)]
    [InlineData("8471.30.00", true)]
    [InlineData("84713000", false)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, HsCode.IsNormalized(input));
    }

    [Theory]
    [InlineData("84", "84", "84")]
    [InlineData("8471", "84.71", "84.71")]
    [InlineData("847130", "8471.30", "8471.30")]
    [InlineData("84713000", "8471.30.00", "8471.30.00")]
    [InlineData("8471300000", "8471.30.00.00", "8471.30.00.00")]
    public void ToString_And_ToNormalizedString_ReturnExpected(string input, string expectedToString, string expectedNormalized)
    {
        var code = HsCode.Parse(input);

        Assert.Equal(expectedToString, code.ToString());
        Assert.Equal(expectedNormalized, code.ToNormalizedString());
    }

    [Fact]
    public void Chapter77_IsStructurallyValid()
    {
        Assert.True(HsCode.IsValid("77"));
        var code = HsCode.Parse("77");
        Assert.Equal(HsCodeLevel.Chapter, code.Level);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = HsCode.Parse("8471");
        var b = HsCode.Parse("8471");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = HsCode.Parse("8471");
        var b = HsCode.Parse("847130");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_Null_IsNotEqual()
    {
        var a = HsCode.Parse("8471");
        Assert.False(a.Equals(null));
        Assert.False(a == null);
        Assert.True(a != null);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = HsCode.Parse("01.01");
        var b = HsCode.Parse("85.01");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = HsCode.Parse("8471");
        Assert.Equal(1, a.CompareTo(null));
    }
}
