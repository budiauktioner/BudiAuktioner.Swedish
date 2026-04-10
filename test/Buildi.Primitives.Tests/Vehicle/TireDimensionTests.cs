using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Vehicle;

namespace Buildi.Primitives.Tests.Vehicle;

public class TireDimensionTests
{
    [Theory]
    [InlineData("205/55R16")]
    [InlineData("205/55 R 16")]
    [InlineData("225/45R17")]
    [InlineData("195/65R15 91H")]
    [InlineData("205/55r16")]
    [InlineData("  225/45R17  ")]
    [InlineData("195/65R15  91h")]
    [InlineData("205/55R16 91")]
    [InlineData("385/65R22,5")]
    [InlineData("385/65R22.5")]
    [InlineData("315/70R22,5 154/150L")]
    [InlineData("315/80R22,5 156K")]
    [InlineData("315/70R22,5 154/150")]
    [InlineData("255/60R18 112H")]
    [InlineData("225/65R16C 112/110R")]
    [InlineData("385/55R22,5 160")]
    public void IsValid_ReturnsTrue_ForValidInputs(string input)
    {
        Assert.True(TireDimension.IsValid(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("205/55")]
    [InlineData("205/55R")]
    [InlineData("20/550R16")]
    [InlineData("205/5R16")]
    [InlineData("205/55X16")]
    [InlineData("205/55R09")]
    [InlineData("205/55R27")]
    [InlineData("099/55R16")]
    [InlineData("401/55R16")]
    [InlineData("205/19R16")]
    [InlineData("205/91R16")]
    [InlineData("205/55R16 H91")]
    [InlineData("385/65R99,5")]
    [InlineData("205/55R16 999H")]
    public void IsValid_ReturnsFalse_ForInvalidInputs(string? input)
    {
        Assert.False(TireDimension.IsValid(input));
    }

    [Theory]
    [InlineData("205/55R16", 205, 55, 'R', 16, null, null, "205/55R16", "205/55 R 16")]
    [InlineData("225/45 R 17", 225, 45, 'R', 17, null, null, "225/45R17", "225/45 R 17")]
    [InlineData("195/65R15 91H", 195, 65, 'R', 15, 91, 'H', "195/65R15 91H", "195/65 R 15 91H")]
    [InlineData("195/65d15 91H", 195, 65, 'D', 15, 91, 'H', "195/65D15 91H", "195/65 D 15 91H")]
    public void TryParse_ReturnsExpectedProperties(
        string input,
        int width,
        int aspect,
        char construction,
        int rim,
        int? load,
        char? speed,
        string expectedValue,
        string expectedFormatted)
    {
        var ok = TireDimension.TryParse(input, out var dim);

        Assert.True(ok);
        Assert.NotNull(dim);
        Assert.Equal(width, dim.WidthMm);
        Assert.Equal(aspect, dim.AspectRatio);
        Assert.Equal(construction, dim.Construction);
        Assert.Equal(rim, dim.RimDiameterInches);
        Assert.Equal(load, dim.LoadIndex);
        Assert.Equal(speed, dim.SpeedRating);
        Assert.Equal(expectedValue, dim.Value);
        Assert.Equal(expectedFormatted, dim.ToString());
        Assert.Equal(expectedValue, dim.ToNormalizedString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("205/55")]
    [InlineData("205/55R09")]
    public void TryParse_ReturnsNull_ForInvalidInput(string? input)
    {
        var ok = TireDimension.TryParse(input, out var dim);

        Assert.False(ok);
        Assert.Null(dim);
    }

    [Fact]
    public void Parse_Throws_ForInvalidInput()
    {
        Assert.Throws<ArgumentException>(() => TireDimension.Parse("not-a-tire"));
    }

    [Theory]
    [InlineData("205/55R16", "205/55 R 16")]
    [InlineData("195/65R15 91H", "195/65 R 15 91H")]
    [InlineData(null, null)]
    [InlineData("bad", null)]
    public void Format_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, TireDimension.Format(input));
    }

    [Theory]
    [InlineData("  x  ", "x")]
    [InlineData("nope", "nope")]
    public void Format_WithFallback_ReturnsTrimmedInput_WhenInvalid(string input, string expected)
    {
        Assert.Equal(expected, TireDimension.Format(input, fallbackToTrimmedInputWhenInvalid: true));
    }

    [Theory]
    [InlineData("205/55R16", "205/55R16")]
    [InlineData("205/55 R 16", "205/55R16")]
    [InlineData("195/65R15 91H", "195/65R15 91H")]
    [InlineData(null, null)]
    public void Normalize_ReturnsExpected(string? input, string? expected)
    {
        Assert.Equal(expected, TireDimension.Normalize(input));
    }

    [Theory]
    [InlineData("205/55R16", true)]
    [InlineData("205/55 R 16", false)]
    [InlineData("195/65R15 91H", true)]
    [InlineData("195/65 R 15 91H", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsNormalized_ReturnsExpected(string? input, bool expected)
    {
        Assert.Equal(expected, TireDimension.IsNormalized(input));
    }

    [Fact]
    public void ToMaskedString_MasksCoreAndOptionalSuffix()
    {
        var a = TireDimension.Parse("205/55R16");
        Assert.Equal("***/**R**", a.ToMaskedString());

        var b = TireDimension.Parse("195/65R15 91H");
        Assert.Equal("***/**R******", b.ToMaskedString());
    }

    [Fact]
    public void FindCandidatesInText_FindsDimensions_WithHighConfidence()
    {
        const string text = "Vi monterade 205/55R16 och sedan 225/45 R 17 på bilen.";
        var candidates = TireDimension.FindCandidatesInText(text);

        Assert.Equal(2, candidates.Count);
        foreach (var candidate in candidates)
        {
            Assert.Equal(TextCandidateCategory.Vehicle, candidate.Category);
            Assert.Equal(TextMatchConfidence.High, candidate.Confidence);
            Assert.Equal(nameof(TireDimension), candidate.TypeName);
        }

        Assert.Equal("205/55R16", candidates[0].Value.Value);
        Assert.Equal("225/45R17", candidates[1].Value.Value);
        Assert.True(candidates[0].StartIndex > 0);
    }

    [Fact]
    public void FindCandidatesInText_IncludesLoadIndexAndSpeed_WhenPresent()
    {
        const string text = "Däck: 195/65R15 91H enligt spec.";
        var candidates = TireDimension.FindCandidatesInText(text);

        Assert.Single(candidates);
        var c = candidates[0];
        Assert.Equal(91, c.Value.LoadIndex);
        Assert.Equal('H', c.Value.SpeedRating);
        Assert.Equal("195/65R15 91H", c.Value.Value);
        Assert.Equal("195/65R15 91H", c.OriginalText.Trim());
    }

    [Fact]
    public void FindCandidatesInText_DoesNotMatchInsideLongerDigitRun()
    {
        const string text = "id1205/55R16x";
        var candidates = TireDimension.FindCandidatesInText(text);

        Assert.Empty(candidates);
    }

    [Fact]
    public void FindCandidatesInText_ReturnsEmpty_ForNullOrEmpty()
    {
        Assert.Empty(TireDimension.FindCandidatesInText(""));
        Assert.Empty(TireDimension.FindCandidatesInText(string.Empty));
    }

    [Fact]
    public void Equality_SameDimension()
    {
        var a = TireDimension.Parse("205/55R16");
        var b = TireDimension.Parse("205/55 R 16");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentDimensions()
    {
        var a = TireDimension.Parse("205/55R16");
        var b = TireDimension.Parse("225/45R17");
        Assert.True(a != b);
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = TireDimension.Parse("195/65R15");
        var b = TireDimension.Parse("225/45R17");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var a = TireDimension.Parse("205/55R16");
        Assert.Equal(1, a.CompareTo(null));
    }

    [Theory]
    [InlineData("385/65R22,5", 385, 65, 'R', 22.5, null, null, null, false)]
    [InlineData("315/70R22,5 154/150L", 315, 70, 'R', 22.5, 154, 150, 'L', false)]
    [InlineData("315/80R22,5 156K", 315, 80, 'R', 22.5, 156, null, 'K', false)]
    [InlineData("385/55R22,5 160", 385, 55, 'R', 22.5, 160, null, null, false)]
    [InlineData("225/65R16C 112/110R", 225, 65, 'R', 16.0, 112, 110, 'R', true)]
    public void TryParse_ReturnsExpectedProperties_ForTruckTires(
        string input, int width, int aspect, char construction, double rim,
        int? load, int? dualLoad, char? speed, bool isCommercial)
    {
        var ok = TireDimension.TryParse(input, out var dim);

        Assert.True(ok);
        Assert.NotNull(dim);
        Assert.Equal(width, dim.WidthMm);
        Assert.Equal(aspect, dim.AspectRatio);
        Assert.Equal(construction, dim.Construction);
        Assert.Equal((decimal)rim, dim.RimDiameterInches);
        Assert.Equal(load, dim.LoadIndex);
        Assert.Equal(dualLoad, dim.DualLoadIndex);
        Assert.Equal(speed, dim.SpeedRating);
        Assert.Equal(isCommercial, dim.IsCommercial);
    }

    [Theory]
    [InlineData("385/65R22,5", "385/65 R 22.5")]
    [InlineData("315/70R22,5 154/150L", "315/70 R 22.5 154/150L")]
    [InlineData("225/65R16C 112/110R", "225/65 R 16 C 112/110R")]
    [InlineData("385/55R22,5 160", "385/55 R 22.5 160")]
    public void Format_ReturnsExpected_ForTruckTires(string input, string expected)
    {
        Assert.Equal(expected, TireDimension.Format(input));
    }

    [Theory]
    [InlineData("385/65R22,5", "385/65R22.5")]
    [InlineData("315/70R22,5 154/150L", "315/70R22.5 154/150L")]
    [InlineData("225/65R16C 112/110R", "225/65R16C 112/110R")]
    [InlineData("385/55R22,5 160", "385/55R22.5 160")]
    public void Normalize_ReturnsExpected_ForTruckTires(string input, string expected)
    {
        Assert.Equal(expected, TireDimension.Normalize(input));
    }
}
