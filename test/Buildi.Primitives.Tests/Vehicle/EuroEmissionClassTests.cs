using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class EuroEmissionClassTests
{
    [Fact]
    public void All_ContainsExpectedCount()
    {
        Assert.Equal(15, EuroEmissionClass.All.Count);
    }

    [Theory]
    [InlineData("Euro 1", 1, null, "Miljöklass 1", 1992)]
    [InlineData("Euro 6d-temp", 6, "d-temp", "Miljöklass 2005", 2017)]
    [InlineData("El", 0, null, "Miljöklass El", 0)]
    public void StaticInstances_HaveExpectedProperties(
        string euroClass, int level, string? subLevel, string miljo, int year)
    {
        var e = EuroEmissionClass.All.Single(x => x.EuroClass == euroClass);
        Assert.Equal(level, e.Level);
        Assert.Equal(subLevel, e.SubLevel);
        Assert.Equal(miljo, e.SwedishMiljoklass);
        Assert.Equal(year, e.IntroductionYear);
        Assert.Equal(euroClass, e.Value);
    }

    [Theory]
    [InlineData("Euro 6")]
    [InlineData("euro 6")]
    [InlineData("EURO6")]
    [InlineData("eu 6")]
    [InlineData("euro6")]
    [InlineData("Euro 6d")]
    [InlineData("euro 6d")]
    [InlineData("EURO6D")]
    [InlineData("euro6d")]
    [InlineData("Euro 6d-temp")]
    [InlineData("euro 6d-temp")]
    [InlineData("euro 6 d-temp")]
    [InlineData("MK2005")]
    [InlineData("miljöklass 2005")]
    [InlineData("Miljöklass 2005")]
    [InlineData("El")]
    [InlineData("el")]
    [InlineData("Elbil")]
    [InlineData("  Euro 5b  ")]
    [InlineData("Euro 7")]
    [InlineData("V")]
    [InlineData("VI")]
    [InlineData("Euro V")]
    [InlineData("Euro VI")]
    [InlineData("III")]
    [InlineData("2005PM")]
    [InlineData("2008")]
    [InlineData("EEV")]
    public void IsValid_ReturnsTrue_ForRecognizedInputs(string input)
    {
        Assert.True(EuroEmissionClass.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Euro 0")]
    [InlineData("Euro 65")]
    [InlineData("Bensin")]
    [InlineData("HYBRID")]
    [InlineData("Miljöklass Hybrid")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(EuroEmissionClass.IsValid(input));
    }

    [Theory]
    [InlineData("euro 6d", "Euro 6d")]
    [InlineData("EURO6", "Euro 6")]
    [InlineData("miljöklass 2005", "Euro 6")]
    [InlineData("MK2005", "Euro 6")]
    [InlineData("elbil", "El")]
    public void TryParse_ReturnsExpectedEuroClass(string input, string expectedEuroClass)
    {
        var ok = EuroEmissionClass.TryParse(input, out var e);
        Assert.True(ok);
        Assert.NotNull(e);
        Assert.Equal(expectedEuroClass, e.EuroClass);
        Assert.Same(EuroEmissionClass.All.First(x => x.EuroClass == expectedEuroClass), e);
    }

    [Theory]
    [InlineData("euro 6d", "Euro 6d")]
    [InlineData("El", "El")]
    [InlineData(null, null)]
    [InlineData("nope", null)]
    public void Format_ReturnsEuroClassOrNull(string? input, string? expected)
    {
        Assert.Equal(expected, EuroEmissionClass.Format(input));
    }

    [Fact]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid()
    {
        Assert.Equal("x", EuroEmissionClass.Format(" x ", fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("euro 6b", "Euro 6b")]
    [InlineData("Euro 6e", "Euro 6e")]
    [InlineData("bogus", null)]
    public void Normalize_ReturnsEuroClassOrNull(string? input, string? expected)
    {
        Assert.Equal(expected, EuroEmissionClass.Normalize(input));
    }

    [Theory]
    [InlineData("Euro 6d", true)]
    [InlineData("euro 6d", false)]
    [InlineData("Euro 7", true)]
    public void IsNormalized_RequiresCanonicalCasingAndSpacing(string? input, bool expected)
    {
        Assert.Equal(expected, EuroEmissionClass.IsNormalized(input));
    }

    [Theory]
    [InlineData("Euro 6d", "Euro 6d")]
    public void ToString_And_ToNormalizedString_MatchEuroClass(string input, string expected)
    {
        var e = EuroEmissionClass.Parse(input);
        Assert.Equal(expected, e.ToString());
        Assert.Equal(expected, e.ToNormalizedString());
    }

    [Theory]
    [InlineData("nope")]
    [InlineData("")]
    public void Parse_Throws_ForInvalidInputs(string input)
    {
        Assert.Throws<ArgumentException>(() => EuroEmissionClass.Parse(input));
    }

    [Fact]
    public void FindCandidatesInText_FindsEuroLabels()
    {
        const string text = "Bilen är Euro 6d-temp och systerbil Euro 5.";
        var c = EuroEmissionClass.FindCandidatesInText(text);

        Assert.Equal(2, c.Count);
        Assert.All(c, x => Assert.Equal(TextCandidateCategory.Vehicle, x.Category));
        Assert.All(c, x => Assert.Equal(TextMatchConfidence.Medium, x.Confidence));
        Assert.Contains(c, x => x.Value == EuroEmissionClass.Euro6dTemp);
        Assert.Contains(c, x => x.Value == EuroEmissionClass.Euro5);
    }

    [Fact]
    public void FindCandidatesInText_Empty_ReturnsEmpty()
    {
        Assert.Empty(EuroEmissionClass.FindCandidatesInText(""));
        Assert.Empty(EuroEmissionClass.FindCandidatesInText(null!));
    }

    [Fact]
    public void FindCandidatesInText_PreservesOriginalSpan()
    {
        var text = "klass euro 6d här";
        var c = EuroEmissionClass.FindCandidatesInText(text);
        Assert.Single(c);
        Assert.Equal("euro 6d", c[0].OriginalText);
    }

    [Fact]
    public void Equality_SameClass()
    {
        var a = EuroEmissionClass.Parse("Euro 6");
        var b = EuroEmissionClass.Parse("EURO6");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentClasses()
    {
        var a = EuroEmissionClass.Parse("Euro 5");
        var b = EuroEmissionClass.Parse("Euro 6");
        Assert.True(a != b);
    }

    [Fact]
    public void Comparison_LowerToHigher()
    {
        var e4 = EuroEmissionClass.Parse("Euro 4");
        var e6 = EuroEmissionClass.Parse("Euro 6");
        Assert.True(e4 < e6);
        Assert.True(e6 > e4);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = EuroEmissionClass.Parse("Euro 3");
        var b = EuroEmissionClass.Parse("Euro 6");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = EuroEmissionClass.Parse("Euro 5");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData("V", "Euro 5")]
    [InlineData("VI", "Euro 6")]
    [InlineData("Euro VI", "Euro 6")]
    [InlineData("III", "Euro 3")]
    [InlineData("Euro 7", "Euro 7")]
    [InlineData("2005PM", "Euro 5")]
    [InlineData("2008", "Euro 5b")]
    [InlineData("EEV", "Euro 5")]
    public void TryParse_ReturnsExpectedEuroClass_ForNewSynonyms(string input, string expectedEuroClass)
    {
        var ok = EuroEmissionClass.TryParse(input, out var e);
        Assert.True(ok);
        Assert.NotNull(e);
        Assert.Equal(expectedEuroClass, e.EuroClass);
    }
}
