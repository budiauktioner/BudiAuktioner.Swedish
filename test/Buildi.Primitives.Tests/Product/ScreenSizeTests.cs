using Buildi.Primitives.Measurement;
using Buildi.Primitives.Product;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Tests.Product;

public class ScreenSizeTests
{
    [Theory]
    [InlineData("15", 15)]
    [InlineData(" 15 ", 15)]
    [InlineData("15 in", 15)]
    [InlineData("15-inch", 15)]
    [InlineData("15\"", 15)]
    [InlineData("38.1 cm", 15)]
    [InlineData("15 inches", 15)]
    public void TryParse_ReturnsExpectedInches(string input, decimal expectedInches)
    {
        var ok = ScreenSize.TryParse(input, out var size);

        Assert.True(ok);
        Assert.NotNull(size);
        Assert.Equal(expectedInches, size!.Inches, precision: 5);
        Assert.Equal($"{expectedInches} in", size.Value);
    }

    [Fact]
    public void TryParse_38_1cm_MatchesCentimeters()
    {
        var ok = ScreenSize.TryParse("38.1 cm", out var size);

        Assert.True(ok);
        Assert.Equal(38.1m, size!.Centimeters, precision: 5);
        Assert.NotNull(size.Diagonal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("-5 in")]
    public void TryParse_ReturnsNull_ForInvalid(string? input)
    {
        var ok = ScreenSize.TryParse(input, out var size);

        Assert.False(ok);
        Assert.Null(size);
    }

    [Theory]
    [InlineData("invalid")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => ScreenSize.Parse(input));
    }

    [Theory]
    [InlineData("15")]
    [InlineData("15 in")]
    [InlineData("38.1 cm")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(ScreenSize.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("x")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(ScreenSize.IsValid(input));
    }

    [Theory]
    [InlineData("15", "15 in")]
    [InlineData("15 in", "15 in")]
    [InlineData("38.1 cm", "15 in")]
    public void Format_ReturnsInchesDisplay(string input, string expected)
    {
        Assert.Equal(expected, ScreenSize.Format(input));
    }

    [Fact]
    public void Format_WithUnit_ConvertsToSpecifiedUnit()
    {
        var result = ScreenSize.Format("15", unit: LengthUnit.Centimeter);
        Assert.Equal("38.1 cm", result);
        Assert.Equal("15 in", ScreenSize.Format("15"));
    }

    [Fact]
    public void Format_WithDecimals_RoundsValue()
    {
        Assert.Equal("16 in", ScreenSize.Format("15.567", decimals: 0));
        Assert.Equal("15.6 in", ScreenSize.Format("15.567", decimals: 1));
        Assert.Equal("15.567 in", ScreenSize.Format("15.567"));
    }

    [Theory]
    [InlineData("15", "15 in")]
    [InlineData("15\"", "15 in")]
    public void Normalize_MatchesFormat_ForValid(string input, string expected)
    {
        Assert.Equal(expected, ScreenSize.Normalize(input));
    }

    [Theory]
    [InlineData("15 in")]
    public void IsNormalized_ReturnsTrue_ForCanonical(string input)
    {
        Assert.True(ScreenSize.IsNormalized(input));
    }

    [Theory]
    [InlineData("15")]
    [InlineData("15\"")]
    [InlineData(" 15 in ")]
    [InlineData(null)]
    public void IsNormalized_ReturnsFalse_WhenNotCanonicalOrInvalid(string? input)
    {
        Assert.False(ScreenSize.IsNormalized(input!));
    }

    [Fact]
    public void ToString_And_ToNormalizedString_MatchValue()
    {
        var size = ScreenSize.Parse("15 in");

        Assert.Equal("15 in", size.ToString());
        Assert.Equal("15 in", size.ToNormalizedString());
        Assert.Equal(size.Value, size.ToString());
    }

    [Fact]
    public void Equality_SameSize()
    {
        var a = ScreenSize.Parse("15");
        var b = ScreenSize.Parse("15 in");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Comparison_SmallerToLarger()
    {
        var s = ScreenSize.Parse("13");
        var l = ScreenSize.Parse("27");
        Assert.True(s < l);
        Assert.True(l > s);
    }

    [Theory]
    [InlineData("15.6\"")]
    [InlineData("15.6\u2033")]
    [InlineData("15.6''")]
    public void TryParse_ReturnsTrue_ForInchSymbolInputs(string input)
    {
        Assert.True(ScreenSize.TryParse(input, out var size));
        Assert.Equal(15.6m, size!.Inches);
    }

    [Fact]
    public void FindCandidatesInText_FindsInchSymbol()
    {
        var candidates = ScreenSize.FindCandidatesInText("Tech specs: 15.6\" screen.");
        Assert.Single(candidates);
        Assert.Equal("15.6 in", candidates[0].NormalizedForm);
        Assert.Equal("15.6\"", candidates[0].OriginalText);
    }

    [Fact]
    public void FindCandidatesInText_FindsHyphenatedInch()
    {
        var candidates = ScreenSize.FindCandidatesInText("A 15-inch display.");
        Assert.Single(candidates);
        Assert.Equal("15 in", candidates[0].NormalizedForm);
    }

    [Fact]
    public void FindCandidatesInText_FindsDoublePrime()
    {
        var candidates = ScreenSize.FindCandidatesInText("The 27\u2033 monitor.");
        Assert.Single(candidates);
        Assert.Equal("27 in", candidates[0].NormalizedForm);
    }

    [Fact]
    public void FindCandidatesInText_FindsMultiple()
    {
        var candidates = ScreenSize.FindCandidatesInText("Choose 13.3\" or 15.6\" models.");
        Assert.Equal(2, candidates.Count);
    }

    [Fact]
    public void FindCandidatesInText_EmptyOrNull_ReturnsEmpty()
    {
        Assert.Empty(ScreenSize.FindCandidatesInText(""));
        Assert.Empty(ScreenSize.FindCandidatesInText(null!));
    }

    [Fact]
    public void FindCandidatesInText_HasCorrectCategory()
    {
        var candidates = ScreenSize.FindCandidatesInText("15.6\" display");
        Assert.Single(candidates);
        Assert.Equal(TextCandidateCategory.Product, candidates[0].Category);
    }

    [Fact]
    public void ToMaskedString_ReturnsExpected()
    {
        var size = ScreenSize.Parse("15.6");
        Assert.Equal("*** in", size.ToMaskedString());
    }

    [Fact]
    public void Create_FromDecimalAndUnit_Works()
    {
        var ss = ScreenSize.Create(15.6m, LengthUnit.Inch);
        Assert.Equal(15.6m, ss.Inches);
    }

    [Fact]
    public void Create_FromIntAndUnit_Works()
    {
        var ss = ScreenSize.Create(15, LengthUnit.Inch);
        Assert.Equal(15m, ss.Inches);
    }

    [Fact]
    public void Create_ZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ScreenSize.Create(0m, LengthUnit.Inch));
        Assert.Throws<ArgumentOutOfRangeException>(() => ScreenSize.Create(-1m, LengthUnit.Inch));
    }

    [Fact]
    public void FromInches_Works()
    {
        var ss = ScreenSize.FromInches(15.6m);
        Assert.Equal(15.6m, ss.Inches);
    }

    [Fact]
    public void FromCentimeters_Works()
    {
        var ss = ScreenSize.FromCentimeters(39);
        Assert.True(ss.Inches > 0);
        Assert.True(ss.Centimeters > 0);
    }

    [Fact]
    public void Create_EqualsStringParsed()
    {
        var fromFactory = ScreenSize.FromInches(15);
        var fromString = ScreenSize.Parse("15");
        Assert.Equal(fromFactory, fromString);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = ScreenSize.Parse("13\"");
        var b = ScreenSize.Parse("15\"");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = ScreenSize.Parse("15 in");
        Assert.Equal(1, a.CompareTo(null));
    }
}
